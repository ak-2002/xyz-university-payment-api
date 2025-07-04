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

    // Configure Redis
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "xyz-university-api:";
    });

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
    builder.Services.AddSwaggerGen(c =>
    {
        // Add version-specific documents
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "XYZ University Payment API v1 (Deprecated)",
            Version = "v1",
            Description = @"## V1 API Endpoints (Deprecated - Use V2 or V3)

**⚠️ This version is deprecated and will be removed in future releases.**

### Available Endpoints:
- **Payments**: `/api/v1/payments`
  - POST `/notify` - Process payment notification
  - GET `/` - Get all payments with pagination
  - GET `/{id}` - Get payment by ID
  - GET `/student/{studentNumber}` - Get payments by student
  - GET `/student/{studentNumber}/summary` - Get payment summary

- **Students**: `/api/v1/students`
  - GET `/` - Get all students
  - GET `/{id}` - Get student by ID
  - GET `/number/{studentNumber}` - Get student by number
  - POST `/` - Create new student
  - PUT `/{id}` - Update student
  - DELETE `/{id}` - Delete student
  - GET `/active` - Get active students
  - GET `/program/{program}` - Get students by program
  - GET `/search/{searchTerm}` - Search students
  - PUT `/{id}/status` - Update student status
  - POST `/validate` - Validate student data

### Features:
- Basic CRUD operations
- Simple pagination
- Basic validation
- Standard error responses

**Migration Guide**: Please migrate to V2 or V3 for enhanced features and better performance.",
            Contact = new OpenApiContact
            {
                Name = "XYZ University API Support",
                Email = "api-support@xyz-university.com"
            }
        });

        c.SwaggerDoc("v2", new OpenApiInfo
        {
            Title = "XYZ University Payment API v2",
            Version = "v2",
            Description = @"## V2 API Endpoints (Recommended)

**🚀 Enhanced version with improved features and performance.**

### Available Endpoints:
- **Payments**: `/api/v2/payments`
  - POST `/notify` - Process payment notification (enhanced)
  - GET `/` - Get all payments with advanced pagination
  - GET `/analytics` - **NEW** Get payment analytics and statistics

- **Students**: `/api/v2/students`
  - GET `/` - Get all students with enhanced filtering
  - GET `/{id}` - Get student by ID with detailed info
  - GET `/number/{studentNumber}` - Get student by number
  - POST `/` - Create new student with enhanced validation
  - PUT `/{id}` - Update student
  - DELETE `/{id}` - Delete student
  - GET `/active` - Get active students
  - GET `/program/{program}` - Get students by program
  - GET `/search/{searchTerm}` - Advanced search with multiple criteria
  - PUT `/{id}/status` - Update student status
  - POST `/validate` - Enhanced validation
  - GET `/export` - **NEW** Export students to CSV/Excel
  - POST `/bulk-import` - **NEW** Bulk import students
  - GET `/statistics` - **NEW** Student statistics and analytics

### V2 Enhancements:
- ✅ Advanced filtering and search capabilities
- ✅ Enhanced pagination with sorting options
- ✅ Payment analytics and reporting
- ✅ Bulk operations (import/export)
- ✅ Improved error handling and validation
- ✅ Better performance and caching
- ✅ Enhanced response metadata

**Recommended for production use.**",
            Contact = new OpenApiContact
            {
                Name = "XYZ University API Support",
                Email = "api-support@xyz-university.com"
            }
        });

        c.SwaggerDoc("v3", new OpenApiInfo
        {
            Title = "XYZ University Payment API v3",
            Version = "v3",
            Description = @"## V3 API Endpoints (Latest Features)

**🔥 Latest version with cutting-edge features and real-time capabilities.**

### Available Endpoints:
- **Payments**: `/api/v3/payments`
  - POST `/notify` - Process payment notification (real-time)
  - GET `/` - Get all payments with real-time stats
  - GET `/{id}` - Get payment by ID with enhanced metadata
  - GET `/real-time-stats` - **NEW** Real-time payment statistics
  - POST `/webhook-test` - **NEW** Test webhook integration
  - GET `/advanced-analytics` - **NEW** Advanced analytics with ML insights

- **Students**: `/api/v3/students`
  - GET `/` - Get all students with real-time data
  - GET `/{id}` - Get student by ID with full profile
  - GET `/number/{studentNumber}` - Get student by number
  - POST `/` - Create new student with AI validation
  - PUT `/{id}` - Update student
  - DELETE `/{id}` - Delete student
  - GET `/active` - Get active students
  - GET `/program/{program}` - Get students by program
  - GET `/search/{searchTerm}` - AI-powered search
  - PUT `/{id}/status` - Update student status
  - POST `/validate` - AI-enhanced validation
  - GET `/export` - Export with multiple formats
  - POST `/bulk-import` - Bulk import with validation
  - GET `/statistics` - Real-time statistics
  - GET `/analytics` - **NEW** Advanced student analytics
  - POST `/bulk-operations` - **NEW** Bulk operations with progress tracking
  - GET `/download-report` - **NEW** Download detailed reports
  - GET `/predictive-insights` - **NEW** AI-powered predictive insights

### V3 Innovations:
- 🚀 Real-time processing and statistics
- 🤖 AI-powered validation and search
- 📊 Advanced analytics and ML insights
- 🔄 Webhook integration support
- 📈 Predictive analytics
- 🎯 Enhanced bulk operations
- 📱 Mobile-optimized responses
- 🔒 Advanced security features
- ⚡ Microservices-ready architecture
- 🌐 GraphQL-compatible endpoints

**Future-ready with the latest technology stack.**",
            Contact = new OpenApiContact
            {
                Name = "XYZ University API Support",
                Email = "api-support@xyz-university.com"
            }
        });

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
    /*
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
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
    */

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
                        new Student { StudentNumber = "S66003", FullName = "Jane Smith", Program = "IT", IsActive = false }
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
                        }
                    );
                    context.SaveChanges();
                    Log.Information("Payment data seeded successfully");
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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "XYZ University Payment API v1 (Deprecated)");
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

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    // Add API Versioning Middleware
    app.UseApiVersioning();

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
