using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Traffic.Data;

namespace Traffic.API
{
    public class Program
    {
        private const string DEFAULT_CONNECTION_STRING = "Host=localhost;Port=5432;Database=postgres_traffic;Username=traffic_user;Password=postgres";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
                ?? DEFAULT_CONNECTION_STRING;

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton(connectionString);

            builder.Services.AddPostgresDatabase(options =>
            {
                options.WithConnectionString(connectionString)
                    .WithInitializeDB(true);
            });

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                        ValidateIssuerSigningKey = true,
                    };
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
