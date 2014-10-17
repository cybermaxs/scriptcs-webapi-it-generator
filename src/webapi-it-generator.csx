using System;
using System.Reflection;
using RazorEngine; 
using System.Text.RegularExpressions;

//settings
var path = System.Environment.CurrentDirectory;
var outpath="out";
bool debug=false;
string baseAddress="http://localhost";
var workingDir= Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[1]);



//parse settings
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
					Console.WriteLine("Directory does not exists :"+ parts[1]);
				else
					path = parts[1];
				break;
			case "outpath" :
					outpath = parts[1];
				break;
			case "baseAddress" :
				baseAddress = parts[1];
				break;
			default:
				Console.WriteLine("unknow parameter "+ parts[0]);
		}
	}
}

var files = System.IO.Directory.GetFiles(path, "*.dll", System.IO.SearchOption.AllDirectories);
if(debug) Console.WriteLine(string.Format("{0} dll to parse", files.Length));
foreach(var file in files)
{
	//skip Microsoft.* and System.* assemblies
	if(file.Contains("Microsoft") || file.Contains("System"))
		continue;
	
	try
	{
		var assembly = Assembly.LoadFrom(file);



		//type
		foreach(var apiController in assembly.GetTypes().Where(t=>t.BaseType == typeof(System.Web.Http.ApiController))) 
		{
			var controllerName = apiController.Name;
			var routePrefix = string.Empty;
			Dictionary<string, string> Actions = new Dictionary<string, string>();
			Dictionary<string, string> Vars = new Dictionary<string, string>();


			//first check routePrefix (class level)
	        var routePrefixAtt = apiController.GetCustomAttribute<System.Web.Http.RoutePrefixAttribute>(false);
	        if(routePrefixAtt !=null) 
	        {
	        	//have route prefix
	            routePrefix=routePrefixAtt.Prefix;
	        }

	        //find api actions
	        foreach (var apiAction in apiController.GetMethods().Where(m => m.GetCustomAttribute<System.Web.Http.RouteAttribute>(false)!=null)) 
	        {
	        	// only HttpGetAttribute is supported ... for now
	        	var notsupportedAtt = new string[] {"HttpPostAttribute", "HttpPutAttribute", "HttpHeadAttribute", "HttpOptionsAttribute", "HttpHeaderAttribute", "HttpDeleteAttribute"};
	        	var atts = apiAction.GetCustomAttributes().Select(t=>t.GetType().Name);
				if(atts.Intersect(notsupportedAtt).Count()>0)
				{
					if(debug) Console.WriteLine("skipped=>"+apiAction.ToString());
					continue;
				}

				var notsupportedName = new string[] {"Post", "Put", "Delete"};

				if(notsupportedName.Any(n=> apiAction.Name.Contains(n)))
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

				//
				Actions[actionName]=ReplaceParameters(route, Vars);
	        }

	        //generate test file
	        try 
	        {	            
	        	var filename=Path.Combine(outpath, controllerName+"Test.cs");

				if(debug)  Console.WriteLine("trying to generate "+ filename);
	        	// generate one file per ApiController
		        var template = File.ReadAllText(Path.Combine(workingDir, "vs-it-template.razor"));
		        //generate test file

		        string result = Razor.Parse(template, new {Name = controllerName,BaseAddress=baseAddress, Vars=Vars, Actions = Actions, RoutePrefix = routePrefix});
		        
		        
		        //create output directory
				if(!Directory.Exists(outpath))
					Directory.CreateDirectory(outpath);

		        if(File.Exists(filename))
					File.Delete(filename);
				File.AppendAllText(filename, result);

				if(Actions.Count()>0)
					Console.WriteLine(string.Format("File {0} generated ({1} tests, {2} variables)", filename, Actions.Count(), Vars.Count()));

			} 
			catch (Exception ex) 
			{
	            if(debug)  Console.WriteLine(string.Format("Could not generate test file; Reason {0}", ex.ToString()));
	        }
    	}
	
	}
		catch(ArgumentException aex) 
	{
		if(debug)  Console.WriteLine(string.Format("Could not load file {0} ; Reason {1}", file, aex.ToString()));
	}
	catch(Exception ex) 
	{
		if(debug)  Console.WriteLine(string.Format("Could not load file {0} ; Reason {1}", file, ex.ToString()));
	}
	
}

public string ReplaceParameters(string route, Dictionary<string, string> vars)
{
	//Console.WriteLine(route);
    var rex = new Regex(@"\{([^}]+)}");
    return(rex.Replace(route, delegate(Match m)
    {
        string key = m.Groups[1].Value;
	    var parts = key.Split(':');

        if(!vars.ContainsKey(parts[0]))
        {
	        if(parts.Length==1 || parts.Length==2)
	        {
		        //first is name
		        var name = parts[0];
		        //second is type
				var typ = parts.Length==2 ? parts[1] : "string"; //not specified means string
				vars[name]=typ;

	    	}
	    }
	    return @"""+this."+parts[0]+@"+""";
	   }));
}