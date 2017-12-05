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
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Gateway.Services;
using Gateway.Services.Implementations;
using Gateway.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IdentityServer4.AccessTokenValidation;
using Gateway.CustomAuthorization;
using Microsoft.EntityFrameworkCore;

namespace Gateway
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

            services.AddTransient<IAccountsService, AccountsService>();
            services.AddTransient<ISubscriptionsService, SubscriptionsService>();
            services.AddTransient<INewsService, NewsService>();
            services.AddTransient<GatewayController>();

            services.AddSingleton<TokensStore>();
            services.AddLogging(lb => lb.AddConsole());
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(o =>
            {
                o.Authority = "https://auth.loc";
                o.RequireHttpsMetadata = false;
                o.ApiName = "api";
                o.JwtValidationClockSkew = TimeSpan.FromSeconds(0);
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowAnyHeader());
            });
            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("AllowAll");
            app.UseMiddleware<GatewayCustomAuthorizationMiddleware>();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvc();
            
        }
    }
}
