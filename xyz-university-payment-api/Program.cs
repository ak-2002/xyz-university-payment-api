using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using xyz_university_payment_api.Infrastructure.Data;
using xyz_university_payment_api.Core.Application.Services;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Presentation.Filters;
using xyz_university_payment_api.Infrastructure.External.Caching;
using xyz_university_payment_api.Core.Shared.Constants;
using Serilog;
using Serilog.Events;
using MassTransit;
using AutoMapper;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting XYZ University Payment API");

    // Early exit for EF commands
    var commandArgs = Environment.GetCommandLineArgs();
    var isEfCommand = commandArgs.Any(arg =>
        arg.Contains("ef") ||
        arg.Contains("migrations") ||
        arg.Contains("database") ||
        arg.Contains("dotnet"));

    if (isEfCommand)
    {
        Log.Information("EF command detected, skipping application startup");
        return;
    }

    var builder = WebApplication.CreateBuilder(args);



    // Configure Serilog for the application
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/xyz-university-api-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
            rollOnFileSizeLimit: true)
        .WriteTo.File("Logs/xyz-university-api-errors-.log",
            restrictedToMinimumLevel: LogEventLevel.Error,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
            rollOnFileSizeLimit: true));

    // Register Database Context
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Configure In-Memory Cache (temporary replacement for Redis)
    builder.Services.AddDistributedMemoryCache();
    
    // Uncomment the following lines when Redis is available:
    // builder.Services.AddStackExchangeRedisCache(options =>
    // {
    //     options.Configuration = builder.Configuration.GetConnectionString("Redis");
    //     options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "xyz-university-api:";
    // });

    // Configure Redis settings
    builder.Services.Configure<xyz_university_payment_api.Infrastructure.External.Caching.RedisConfig>(builder.Configuration.GetSection("Redis"));

    // Configure API Versioning
    builder.Services.Configure<xyz_university_payment_api.Core.Shared.Constants.ApiVersionConfig>(builder.Configuration.GetSection("ApiVersioning"));

    // Register Cache Service
    builder.Services.AddScoped<ICacheService, xyz_university_payment_api.Core.Application.Services.CacheService>();

    // Register API Version Service
    builder.Services.AddScoped<xyz_university_payment_api.Core.Application.Services.ApiVersionService>();

    // Register Unit of Work and Generic Repository
    builder.Services.AddScoped<IUnitOfWork, xyz_university_payment_api.Infrastructure.Data.UnitOfWork>();

    // Register Services
    builder.Services.AddScoped<IStudentService, xyz_university_payment_api.Core.Application.Services.StudentService>();
    builder.Services.AddScoped<IPaymentService, xyz_university_payment_api.Core.Application.Services.PaymentService>();
    builder.Services.AddScoped<ILoggingService, xyz_university_payment_api.Core.Application.Services.LoggingService>();
    builder.Services.AddScoped<IMessagePublisher, xyz_university_payment_api.Core.Application.Services.RabbitMQMessagePublisher>();

    // Register Authorization Services
    builder.Services.AddScoped<IAuthorizationService, xyz_university_payment_api.Core.Application.Services.AuthorizationService>();
    builder.Services.AddScoped<IJwtTokenService, xyz_university_payment_api.Core.Application.Services.JwtTokenService>();

    // Add HttpContextAccessor for accessing current user context in services
    builder.Services.AddHttpContextAccessor();

    // Add AutoMapper
    builder.Services.AddAutoMapper(typeof(Program).Assembly);

    // Add FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    //Add MassTransit for RabbitMQ messaging
    builder.Services.AddMassTransit(x =>
    {

        //Register Consumers
        x.AddConsumer<xyz_university_payment_api.Core.Application.Services.PaymentProcessedMessageConsumer>();
        x.AddConsumer<xyz_university_payment_api.Core.Application.Services.PaymentFailedMessageConsumer>();
        x.AddConsumer<xyz_university_payment_api.Core.Application.Services.PaymentValidationMessageConsumer>();

        x.UsingRabbitMq((
            context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context);
            });
    });


    // Add Controllers
    builder.Services.AddControllers(options =>
    {
        // Add global exception filter
        options.Filters.Add<GlobalExceptionFilter>();
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add Swagger
    builder.Services.AddEndpointsApiExplorer();
    
    // Configure Swagger with API versioning
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "XYZ University Payment API v1", Version = "v1" });
        c.SwaggerDoc("v2", new OpenApiInfo { Title = "XYZ University Payment API v2", Version = "v2" });
        c.SwaggerDoc("v3", new OpenApiInfo { Title = "XYZ University Payment API v3", Version = "v3" });
        
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Include XML comments if available
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    // Add authentication with custom JWT implementation
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured. Please set the Jwt:Key configuration value.");
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "xyz-university";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "xyz-api";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

    // Add Authorization
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ApiScope", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("scope", "xyz_api");
        });

        // Add custom authorization policies
        options.AddActiveUserPolicy();
        options.AddAdminPolicy();
        options.AddPaymentPolicy();
        options.AddStudentPolicy();
        options.AddUserManagementPolicy();
        options.AddReadOnlyPolicy();
        options.AddWritePolicy();
        options.AddDeletePolicy();
        options.AddFinancialPolicy();
        options.AddReportingPolicy();
    });

    // Add API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(3, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = Microsoft.AspNetCore.Mvc.Versioning.ApiVersionReader.Combine(
            new Microsoft.AspNetCore.Mvc.Versioning.UrlSegmentApiVersionReader(),
            new Microsoft.AspNetCore.Mvc.Versioning.HeaderApiVersionReader("X-API-Version"),
            new Microsoft.AspNetCore.Mvc.Versioning.QueryStringApiVersionReader("api-version")
        );
    });

    // Add API Versioning Explorer for Swagger
    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Swagger configuration is now inline

    var app = builder.Build();

    // Ensure database is created and seeded
    // Only run seeding if NOT running from EF CLI or dotnet commands
    var isDesignTime = Environment.GetEnvironmentVariable("EF_DESIGN_TIME") == "true" ||
                      Environment.GetEnvironmentVariable("DOTNET_EF_TOOLS_RUNNING") == "true";

    if (!isDesignTime)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

            try
            {
                Log.Information("Ensuring database is created");
                context.Database.Migrate();
                Log.Information("Database created successfully");

                // Seed default roles and permissions
                Log.Information("Seeding default roles and permissions");
                await authorizationService.SeedDefaultRolesAndPermissionsAsync();
                Log.Information("Default roles and permissions seeded successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while setting up the database");
                // Don't throw here - let the application start even if seeding fails
                // The user can manually trigger seeding via API endpoints
            }

            try
            {
                if (!context.Students.Any())
                {
                    Log.Information("Seeding initial student data");
                    context.Students.AddRange(
                        new Student { StudentNumber = "S12345", FullName = "John Doe", Program = "CS", IsActive = true },
                        new Student { StudentNumber = "S67890", FullName = "Jane Smith", Program = "IT", IsActive = true },
                        new Student { StudentNumber = "S66001", FullName = "Alex Mutahi", Program = "ACC", IsActive = true },
                        new Student { StudentNumber = "S66002", FullName = "Janet Mwangi", Program = "SOCIOLOGY", IsActive = true },
                        new Student { StudentNumber = "S66003", FullName = "Jane Smith", Program = "IT", IsActive = false },
                        // Add more students to match the dashboard controller mappings
                        new Student { StudentNumber = "S66004", FullName = "John Doe", Program = "Computer Science", Email = "john.student@xyzuniversity.edu", IsActive = true },
                        new Student { StudentNumber = "S66005", FullName = "Sarah Johnson", Program = "Business Administration", Email = "sarah.student@xyzuniversity.edu", IsActive = true },
                        new Student { StudentNumber = "S66006", FullName = "Mike Wilson", Program = "Engineering", Email = "mike.student@xyzuniversity.edu", IsActive = true },
                        new Student { StudentNumber = "S66007", FullName = "Andrew Smith", Program = "Computer Science", Email = "andrew.student@xyzuniversity.edu", IsActive = true }
                    );
                    context.SaveChanges();
                    Log.Information("Student data seeded successfully");
                }

                if (!context.PaymentNotifications.Any())
                {
                    Log.Information("Seeding initial payment data");
                    context.PaymentNotifications.AddRange(
                        new PaymentNotification
                        {
                            StudentNumber = "S12345",
                            PaymentReference = "REF001",
                            AmountPaid = 5000m,
                            PaymentDate = DateTime.UtcNow.AddDays(-3),
                            DateReceived = DateTime.UtcNow.AddDays(-2)
                        },
                        new PaymentNotification
                        {
                            StudentNumber = "S67890",
                            PaymentReference = "REF002",
                            AmountPaid = 3500m,
                            PaymentDate = DateTime.UtcNow.AddDays(-1),
                            DateReceived = DateTime.UtcNow
                        },
                        new PaymentNotification
                        {
                            StudentNumber = "S66001",
                            PaymentReference = "REF003",
                            AmountPaid = 4200m,
                            PaymentDate = DateTime.UtcNow.AddDays(-5),
                            DateReceived = DateTime.UtcNow.AddDays(-4)
                        }
                    );
                    context.SaveChanges();
                    Log.Information("Payment data seeded successfully");
                }

                // Create student user account for testing
                if (!context.Users.Any(u => u.Username == "alex.student"))
                {
                    Log.Information("Creating student user account for Alex Mutahi");
                    
                    // Create the user
                    var studentUser = new User
                    {
                        Username = "alex.student",
                        Email = "alex.mutahi@student.xyzuniversity.edu",
                        PasswordHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes("Student123!"))),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    context.Users.Add(studentUser);
                    await context.SaveChangesAsync();
                    
                    // Get the created user and assign Student role
                    var createdUser = await context.Users.FirstAsync(u => u.Username == "alex.student");
                    var studentRole = await context.Roles.FirstAsync(r => r.Name == "Student");
                    
                    // Assign role to user
                    var userRole = new UserRole
                    {
                        UserId = createdUser.Id,
                        RoleId = studentRole.Id,
                        AssignedAt = DateTime.UtcNow,
                        AssignedBy = "System"
                    };
                    
                    context.UserRoles.Add(userRole);
                    await context.SaveChangesAsync();
                    
                    Log.Information("Created student user account: {Username} (Password: {Password}) linked to Alex Mutahi (S66001)", 
                        studentUser.Username, "Student123!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while seeding sample data");
                // Don't throw here - let the application start even if seeding fails
            }
        }
    }


    // Configure Middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "XYZ University Payment API v1");
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "XYZ University Payment API v2");
            c.SwaggerEndpoint("/swagger/v3/swagger.json", "XYZ University Payment API v3");
            
            c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
            c.DocumentTitle = "XYZ University Payment API Documentation";
            c.DefaultModelsExpandDepth(-1); // Hide schemas section
            c.DisplayRequestDuration();
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
        });
    }

    // app.UseHttpsRedirection(); // Commented out to allow HTTP requests

    app.UseCors("AllowAll");

    // Add API Versioning Middleware - MUST be before routing
    app.UseApiVersioning();

    app.UseRouting();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    // Optional: Base Route to Avoid 404 at Root
    app.MapGet("/", () => "Welcome to XYZ University Payment API");

    Log.Information("XYZ University Payment API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
