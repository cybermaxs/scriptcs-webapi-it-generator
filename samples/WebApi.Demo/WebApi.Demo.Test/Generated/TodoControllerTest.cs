//auto generated 

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace  Tests
{
    [TestClass]
    public class TodoControllerTest
    {
        HttpClient Client { get; set; }
        HttpResponseMessage Response { get; set; }

        //vars
       	private int id;
       	private string name;

        #region TestInit & Cleanup
        [TestInitializeAttribute]
        public void TestInit()
        {
            this.Client = new HttpClient();

            //TODO : change the base address
            this.Client.BaseAddress = new Uri("http://localhost:33442/");

            //TODO set vars here
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Debug.WriteLine("Requested Uri : "+this.Response.RequestMessage.RequestUri);
            Assert.IsTrue(this.Response.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.OK, this.Response.StatusCode);

            var content = this.Response.Content.ReadAsStringAsync().Result;
            Assert.IsTrue(content.Length > 0);
            Debug.WriteLine(content);

            this.Client.Dispose();
            this.Client = null;
        }
        #endregion


    	
 		[TestMethod]
        public void TodoController_Get()
        {	            
			//act
	        this.Response = this.Client.GetAsync("todo/"+this.name+"").Result;
	    }
	    
	
	}

}