Traffic Now
  The Traffic Now application consists of 4 main modules:
1. Backend is the application server that users and transports connect to. The task is to store and process information.
2. Frontend is the client part of the application. The task is to display information and a convenient place of work for users.
3. Transport.API - API For a separate transport. The task is to simulate traffic and send the data to the server.
4. Admin cmd - A panel for the work of the administrator. The task is to manage protected data.
  Setup and launch:
1. Install: .NET, Postgres (or we install a Docker container), Node.js.
2. Build the server using .NET.
3. Configure the build (aokp settings.json), namely: the DB connection string ("ConnectionStrings") and "InitDB" are true (for the first run).
4. We start the server, then the client (using npm start). The website runs on port 3000.
  Deploys:
https://traffic-now-transport.onrender.com - API for transport
https://traffic-now.onrender.com - the server
https://traffic-now-front.onrender.com - frontend