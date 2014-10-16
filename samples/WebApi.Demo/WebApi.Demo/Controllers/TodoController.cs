using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApi.Demo.Controllers
{
    public class Todo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDone { get; set; }
    }

    [RoutePrefix("todo")]
    public class TodoController : ApiController
    {
        private List<Todo> Todos = new List<Todo>();
        public TodoController()
        {
            Todos.Add(new Todo() { Id = 1, Name = "Todo1", IsDone = false });
            Todos.Add(new Todo() { Id = 2, Name = "Todo2", IsDone = true });
            Todos.Add(new Todo() { Id = 3, Name = "Todo3", IsDone = false });
        }

        [Route("")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [Route("{id:int}")]
        public Todo Get(int id)
        {
            return Todos.FirstOrDefault(t => t.Id == id); ;
        }

        [Route("{name}")]
        public Todo Get(string name)
        {
            return Todos.FirstOrDefault(t => t.Name == name); 
        }

        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpPost]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete]
        public void Delete(int id)
        {
        }
    }
}