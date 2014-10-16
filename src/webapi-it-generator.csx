using System;
using System.Reflection;
using RazorEngine; 
using System.Text.RegularExpressions;

//settings
var path = System.Environment.CurrentDirectory;
var outpath="out";
bool debug=false;

//path = @"C:\betc-projets\WK_HORSERACING\Front\Dev\Development\HorseRacing.Service\HorseRacing.Api\bin";

foreach(var arg in Env.ScriptArgs)
{
	string[] parts = arg.Split('=');
	if(parts.Length==2)
	{
		switch(parts[0])
		{
			case "debug":
				if(!bool.TryParse(parts[1], out debug))
					Console.WriteLine("Could not parse boolean value "+ parts[1]);
				break;
			case "path" :
				if(!Directory.Exists(parts[1]))
					Console.WriteLine("Directory does not exists "+ parts[1]);
				else
					path = parts[1];
				break;
				case "outpath" :
				if(!Directory.Exists(parts[1]))
					Console.WriteLine("Directory does not exists "+ parts[1]);
				else
					outpath = parts[1];
				break;
			default:
				Console.WriteLine("unknow parameter "+ parts[0]);
		}
	}
}

// PARSING

var files = System.IO.Directory.GetFiles(path, "*.dll", System.IO.SearchOption.AllDirectories);
foreach(var file in files)
{
	//skip Microsoft.* and System.* assemblies
	if(file.Contains("Microsoft") || file.Contains("System"))
		continue;
	//Console.WriteLine(file);
	try
	{
		var assembly = Assembly.LoadFrom(file);
		//type
		foreach(var apiController in assembly.GetTypes().Where(t=>t.BaseType == typeof(System.Web.Http.ApiController))) 
		{
			var controllerdef = new ControllerDef();
			controllerdef.Name = apiController.Name;

			//first check routePrefix (class level)
	        var routePrefixAtt = apiController.GetCustomAttribute<System.Web.Http.RoutePrefixAttribute>(false);
	        if(routePrefixAtt !=null) 
	        {
	        	//have route prefix
	            controllerdef.RoutePrefix=routePrefixAtt.Prefix;
	        }

	        foreach (var apiAction in apiController.GetMethods().Where(m => m.GetCustomAttribute<System.Web.Http.RouteAttribute>(false)!=null)) 
	        {
	        	// only HttpGet is supported ... for now
	        	var notsupported = new string[] {"HttpPostAttribute", "HttpPutAttribute", "HttpHeadAttribute", "HttpOptionsAttribute", "HttpHeaderAttribute", "HttpDeleteAttribute"};
	        	var atts = apiAction.GetCustomAttributes().Select(t=>t.GetType().Name);
				if(atts.Intersect(notsupported).Count()>0)
				{
					if(debug) Console.WriteLine("skipped=>"+apiAction.ToString());
					continue;
				}

				var actionName = apiAction.Name;
				var route = string.Empty;

				//check http get 
				var routeAtt = apiAction.GetCustomAttribute<System.Web.Http.RouteAttribute>(false);
				if(routeAtt!=null)
				{
					route=routeAtt.Template;
				}

				controllerdef.AddAction(actionName, route);
	        }

	        try 
	        {	            

	        	// generate one file per ApiController
		        var template = File.ReadAllText("vs-it-template.razor");
		        //generate test file

		          string result = Razor.Parse(template, new {Name = controllerdef.Name, Vars=controllerdef.Vars, Actions = controllerdef.Actions, RoutePrefix = controllerdef.RoutePrefix});
		        
		        var filename=Path.Combine(outpath, controllerdef.FileName);

		        if(File.Exists(filename))
					File.Delete(filename);
				File.AppendAllText(filename, result);

				if(controllerdef.Actions.Count()>0)
					Console.WriteLine(string.Format("File {0} generated ({1} tests, {2} variables)", filename, controllerdef.Actions.Count(), controllerdef.Vars.Count()));

			} 
			catch (Exception ex) 
			{
	            if(debug)  Console.WriteLine(string.Format("Could not generate test file {0} ; Reason {1}", controllerdef.FileName, ex.ToString()));
	        }
    	}
	
	}
	catch(Exception ex) 
	{
	if(debug)  Console.WriteLine(string.Format("Could not load file {0} ; Reason {1}", file, ex.ToString()));
	}
	
}


public class ControllerDef
{
	public string Name {get;set;}
	public string FileName {get{return this.Name+"Test.cs";}}
	public string RoutePrefix {get;set;}

	public Dictionary<string, string> Actions {get;set;}
	public Dictionary<string, string> Vars {get;set;}

	public ControllerDef()
	{
		this.Actions = new Dictionary<string, string>();
		this.Vars = new Dictionary<string, string>();
	}

	public void AddAction(string name, string route)
	{
		route = ReplaceTokens(route);
		this.Actions.Add(name, route);
	}

	private string ReplaceTokens(string route)
	{
		//Console.WriteLine(route);
	    var rex = new Regex(@"\{([^}]+)}");
	    return(rex.Replace(route, delegate(Match m)
	    {
	        string key = m.Groups[1].Value;

	        var parts = key.Split(':');
	        if(parts.Length==2)
	        {
	        //first is name
		        var name = parts[0];
		        //second is type
				var typ = parts[1];
				Vars[name]=typ;

	    	}

	        return @"""+this."+parts[0]+@"+""";
	        //return(key);
	    }));
	}
}