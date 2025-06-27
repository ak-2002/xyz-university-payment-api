using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using xyz_university_payment_api.Data;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Models;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Filters;
using Serilog;
using Serilog.Events;
using MassTransit;
using AutoMapper;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.Text;


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

    // Register Unit of Work and Generic Repository
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Register Services
    builder.Services.AddScoped<IStudentService, StudentService>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<ILoggingService, LoggingService>();
    builder.Services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();

    // Register Authorization Services
    builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

    // Add AutoMapper
    builder.Services.AddAutoMapper(typeof(Program).Assembly);

    // Add FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    //Add MassTransit for RabbitMQ messaging
    builder.Services.AddMassTransit(x =>
    {

        //Register Consumers
        x.AddConsumer<PaymentProcessedMessageConsumer>();
        x.AddConsumer<PaymentFailedMessageConsumer>();
        x.AddConsumer<PaymentValidationMessageConsumer>();
        
        x.UsingRabbitMq((
            context, cfg ) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
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
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "XYZ University Payment API", Version = "v1" });

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
    });

   
    // Add authentication with custom JWT implementation
   builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
            var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-with-at-least-32-characters";
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
    });

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
                Log.Error(ex, "An error occurred while setting up the database");
                throw;
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
            c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
            c.DocumentTitle = "XYZ University Payment API Documentation";
            c.DefaultModelsExpandDepth(-1); // Hide schemas section
        });
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

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
