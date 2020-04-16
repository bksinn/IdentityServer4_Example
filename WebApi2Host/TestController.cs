using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace WebApi2Host
{
    public class TestController : ApiController
    {
        [HttpGet]
        [Route("test2")]
        public IHttpActionResult Get()
        {
            return Ok(new { message = "hello web api2!" });
        }
    }
}