using CommandAPI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using AutoMapper;
using System;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CommandAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = new NpgsqlConnectionStringBuilder();
            builder.ConnectionString = Configuration.GetConnectionString("PostgreSqlConnection");
            builder.Username = Configuration["UserID"];
            builder.Password = Configuration["Password"];
            // services.AddDbContext<CommandContext>(opt => opt.UseNpgsql(Configuration.GetConnectionString("PostgreSqlConnection")));
            services.AddDbContext<CommandContext>(opt => opt.UseNpgsql(builder.ConnectionString));

            // add JWT Bearer Authentication Scheme Service
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.Audience = Configuration["ResourceId"];
                opt.Authority = $"{Configuration["Instance"]}{Configuration["TenantId"]}";
            }
            );

            //register services to enable the use of "Controllers" throughout the application.
            // configure the controllers to use newtonsoftjson
            services.AddControllers().AddNewtonsoftJson(s =>
            {
                s.SerializerSettings.ContractResolver = new
                CamelCasePropertyNamesContractResolver();
            });
            // add the AutoMapper service
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            // //this configuration creates one MockCommandAPIRepo per request for one.
            // services.AddScoped<ICommandAPIRepo, MockCommandAPIRepo>();
            // add the real SqlCommandAPIRepo as a service
            services.AddScoped<ICommandAPIRepo, SqlCommandAPIRepo>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, CommandContext context)
        {
            // migrate ef changes on app startup 
            context.Database.Migrate();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            //add authetication/authorization to the request pipeline
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //map the controllers to the endpoints. (use the controller services registered in the ConfigureServices method as endpoints in the Request Pipeline)
                endpoints.MapControllers();
            });
        }
    }
}
