using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;

namespace WebApi2Host
{
    public class Startup
    {
        const string Authority = "https://localhost:5001";
        const string ApiName = "api2";

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

            var webApi = new HttpConfiguration();
            webApi.Formatters.Remove(webApi.Formatters.XmlFormatter);
            webApi.MapHttpAttributeRoutes();
            webApi.Filters.Add(new AuthorizeAttribute());
            app.UseWebApi(webApi);
        }

        static DiscoveryCache __discoveryCache = new DiscoveryCache(Authority);

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