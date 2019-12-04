using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OneLoginSampleAuthApi.Auth;
using OneLoginSampleAuthApi.Models;

namespace OneLoginSampleAuthApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //add OneLogin custom authentication service and mark it as the default scheme
            //you can add additional login schemes for consumers coming from a separate location if required.
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = OneLoginOpenIdConnectAuthenticationOptions.DefaultScheme;
                })
                .AddOneLoginOidcAccessToken(OneLoginOpenIdConnectAuthenticationOptions.DefaultScheme, options => { });

            services
                .AddMvc(options =>
                {
                    //default authorization policy adding to all incoming requests.
                    //this will enforce that all requests are authenticated unless they have a [AllowAnonymous] attribute applied
                    var policy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));

                    //housekeeping. clearing down the supported media types, to keep swagger cleaner.
                    foreach (var item in options.InputFormatters)
                    {
                        if (item is JsonInputFormatter)
                        {
                            ((JsonInputFormatter)item).SupportedMediaTypes.Clear();
                            ((JsonInputFormatter)item).SupportedMediaTypes.Add("application/json");
                        }
                    }
                    foreach (var item in options.OutputFormatters)
                    {
                        if (item is JsonOutputFormatter)
                        {
                            ((JsonOutputFormatter)item).SupportedMediaTypes.Clear();
                            ((JsonOutputFormatter)item).SupportedMediaTypes.Add("application/json");
                        }
                    }
                    options.OutputFormatters.RemoveType<StringOutputFormatter>();
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //register services in DI so get auto injected into authentication handler / controllers etc...
            services.AddMemoryCache();
            services.AddHttpClient<OneLoginOpenIdConnectApiService>();
            services.Configure<OneLoginOptions>(Configuration.GetSection(Constants.ConfigGroups.OneLogin));

            //register swagger generator
            services.AddSwaggerGen(c =>
            {
                //added security definition to swagger. Note: in swagger UI you need to enter the keyword "Bearer"
                //followed by a space and then the access token you can get from running the SPA application
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                c.AddSecurityDefinition(OneLoginOpenIdConnectAuthenticationOptions.DefaultScheme, new OpenApiSecurityScheme()
                {
                    Description = "Access Token received in Mission Control SPA after authenticating with OneLogin",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = OneLoginOpenIdConnectAuthenticationOptions.DefaultScheme
                        }
                    }, new List<string>()
                }});
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();
            app.UseExceptionHandler(errorApp =>
            {
                //example global error handler.
                //this will catch any unhandled exception.
                //example shows how you can log the exception out to registered logger(s) [Note: set these up in Program.cs]
                //and return a default error object in an expected shape with the 500 response status code.
                errorApp.Run(async context =>
                {
                    //log out error
                    var log = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                    var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                    if (feature?.Error != null)
                    {
                        log.LogError(Constants.LoggingEvents.UnhandledException, feature.Error, "Unhandled exception caught in global error handler");
                    }
                    else
                    {
                        log.LogError(Constants.LoggingEvents.UnhandledException, "Unhandled exception caught in global error handler (error object not defined)");
                    }

                    //return 500 with default error JSON response
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new Models.ApiErrorResponse() { StatusCode = 500, Message = "Unhandled exception" }), Encoding.UTF8);
                });
            });
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample API V1");
                options.RoutePrefix = string.Empty;
            });
        }
    }
}
