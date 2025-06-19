var builder = WebApplication.CreateBuilder(args);

// Add IdentityServer
builder.Services.AddIdentityServer()
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryApiResources(Config.ApiResources)
    .AddInMemoryClients(Config.Clients)
    .AddDeveloperSigningCredential(); // For development only

var app = builder.Build();

app.UseIdentityServer();

app.MapGet("/", () => "Identity Server is running...");

app.Run();
