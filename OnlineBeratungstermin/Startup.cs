using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OnlineBeratungstermin.Helpers;
using System.Globalization;

namespace OnlineBeratungstermin
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add DbContext
            services.AddDbContext<OnlineTermineDbContext>(options =>
                options.UseSqlServer(_configuration.GetConnectionString("OnlineTermineDatabase")));

           
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(new FileLoggerProvider("Logs"));
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
                logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
                logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.None);
                logging.AddFilter("Microsoft.AspNetCore.Hosting.Internal.WebHost", LogLevel.None);
                logging.AddFilter("Microsoft.Extensions.Hosting.Internal.Host", LogLevel.None);
                logging.AddFilter("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", LogLevel.None);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Model.Validation", LogLevel.None);
            });

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Online Beratungs Termin", Version = "v1" });
            });


        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, OnlineTermineDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Beratungs Termin v1");
            });

            // Set culture to German (de)
            CultureInfo germanCulture = new CultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentCulture = germanCulture;
            CultureInfo.DefaultThreadCurrentUICulture = germanCulture;


            // Apply database migrations
            dbContext.Database.Migrate();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}