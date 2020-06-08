using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.API.Data;
using Identity.API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Identity.API
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
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("TaskIdentityDB"))
            );
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            //IdentityServer Congfiguration
            services.AddIdentityServer(options => {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            // this adds the config data from DB (clients, resources)
            .AddConfigurationStore(options => {
                options.ConfigureDbContext = opt =>
                {
                    opt.UseSqlServer(Configuration.GetConnectionString("TaskIdentityDB"),
                        optionsBuilder => optionsBuilder.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name));
                };
            })
            //PersistedGrantDbContext ConfigurationDbContext
            //// this adds the operational data from DB (codes, tokens, consents)
            .AddOperationalStore(options => {
                options.ConfigureDbContext = opt =>
                    {
                        opt.UseSqlServer(Configuration.GetConnectionString("TaskIdentityDB"),
                            optionsBuilder => optionsBuilder.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name));
                    };
                options.EnableTokenCleanup = true;
            }).AddAspNetIdentity<AppUser>();
            services.AddAuthentication();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
