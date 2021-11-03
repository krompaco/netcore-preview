using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using EPiServer.Authorization;
using EPiServer.Data;
using EPiServer.Scheduler;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using AlloyTemplates;
using AlloyMvcTemplates.Extensions;

namespace EPiServer.Templates.Alloy.Mvc
{
    public class Startup
    {
        private readonly IWebHostEnvironment _webHostingEnvironment;
        private readonly IConfiguration _configuration;

        public Startup(IWebHostEnvironment webHostingEnvironment, IConfiguration configuration)
        {
            _webHostingEnvironment = webHostingEnvironment;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (_webHostingEnvironment.IsDevelopment())
            {
                services.Configure<SchedulerOptions>(o =>
                {
                    o.Enabled = false;
                });

                services.PostConfigure<DataAccessOptions>(o =>
                {
                    o.SetConnectionString(_configuration.GetConnectionString("EPiServerDB").Replace("App_Data", Path.GetFullPath("App_Data")));
                });
            }

            // Same settings as Velusia.Client to work with the sample server
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = Global.LoginPath;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
                    options.SlidingExpiration = false;
                })
                .AddOpenIdConnect(options =>
                {
                    // Note: these settings must match the application details
                    // inserted in the database at the server level.
                    options.ClientId = "mvc";
                    options.ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654";

                    options.RequireHttpsMetadata = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    // Use the authorization code flow.
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;

                    // Note: setting the Authority allows the OIDC client middleware to automatically
                    // retrieve the identity provider's configuration and spare you from setting
                    // the different endpoints URIs or the token validation parameters explicitly.
                    options.Authority = "https://localhost:44313/";

                    options.Scope.Add("email");
                    options.Scope.Add("roles");

                    options.SecurityTokenValidator = new JwtSecurityTokenHandler
                    {
                        // Disable the built-in JWT claims mapping feature.
                        InboundClaimTypeMap = new Dictionary<string, string>()
                    };

                    options.TokenValidationParameters.NameClaimType = "name";
                    options.TokenValidationParameters.RoleClaimType = "role";

                    options.Events.OnUserInformationReceived = async ctx =>
                    {
                        var claimsIdentity = ctx.Principal?.Identities.First();

                        if (claimsIdentity == null)
                        {
                            return;
                        }

                        // TODO: Remove this and fetch roles from some place or have them handled in OpenIddict server
                        claimsIdentity.AddClaim(
                            new Claim(options.TokenValidationParameters.RoleClaimType, Roles.WebAdmins));

                        // Sync user and the roles to CMS db tables in the background
                        await ServiceLocator.Current.GetInstance<ISynchronizingUserService>().SynchronizeAsync(claimsIdentity);
                    };
                });

            services.AddMvc()
                .AddRazorRuntimeCompilation();

            services.AddAlloy();
            services.AddCms();

            services.AddEmbeddedLocalization<Startup>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Simple way to "disconnect" the built-in endpoints for CMS/AspNetIdentity
            var options = new RewriteOptions()
                .AddRedirect("(?i)^util/login(.*)", "en/login$1")
                .AddRedirect("(?i)^util/logout(.*)", "en/logout$1");
            app.UseRewriter(options);

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapContent();
                endpoints.MapRazorPages();
            });
        }
    }
}
