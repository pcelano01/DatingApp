using System.Net;
using AutoMapper;
using Dating.API.Data;
using Dating.API.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            if(Configuration.GetValue<string>("Env") != null)
            {
                if (Configuration.GetValue<string>("Env").Equals("Development"))
                {
                    if(Configuration.GetValue<string>("CurrentDataProvider").Equals("Sqlite"))
                        services.AddDbContext<DataContext>(x => 
                        {
                            x.UseLazyLoadingProxies();
                            x.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
                        }
                        );
                    else
                        services.AddDbContext<DataContext>(x => 
                        {
                            x.UseLazyLoadingProxies();
                            x.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"));
                            //x.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"), builder => builder.UseRowNumberForPaging());
                        }
                        );

                    //il secret storage che funziona solo in ambiente di sviluppo
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
                else if (Configuration.GetValue<string>("Env").Equals("Staging")
                        ||
                        Configuration.GetValue<string>("Env").Equals("Production"))
                {
                    services.AddDbContext<DataContext>(x => 
                            {
                                x.UseLazyLoadingProxies();
                                x.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"));
                                //x.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"), builder => builder.UseRowNumberForPaging());
                            }
                    );

                    // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    // .AddJwtBearer(options => 
                    // {
                    //     options.TokenValidationParameters = new TokenValidationParameters()
                    //                                 {
                    //                                     ValidateIssuerSigningKey = true,
                    //                                     IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes("Da definire!")),
                    //                                     ValidateIssuer = false,
                    //                                     ValidateAudience = false
                    //                                 };
                    // });
                }         
                else
                {
                    services.AddDbContext<DataContext>(x => 
                    {
                        x.UseLazyLoadingProxies();
                        x.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
                    }                    
                    );
                }                
            }                 
            else
            {
                //Production and Staging
                if(Configuration.GetConnectionString("SqlServerConnection") != null)
                    services.AddDbContext<DataContext>(x => 
                    {
                        x.UseLazyLoadingProxies();
                        x.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"));
                        //x.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"), builder => builder.UseRowNumberForPaging());
                    }                    
                    );
                else
                    services.AddDbContext<DataContext>(x => 
                    {
                        x.UseLazyLoadingProxies();
                        x.UseSqlServer("Data Source=DESKTOP-QB60D23\\SQLEXPRESS;Initial Catalog=Dating;Integrated Security=True");
                        //x.UseSqlServer("Data Source=DESKTOP-QB60D23\\SQLEXPRESS;Initial Catalog=Dating;Integrated Security=True", builder => builder.UseRowNumberForPaging());
                    }                    
                    );
            }                

            services.AddControllers().AddNewtonsoftJson(opt => {
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; 
            });
            // services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            // .AddJsonOptions(opt => {
            //         opt.SerializerSettings.ReferenceLoopHandling= Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            // });
            services.AddCors();
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            services.AddAutoMapper(typeof(DatingRepository).Assembly);
            services.AddScoped<IDatingRepository, DatingRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();    
            services.AddScoped<LogUserActivity>();       
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
                app.UseExceptionHandler(builder => {
                    builder.Run(async context => {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if(error != null)
                        {
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    });
                });
            }

            //app.UseHsts();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();            

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            
            app.UseDefaultFiles();
            app.UseStaticFiles();
            //app.UseMvc();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapFallbackToController("Index", "Fallback");
            });
            // app.UseMvc(
            //     routes => {
            //         routes.MapSpaFallbackRoute(
            //             name: "spa-fallback",
            //             defaults: new { controller = "Fallback", action = "Index" }
            //         );
            //     }
            // );
        }
    }
}
