using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Traffic.API.APIServices;
using Traffic.API.Hubs;
using Traffic.API.Services;
using Traffic.Application.Services;
using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Abstractions.Services;
using Traffic.Data;
using Traffic.Data.Repositories;

namespace Traffic.API
{
    public class Program
    {
        private const string DEFAULT_CONNECTION_STRING = "Host=localhost;Port=5432;Database=postgres_traffic;Username=traffic_user;Password=postgres";
        private static readonly bool _printAuth = false;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
                ?? DEFAULT_CONNECTION_STRING;
            var httpPort = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS") ?? "7002";
            var httpsPort = Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORTS") ?? "7003";

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(int.Parse(httpPort), options =>
                {
                    Console.WriteLine($"HTTP server listening on port {httpPort}");
                });

                serverOptions.ListenAnyIP(int.Parse(httpsPort), listenOptions =>
                {
                    listenOptions.UseHttps();
                    Console.WriteLine($"HTTPS server listening on port {httpsPort}");
                });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();

            ConfigureCors(builder);

            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            })
                .AddJsonProtocol();

            builder.Services.AddSingleton(connectionString);
            builder.Services.AddSingleton<ITransportDataService, TransportDataService>();
            builder.Services.AddSingleton<TransportClientHub>();
            builder.Services.AddSingleton<TransportAPIHub>();

            builder.Services.AddPostgresDatabase(options =>
            {
                options.WithConnectionString(connectionString)
                    .WithInitializeDB(bool.Parse(builder.Configuration["InitDB"] ?? "false"));
            });

            ConfigureAuthorization(builder);
            ConfigureRepositories(builder);
            ConfigureRepositoryServices(builder);
            builder.Services.AddScoped<IMapSerializeService, MapSerializeService>();
            builder.Services.AddScoped<ITransportHttpConnection, TransportHttpConnection>();

            builder.Services.AddHostedService<TransportCoordinatorService>();
            builder.Services.AddHostedService<TransportResponseService>();

            builder.Services.AddAuthorization();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.Use(ConfigureWebSocket);
            app.UseCors("AllowSpecificOrigins");
            app.Lifetime.ApplicationStarted.Register(() =>
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Application started");
                logger.LogInformation("HTTP port: {HttpPort}", httpPort);
                logger.LogInformation("HTTPS port: {HttpsPort}", httpsPort);
                logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
            });

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.MapHub<TransportClientHub>("/transportClientHub")
                .RequireAuthorization();
            app.MapHub<TransportAPIHub>("/transportAPIHub");

            app.Run();
        }

        private static async Task ConfigureWebSocket(HttpContext context, Func<Task> next)
        {
            if (context.Request.Path.StartsWithSegments("/transportClientHub") && _printAuth)
            {
                Console.WriteLine($"Hub request: {context.Request.Path}");
                Console.WriteLine($"Query string: {context.Request.QueryString}");

                var tokenFromQuery = context.Request.Query["access_token"];
                var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

                Console.WriteLine($"Token from query: {!string.IsNullOrEmpty(tokenFromQuery)}");
                Console.WriteLine($"Auth header: {!string.IsNullOrEmpty(authHeader)}");
            }

            await next();
        }

        private static void ConfigureCors(WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    policy.WithOrigins(
                            "https://localhost:7003",
                            "http://localhost:3000",
                            "http://127.0.0.1:3000",
                            "http://localhost:3001",
                            "http://127.0.0.1:3001",
                            "null"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetIsOriginAllowed(_ => true);
                });
            });
        }

        private static void ConfigureRepositories(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPointRepository, PointRepository>();
            builder.Services.AddScoped<IMapRepository, MapRepository>();
            builder.Services.AddScoped<ITransportRepository, TransportRepository>();
            builder.Services.AddScoped<IRouteRepository, RouteRepository>();
        }

        private static void ConfigureRepositoryServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPointService, PointService>();
            builder.Services.AddScoped<IMapService, MapService>();
            builder.Services.AddScoped<ITransportService, TransportService>();
            builder.Services.AddScoped<IRouteService, RouteService>();
        }

        private static void ConfigureAuthorization(WebApplicationBuilder builder)
        {
            var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]!);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                            if (_printAuth)
                            {
                                Console.WriteLine($"Token from query: {accessToken.ToString()[..20]}...");
                            }
                        }
                        else if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                        {
                            context.Token = authHeader["Bearer ".Length..];
                            if (_printAuth)
                            {
                                Console.WriteLine($"Token from header: {context.Token}...");
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        if (_printAuth)
                        {
                            Console.WriteLine($"Token validated for: {context.Principal?.Identity?.Name}");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}