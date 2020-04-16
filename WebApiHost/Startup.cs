using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WebApiHost
{
    public class Startup
    {
        const string SpaClient = "http://localhost:5002";

        const string Authority = "https://localhost:5001";
        const string ApiName = "api1";

        public void Configuration(IAppBuilder app)
        {
            // stupid MSFT
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = ApiName,
                    ValidIssuer = Authority,
                    IssuerSigningKeyResolver = LoadKeys
                }
            });

            app.Use(async (ctx, next) => 
            {
                if (ctx.Authentication.User.Identity.IsAuthenticated)
                {
                    if (ctx.Authentication.User.HasClaim("sub", "489984667"))
                    {
                        ctx.Authentication.User.Identities.First().AddClaim(new Claim("permission", "CanDoStuff"));
                    }
                }

                await next();
            });
            
            var webApi = new HttpConfiguration();
            webApi.Formatters.Remove(webApi.Formatters.XmlFormatter);
            webApi.MapHttpAttributeRoutes();
            webApi.EnableCors(new EnableCorsAttribute(SpaClient, "*", "*"));
            webApi.Filters.Add(new AuthorizeAttribute());
            app.UseWebApi(webApi);
        }

        internal static DiscoveryCache __discoveryCache = new DiscoveryCache(Authority);

        private IEnumerable<SecurityKey> LoadKeys(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters)
        {
            var disco = __discoveryCache.GetAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            var keys = disco.KeySet.Keys
                .Where(x => x.N != null && x.E != null)
                .Select(x => {
                    var rsa = new RSAParameters
                    {
                        Exponent = Base64UrlEncoder.DecodeBytes(x.E),
                        Modulus = Base64UrlEncoder.DecodeBytes(x.N),
                    };

                    return new RsaSecurityKey(rsa)
                    {
                        KeyId = x.Kid
                    };
                });

            return keys;
        }
    }
}