// Purpose: Configures services, database connection, and HTTP request pipeline
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using xyz_university_payment_api.Data;
using xyz_university_payment_api.Services;
using xyz_university_payment_api.Middleware;

namespace xyz_university_payment_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Registers services, database, and Swagger for API documentation
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<StudentService>();
            services.AddScoped<PaymentService>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        // Configures middleware pipeline: Swagger, Routing, and Controller Mapping
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //register error handling middleware
            app.UseMiddleware<xyz_university_payment_api.Middleware.ErrorHandlingMiddleware>();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
