using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using System.Collections.Generic;
using System.Security.Claims;

public static class Config
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("xyz_api", "XYZ University API")
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("xyz_api", "XYZ University API")
            {
                Scopes = { "xyz_api" }
            }
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "xyz_client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("super_secret_password".Sha256()) },
                AllowedScopes = { "xyz_api" },
                RequireClientSecret = false,
                AllowedCorsOrigins = { "http://localhost:5173" }
            }
        };

    public static List<TestUser> Users =>
        new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "1",
                Username = "admin",
                Password = "Admin123!",
                Claims = new List<Claim>
                {
                    new Claim("name", "Admin User"),
                    new Claim("email", "admin@xyz-university.com"),
                    new Claim("role", "admin"),
                    new Claim("department", "IT")
                }
            },
            new TestUser
            {
                SubjectId = "2",
                Username = "finance",
                Password = "Finance123!",
                Claims = new List<Claim>
                {
                    new Claim("name", "Finance Manager"),
                    new Claim("email", "finance@xyz-university.com"),
                    new Claim("role", "finance"),
                    new Claim("department", "Finance")
                }
            },
            new TestUser
            {
                SubjectId = "3",
                Username = "registrar",
                Password = "Registrar123!",
                Claims = new List<Claim>
                {
                    new Claim("name", "Registrar Officer"),
                    new Claim("email", "registrar@xyz-university.com"),
                    new Claim("role", "registrar"),
                    new Claim("department", "Registrar")
                }
            },
            new TestUser
            {
                SubjectId = "4",
                Username = "student",
                Password = "Student123!",
                Claims = new List<Claim>
                {
                    new Claim("name", "John Doe"),
                    new Claim("email", "john.doe@student.xyz-university.com"),
                    new Claim("role", "student"),
                    new Claim("student_number", "S12345"),
                    new Claim("program", "Computer Science")
                }
            },
            new TestUser
            {
                SubjectId = "5",
                Username = "viewer",
                Password = "Viewer123!",
                Claims = new List<Claim>
                {
                    new Claim("name", "View Only User"),
                    new Claim("email", "viewer@xyz-university.com"),
                    new Claim("role", "viewer"),
                    new Claim("department", "General")
                }
            }
        };
}
