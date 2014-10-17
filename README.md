scriptcs-webapi-it-generator
============================

Generate integration tests for Web Api for VS Test Runner

###Concept
I was asked to do a performance of a large web api containing many controllers and actions. 
In the same time, as this api is still in dev, it changes very often (additions and deletions)
To automate this task, I need an easy way to generate integration tests.

###Demo
This repo contains a simple web api project : run command generate_it to generate test files.
You can also [inspect the generated files](https://github.com/Cybermaxs/scriptcs-webapi-it-generator/tree/master/samples/WebApi.Demo/WebApi.Demo.Test/Generated). 

###WORKS ON MY MACHINE
This can be viewed as a dump of my work.
Don't like my test template ? don't hesitate to adapt it to your context.

options (follow [scripts recommendations](https://github.com/scriptcs/scriptcs/wiki/Pass-arguments-to-scripts))
 - path : path to inspect
 - baseAddress : baseAddress of api (for the HttpClient)
 - outpath : where to generate test file
 - debug : add many logs

###Requirements
[Scriptcs](http://scriptcs.net/)
Web Api 2 with attribute routing

###How it works
An ugly reflection (all the way) on several assemblies to find ApiControllers.
RoutePrefixAttribute & RouteAttribute are used to generate Get Requests (Post, Put, .. are not implemented)
RazorEngine is used to generate test file : one file par ApiController.