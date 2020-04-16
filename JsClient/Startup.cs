using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;

namespace JsClient
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDefaultFiles();

            // this tests CSP
            app.Use(async (ctx, next) =>
            {
                ctx.Response.OnStarting(() =>
                {
                    if (ctx.Response.ContentType?.StartsWith("text/html") == true)
                    {
                        //ctx.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; connect-src https://localhost:5001 https://localhost:44350; frame-src 'self' https://localhost:5001");
                    }
                    return Task.CompletedTask;
                });

                await next();
            });

            app.UseStaticFiles();
        }
    }
}