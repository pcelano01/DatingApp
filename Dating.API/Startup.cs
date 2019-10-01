using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dating.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

//Microsoft.EntityFrameworkCore.SqlServer
//Microsoft.EntityFrameworkCore.SqlServer.Design
//Microsoft.EntityFrameworkCore.Tools


namespace Dating.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }   

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (Configuration.GetValue<string>("Env").Equals("Development"))
            {
                if(Configuration.GetValue<string>("CurrentDataProvider").Equals("Sqlite"))
                    services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
                else
                    services.AddDbContext<DataContext>(x => x.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection")));
            }
            else if (Configuration.GetValue<string>("Env").Equals("Staging")
                    ||
                    Configuration.GetValue<string>("Env").Equals("Production"))
            {
                services.AddDbContext<DataContext>(x => x.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection")));
            }         
            else
            {
                services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            }   

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddCors();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => 
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                                            {
                                                ValidateIssuerSigningKey = true,
                                                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                                                ValidateIssuer = false,
                                                ValidateAudience = false
                                            };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
