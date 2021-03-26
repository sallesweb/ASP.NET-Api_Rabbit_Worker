namespace Api
{
    using Core.Factories;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;

    public class Startup
    {
        private IWebHostEnvironment CurrentEnvironment { get; set; }

        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment currentEnvironment)
        {
            this.Configuration = configuration;
            this.CurrentEnvironment = currentEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var env = CurrentEnvironment.EnvironmentName;
            var builder = new ConfigurationBuilder()
                .SetBasePath(CurrentEnvironment.ContentRootPath)
                .AddJsonFile($"appsettings{env}.json", optional: true, reloadOnChange: true);

            services.AddSingleton<IConnectionRabbitFactory, ConnectionRabbitFactory>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
