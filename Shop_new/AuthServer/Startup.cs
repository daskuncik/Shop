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
using Microsoft.EntityFrameworkCore;
using AuthServer.Entities;


namespace AuthServer
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

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                //.AddTestUsers(Config.GetUsers())
                .AddProfileService<ProfileService>();

            services.AddTransient<ProfileService>();


            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("Auth"));
            services.AddDbContext<UserContext>(options =>
                options.UseInMemoryDatabase("Users"));
            services.AddDbContext<TokenDbContext>(options =>
                options.UseInMemoryDatabase("Tokenss"));



            services.AddTransient<TokenStore>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseCors("AllowAll");
            app.UseIdentityServer();
            app.UseMvc();
        }
    }
}
