## Hypnolog C# Troubleshooting
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

