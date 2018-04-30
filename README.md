HypnoLog C# Library
============================

## What is HypnoLog?
*Get Hypnotized While Logging*

*HypnoLog* allows you to fast and easily visualize your application data/objects while debugging. From any environment, in any language. Forget about those black text-based console debug-printing back from the 70's. 

**See [HypnoLog main repo](https://github.com/SimonLdj/hypnolog-server).**

What it looks like, visualizing your data in the browser:
![alt text](/doc/images/screenshot_hypnolog-csharp-example.png "HypnoLog UI screenshot")

## About HypnoLog C# Library
Logging using *HypnoLog* means sending you data as JSON HTTP request to HypnoLog server. This library wraps all of those into simple easy to use C# functions.
To use *HypnoLog* in your C# project you can include the library by (a) adding reference to the `.dll` file or (b) using Nuget Package Manager or (c) include the source code as another project in your solution.

## Usage Examples
Really simple. Import HypnoLog:
```csharp
using HL = HypnoLog.HypnoLog;
```
Log:
```csharp
// Log a string
HL.Log("Hello HypnoLog from C#!");

// log array of numbers as a graph (plot)
HL.Log(new []{1, 2, 3}, "plot");
```

For more examples, see [Basic Example](/HypnoLogExample/BasicExample.cs) and [Advanced Example](HypnoLogExample/AdvancedExample.cs) code files.

Read how to view the log and more about *HypnoLog* in [HypnoLog main repo page](https://github.com/SimonLdj/hypnolog-server).

## Troubleshooting
### 1. I get exception such as "Could not load file or assembly":

This exception typically happens at run time.
Example of such exception:

    A first chance exception of type 'System.IO.FileNotFoundException' occurred in System.Net.Http.Formatting.dll
    Additional information: Could not load file or assembly 'Newtonsoft.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed' or one of its dependencies. The system cannot find the file specified.
    

This caused by faulty assembly redirects. (The assembly used using Nuget packages).

Specifically the exception above happens because `Microsoft.AspNet.WebApi.Client package`
looking for `Newtonsoft.Json` in older version than it is (6.0.0.0 instead of 8.0.0.0 for example).

**To fix this issue, open Package Manager Console in Visual Studio and type `Get-Project -All | Add-bindingRedirect`**

Or the manual solution:
The solution is to specifically configure to use newer version of `Newtonsoft.Json` package.
Do this by adding the following configuration to `App.config` file of your *StartUp Project*

    <runtime>
        <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
            <dependentAssembly>
                <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
                <bindingRedirect oldVersion="0.0.0.0-8.0.0.0" newVersion="8.0.0.0" />
            </dependentAssembly>
        </assemblyBinding>
    </runtime>

Note `<runtime>` tag should be under the `<configuration>` root tag.

See this blog post for more information: http://blog.myget.org/post/2014/11/27/Could-not-load-file-or-assembly-NuGet-Assembly-Redirects.aspx

### 2. I don't see some of object's properties when logging it

HypnoLog uses `Newtonsoft.Json` to serialize objects as JSON.
Check the `JsonSerializerSettings` object used by HypnoLog (created in constructor).
You can try modify the setting such as `ReferenceLoopHandling` (might be set to ignore), `MaxDepth`,...

