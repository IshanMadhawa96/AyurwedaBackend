using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AyurwedaBackend.Data;
using AyurwedaBackend.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AyurwedaBackend.Models;
using Microsoft.AspNetCore.Identity;
using AyurwedaBackend.Helpers;
using static AyurwedaBackend.Services.Email.EmailService;
using AyurwedaBackend.Services.Email;

namespace AyurwedaBackend
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
            // Dbcontext Configuration
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
            Configuration.GetConnectionString("DefaultConnection")
            ));
            //For Entitiy Framework
            services.AddIdentity<ApplicationUser, IdentityRole>(/*opt =>
            {
                //ADD EMAIL CONFIRM IS REQUIRED
                opt.SignIn.RequireConfirmedEmail = true;   
            }*/)
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            //JWT SERVICE
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                
            }).AddJwtBearer(option =>
            {
                option.SaveToken = true;
                option.RequireHttpsMetadata = false;
                option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });

           /* services.Configure<IdentityOptions>(opts =>
            {
                opts.User.RequireUniqueEmail = true;
                //opts.Password.RequiredLength = 8;
                opts.SignIn.RequireConfirmedEmail = true;
            });*/

            //controllers
            services.AddControllers();
            services.AddTransient<IAccountRepository,AccountRepository>();

            // configure strongly typed settings object
            services.Configure<MailSettings>(Configuration.GetSection("MailSettings"));

            // configure DI for application services
            services.AddScoped<IEmailService, EmailService>();
            //for sawaggerui
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AyurwedaBackend", Version = "v1" });
            });
            //fix cors
            services.AddCors(option =>
            {
                option.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();   
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //tell what is the env 
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AyurwedaBackend v1"));
            }
            //middleware and what use in application
            app.UseHttpsRedirection();

            app.UseRouting();
            //fix  cors
            app.UseCors(); 
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
