using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using xyz_university_payment_api.Data;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Models;
using xyz_university_payment_api.Interfaces;
using Serilog;
using Serilog.Events;
using MassTransit;


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

    // Register Repositories
    builder.Services.AddScoped<IStudentRepository, StudentRepository>();
    builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

    // Register Services
    builder.Services.AddScoped<IStudentService, StudentService>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<ILoggingService, LoggingService>();
    builder.Services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();


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
    builder.Services.AddControllers();

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
            Description = "JWT Authorization header using the Bearer scheme Example:  \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
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
            new string[] {}
        }
    });
    });

   
    // Add authentication
   builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["IdentityServer:Authority"] ?? "http://localhost:5153"; // IdentityServer URL
        options.RequireHttpsMetadata = false;
        options.Audience = "xyz_api";
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost:5153",
            ValidAudience = "xyz_api"

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

    // Ensure database is created
   // Only seed if NOT running from EF CLI
if (!AppDomain.CurrentDomain.FriendlyName.Contains("ef"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            Log.Information("Ensuring database is created");
            // context.Database.Migrate();
            Log.Information("Database created successfully");

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
        app.UseSwaggerUI();
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
