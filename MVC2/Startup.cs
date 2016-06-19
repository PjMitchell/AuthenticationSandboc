using IdentityServer3.AccessTokenValidation;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel.Client;
using IdentityServer3.Core;

[assembly: OwinStartupAttribute(typeof(WebApplication1.Startup))]
namespace WebApplication1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/api", apiApp =>
            {
                // token validation

                apiApp.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
                {
                    Authority = "https://localhost:44319/identity",
                    RequiredScopes = new[] { "sampleApi" },
                    DelayLoadMetadata = true
                });

                // add app local claims per request
                apiApp.UseClaimsTransformation(incoming =>
                {
                    // either add claims to incoming, or create new principal
                    var appPrincipal = new ClaimsPrincipal(incoming);
                    incoming.Identities.First().AddClaim(new Claim("appSpecific", "some_value"));

                    return Task.FromResult(appPrincipal);
                });

                // web api configuration
                var config = new HttpConfiguration();
                config.MapHttpAttributeRoutes();

                apiApp.UseWebApi(config);
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });


            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "https://localhost:44319/identity",

                ClientId = "mvc2",
                Scope = "openid profile roles sampleApi",
                ResponseType = "id_token token",
                RedirectUri = "https://localhost:44341/",

                SignInAsAuthenticationType = "Cookies",
                UseTokenLifetime = false,

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = async n =>
                    {
                        var nid = new ClaimsIdentity(
                            n.AuthenticationTicket.Identity.AuthenticationType,
                            Constants.ClaimTypes.GivenName,
                            Constants.ClaimTypes.Role);

                        // get userinfo data
                        var userInfoClient = new UserInfoClient(
                            new Uri(n.Options.Authority + "/connect/userinfo"),
                            n.ProtocolMessage.AccessToken);

                        var userInfo = await userInfoClient.GetAsync();
                        userInfo.Claims.ToList().ForEach(ui => nid.AddClaim(new Claim(ui.Item1, ui.Item2)));

                        // keep the id_token for logout
                        nid.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));

                        // add access token for sample API
                        nid.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));

                        // keep track of access token expiration
                        nid.AddClaim(new Claim("expires_at", DateTimeOffset.Now.AddSeconds(int.Parse(n.ProtocolMessage.ExpiresIn)).ToString()));

                        // add some other app specific claim
                        nid.AddClaim(new Claim("app_specific", "some data"));

                        n.AuthenticationTicket = new AuthenticationTicket(
                            nid,
                            n.AuthenticationTicket.Properties);
                    },

                    RedirectToIdentityProvider = n =>
                    {
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                        {
                            var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                            if (idTokenHint != null)
                            {
                                n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                            }
                        }

                        return Task.FromResult(0);
                    }
                }
            });

        }

        private static void Works(IAppBuilder app)
        {
            // token validation
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "https://localhost:44319/identity",
                RequiredScopes = new[] { "sampleApi" }
            });

            // add app local claims per request
            app.UseClaimsTransformation(incoming =>
            {
                // either add claims to incoming, or create new principal
                var appPrincipal = new ClaimsPrincipal(incoming);
                incoming.Identities.First().AddClaim(new Claim("appSpecific", "some_value"));

                return Task.FromResult(appPrincipal);
            });

            // web api configuration
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            app.UseWebApi(config);
        }

        private static void Stash(IAppBuilder app)
        {
            //app.Map("/api", apiApp => {
            //    // token validation

            //    apiApp.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            //    {
            //        Authority = "https://localhost:44319/identity",
            //        RequiredScopes = new[] { "sampleApi" },
            //        DelayLoadMetadata = true
            //    });

            //    // add app local claims per request
            //    apiApp.UseClaimsTransformation(incoming =>
            //    {
            //        // either add claims to incoming, or create new principal
            //        var appPrincipal = new ClaimsPrincipal(incoming);
            //        incoming.Identities.First().AddClaim(new Claim("appSpecific", "some_value"));

            //        return Task.FromResult(appPrincipal);
            //    });

            //    // web api configuration
            //    var config = new HttpConfiguration();
            //    config.MapHttpAttributeRoutes();

            //    apiApp.UseWebApi(config);
            //});

            // token validation

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "https://localhost:44319/identity",
                RequiredScopes = new[] { "sampleApi" }
                //,
                //DelayLoadMetadata = true
            });

            // add app local claims per request
            app.UseClaimsTransformation(incoming =>
            {
                // either add claims to incoming, or create new principal
                var appPrincipal = new ClaimsPrincipal(incoming);
                incoming.Identities.First().AddClaim(new Claim("appSpecific", "some_value"));

                return Task.FromResult(appPrincipal);
            });

            // web api configuration
            var config2 = new HttpConfiguration();
            config2.MapHttpAttributeRoutes();

            app.UseWebApi(config2);


            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AuthenticationType = "Cookies"
            //});


            //app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            //{
            //    Authority = "https://localhost:44319/identity",

            //    ClientId = "mvc",
            //    Scope = "openid profile roles sampleApi",
            //    ResponseType = "id_token token",
            //    RedirectUri = "https://localhost:44319/",

            //    SignInAsAuthenticationType = "Cookies",
            //    UseTokenLifetime = false,

            //    Notifications = new OpenIdConnectAuthenticationNotifications
            //    {
            //        SecurityTokenValidated = async n =>
            //        {
            //            var nid = new ClaimsIdentity(
            //                n.AuthenticationTicket.Identity.AuthenticationType,
            //                Constants.ClaimTypes.GivenName,
            //                Constants.ClaimTypes.Role);

            //            // get userinfo data
            //            var userInfoClient = new UserInfoClient(
            //                new Uri(n.Options.Authority + "/connect/userinfo"),
            //                n.ProtocolMessage.AccessToken);

            //            var userInfo = await userInfoClient.GetAsync();
            //            userInfo.Claims.ToList().ForEach(ui => nid.AddClaim(new Claim(ui.Item1, ui.Item2)));

            //            // keep the id_token for logout
            //            nid.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));

            //            // add access token for sample API
            //            nid.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));

            //            // keep track of access token expiration
            //            nid.AddClaim(new Claim("expires_at", DateTimeOffset.Now.AddSeconds(int.Parse(n.ProtocolMessage.ExpiresIn)).ToString()));

            //            // add some other app specific claim
            //            nid.AddClaim(new Claim("app_specific", "some data"));

            //            n.AuthenticationTicket = new AuthenticationTicket(
            //                nid,
            //                n.AuthenticationTicket.Properties);
            //        },

            //        RedirectToIdentityProvider = n =>
            //        {
            //            if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
            //            {
            //                var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

            //                if (idTokenHint != null)
            //                {
            //                    n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
            //                }
            //            }

            //            return Task.FromResult(0);
            //        }
            //    }
            //});
        }
    }
}
