using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop_new.Services;
using System.IdentityModel.Tokens.Jwt;
using IdentityServer4;
using Shop_new.CustomAuthorisation;
using IdentityServer4.AccessTokenValidation;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Shop_new
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
            // установка конфигурации подключения
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => //CookieAuthenticationOptions
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/user");
                });
            
            services.AddTransient<BillService>();
            services.AddTransient<OrderService>();
            services.AddTransient<WareHouseService>();
            services.AddTransient<UserService>();

            services.AddLogging(lb => lb.AddConsole());
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(o =>
                {
                    o.Authority = "http://localhost:49491/";
                    o.RequireHttpsMetadata = false;
                    o.ApiName = "api";
                });
            services.AddSingleton<TokenStore>();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowAnyHeader());
            });
            services.AddDbContext<TokenDbContext>(options =>
            options.UseInMemoryDatabase("Tokenss"));

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseCors("AllowAll");
            app.UseAuthentication();
            //app.UseMiddleware<ShopnewCustomAuthorizationMiddleware>();
            app.UseMvc();
            app.UseStaticFiles();
        }
    }
}
