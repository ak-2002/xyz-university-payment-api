var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add IdentityServer
builder.Services.AddIdentityServer()
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryApiResources(Config.ApiResources)
    .AddInMemoryClients(Config.Clients)
    .AddTestUsers(Config.Users)
    .AddDeveloperSigningCredential(); // For development only

var app = builder.Build();

// Use CORS
app.UseCors("AllowFrontend");

app.UseIdentityServer();

// Add OpenID Connect discovery endpoints
app.MapGet("/.well-known/openid_configuration", (HttpContext context) => new
{
    issuer = "http://localhost:5153",
    jwks_uri = "http://localhost:5153/.well-known/jwks",
    token_endpoint = "http://localhost:5153/connect/token",
    scopes_supported = new[] { "xyz_api" },
    claims_supported = new[] { "sub", "scope", "name", "email", "role" },
    grant_types_supported = new[] { "password", "client_credentials" },
    token_endpoint_auth_methods_supported = new[] { "client_secret_post" }
});

// Get the actual signing credentials from IdentityServer
app.MapGet("/.well-known/jwks", async (HttpContext context) =>
{
    try
    {
        var keyMaterialService = context.RequestServices.GetRequiredService<Duende.IdentityServer.Services.IKeyMaterialService>();
        var signingCredentials = await keyMaterialService.GetSigningCredentialsAsync();
        
        var key = signingCredentials.Key as Microsoft.IdentityModel.Tokens.RsaSecurityKey;
        if (key?.Rsa != null)
        {
            var rsa = key.Rsa;
            var parameters = rsa.ExportParameters(false);
            
            return Results.Json(new
            {
                keys = new[]
                {
                    new
                    {
                        kty = "RSA",
                        use = "sig",
                        kid = "default",
                        e = Convert.ToBase64String(parameters.Exponent ?? Array.Empty<byte>()),
                        n = Convert.ToBase64String(parameters.Modulus ?? Array.Empty<byte>())
                    }
                }
            });
        }
    }
    catch (Exception ex)
    {
        // Log the exception if needed
        Console.WriteLine($"Error generating JWKS: {ex.Message}");
    }
    
    return Results.Json(new { keys = new object[] { } });
});

app.MapGet("/", () => "Identity Server is running...");

app.Run();
