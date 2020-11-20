using git_webhook_server.Models;
using git_webhook_server.Services;
using git_webhook_server.Services.EventLogService;
using git_webhook_server.Services.EventProcessors;
using git_webhook_server.Services.ProcessExecutor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace git_webhook_server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContextFactory<DatabaseContext>(options => options.UseSqlite("Data Source=data.db;"));

            services.Configure<WebHookOptions>(Configuration);
            services.Configure<SecretOptions>(Configuration);

            services.AddScoped<IRuleMatcher, RuleMatcher>();
            services.AddScoped<IProcessExecutor, ProcessExecutor>();
            services.AddScoped<IPushEventProcessor, PushEventProcessor>();
            services.AddScoped<IEventLogService, EventLogService>();

            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
