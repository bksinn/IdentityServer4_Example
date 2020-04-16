using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using IdentityModel.Client;

namespace WebApiHost
{
    public class TestController : ApiController
    {
        [HttpGet]
        [Route("test")]
        public async Task<IHttpActionResult> Get()
        {
            var access_token = Request.Headers.Authorization.Parameter;

            var disco = await Startup.__discoveryCache.GetAsync();

            var client = new HttpClient();
            var tokenResult = await client.RequestTokenAsync(new TokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "api1",
                ClientSecret = "api1_secret",
                GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",
                Parameters = {
                    { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" },
                    { "subject_token", access_token },
                }
            });
            if (tokenResult.IsError)
            {
                // todo
            }

            var access_token2 = tokenResult.AccessToken;

            var req = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44364/test2");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token2);

            var result = await client.SendAsync(req);
            if (!result.IsSuccessStatusCode)
            {
                // todo -- decide how to handle
            }

            var json = await result.Content.ReadAsStringAsync();

            var user = (ClaimsPrincipal)User;
            var claims = user.Claims.Select(x => x.Type + ":" + x.Value);

            return Ok(new { message = "hello web api!", 
                claims,
                message2 = json 
            });
        }
    }
}