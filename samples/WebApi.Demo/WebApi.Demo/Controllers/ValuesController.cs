using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApi.Demo.Controllers
{
    [RoutePrefix("values")]
    public class ValuesController : ApiController
    {
        [Route("all")]
        public IEnumerable<string> Getter()
        {
            return new string[] { "value1", "value2" };
        }

        [Route("{id:int}")]
        public string GetById(int id)
        {
            return "value";
        }

        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete]
        public void Delete(int id)
        {
        }
    }
}