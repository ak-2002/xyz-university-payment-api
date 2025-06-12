using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using xyz_university_payment_api.Data;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Models;


var builder = WebApplication.CreateBuilder(args);

// Register Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("XYZUniversityDB")); // You can switch to SQL later




// Register Services
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<PaymentService>();

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "XYZ University Payment API", Version = "v1" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Seed sample students if none exist
    if (!context.Students.Any())
    {
        context.Students.AddRange(
            new Student { StudentNumber = "S12345", FullName = "John Doe", Program = "CS", IsActive = true },
            new Student { StudentNumber = "S67890", FullName = "Jane Smith", Program = "IT", IsActive = true },
            new Student { StudentNumber = "S66001", FullName = "Alex Mutahi", Program = "ACC", IsActive = true },
            new Student { StudentNumber = "S66002", FullName = "Janet Mwangi", Program = "SOCIOLOGY", IsActive = true },
            new Student { StudentNumber = "S66003", FullName = "Jane Smith", Program = "IT", IsActive = false }
        );
        context.SaveChanges();
    }

    if (!context.PaymentNotifications.Any())
{
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
}

}




// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Optional: Base Route to Avoid 404 at Root
app.MapGet("/", () => "Welcome to XYZ University Payment API");

app.Run();
