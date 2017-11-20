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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.IO;

namespace WebApplication1
{
    internal class MaximumOfficeNumberAuthorizationHandler : AuthorizationHandler<MaximumOfficeNumberRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MaximumOfficeNumberRequirement requirement)
        {
            Console.WriteLine("HandleRequirementAsync");
            context.Succeed(requirement);
            // Bail out if the office number claim isn't present
            if (!context.User.HasClaim(c => c.Issuer == "http://localhost:5000/" && c.Type == "office"))
            {
                return Task.CompletedTask;
            }

            // Bail out if we can't read an int from the 'office' claim
            int officeNumber;
            if (!int.TryParse(context.User.FindFirst(c => c.Issuer == "http://localhost:5000/" && c.Type == "office").Value, out officeNumber))
            {
                return Task.CompletedTask;
            }

            // Finally, validate that the office number from the claim is not greater
            // than the requirement's maximum
            if (officeNumber <= requirement.MaximumOfficeNumber)
            {
                // Mark the requirement as satisfied
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    // A custom authorization requirement which requires office number to be below a certain value
    internal class MaximumOfficeNumberRequirement : IAuthorizationRequirement
    {
        public MaximumOfficeNumberRequirement(int officeNumber)
        {
            MaximumOfficeNumber = officeNumber;
        }

        public int MaximumOfficeNumber { get; private set; }
    }

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
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
                
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("OfficeNumberUnder200", policy => policy.Requirements.Add(new MaximumOfficeNumberRequirement(200)));
            });

            services.AddSingleton<IAuthorizationHandler, MaximumOfficeNumberAuthorizationHandler>();
            services.AddCors();
            services.AddMvc();
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = "Jwt";
                options.DefaultChallengeScheme = "Jwt";
            })
            .AddJwtBearer("Jwt", jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    //ValidAudience = "the audience you want to validate",
                    ValidateIssuer = false,
                    //ValidIssuer = "the isser you want to validate",
                    
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the secret that needs to be at least 16 characeters long for HmacSha256")),

                    ValidateLifetime = true, //validate the expiration and not before values in the token

                    ClockSkew = TimeSpan.FromMinutes(5) //5 minute tolerance for the expiration date
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            loggerFactory.AddFile("Logs/myapp-{Date}.txt");

            app.UseCors(builder =>
                builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
            );
            app.UseResponseCompression();

            app.UseAuthentication();
            app.UseMvc();

        }
    }
}
