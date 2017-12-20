using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Statistics.EventBus;
using Statistics.RabbitHelper;
using Statistics.Events;
using Statistics.EventHandlers;
using Microsoft.EntityFrameworkCore;
using Statistics.EventsStorage;

namespace Statistics
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
            services.AddMvc();

            services.AddLogging(ops => ops.SetMinimumLevel(LogLevel.Critical));
            services.AddDbContext<AppDbContext>(ops =>
                ops.UseInMemoryDatabase("Statistics"));
            services.AddSingleton<IRabbitPersistentConnection, RabbitPersistentConnection>();
            services.AddSingleton<IEventStorage, EventStorage>();
            services.AddSingleton<IEventBus, RabbitEventBus>();
            services.AddSingleton<IEventHandler, AddGoodsEventHadler>();
            services.AddSingleton<IEventHandler, AddOrderEventHandler>();
            services.AddSingleton<IEventHandler, AddPayEventHandler>();
            services.AddSingleton<IEventHandler, DeleteGoodsEventHandler>();
            services.AddSingleton<IEventHandler, DeleteOrderEventHandler>();
            services.AddSingleton<IEventHandler, LoginEventHandler>();
            services.AddSingleton<IEventHandler, RequestEventHandler>();
            services.AddTransient<DbProxy>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.ApplicationServices.GetServices<IEventHandler>();
            app.UseMvc();
        }
    }
}
