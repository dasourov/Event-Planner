// Load environment variables from .env before building distributed application
EnvLoader.Load();

var builder = DistributedApplication.CreateBuilder(args);

// Retrieve MongoDB Connection String from Environment
var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
IResourceBuilder<IResourceWithConnectionString> db;

if (!string.IsNullOrEmpty(connectionString))
{
    // Use the remote database connection string directly (e.g. MongoDB Atlas)
    //db = builder.AddConnectionString("mongodb", "MONGODB_CONNECTION_STRING");
    
    // Make the value visible to Aspire's connection-string resolver
    builder.Configuration["ConnectionStrings:mongodb"] = connectionString;
    db = builder.AddConnectionString("mongodb");
}
else
{
    // Fallback: Spin up a local MongoDB Container resource
    //var mongodb = builder.AddMongoDB("mongodb");
    //db = mongodb.AddDatabase("gather");

     // Fallback: spin up a local container (needs Docker)
     var mongodb = builder.AddMongoDB("mongodb")
         .WithDataVolume()
         .WithEndpoint(port: 27017, targetPort: 27017);
     db = mongodb.AddDatabase("gather");
}

// Pass configuration to the server backend
var server = builder.AddProject<Projects.EventPlanner_Server>("server")
    .WithReference(db)
    .WaitFor(db)
    .WithEnvironment("Jwt__Secret", Environment.GetEnvironmentVariable("JWT_SECRET") ?? "super_secret_key_that_is_at_least_32_characters_long_12345!")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(server);
server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();

// Robust helper loader to parse local .env files
public static class EnvLoader
{
    public static void Load()
    {
        var dir = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
        while (dir != null)
        {
            var envFile = System.IO.Path.Combine(dir.FullName, ".env");
            if (System.IO.File.Exists(envFile))
            {
                foreach (var line in System.IO.File.ReadAllLines(envFile))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var val = parts[1].Trim().Trim('"').Trim('\'');
                        System.Environment.SetEnvironmentVariable(key, val);
                    }
                }
                break;
            }
            dir = dir.Parent;
        }
    }
}
