HypnoLog C# Library
============================

## What is HypnoLog?
*Get Hypnotized While Logging*

*HypnoLog* allows you to fast and easily visualize your application data/objects while debugging. From any environment, in any language. Forget about those black text-based console debug-printing back from the 70's. 

**See [HypnoLog main repo](https://github.com/SimonLdj/hypnolog-server).**

What it looks like, visualizing your data in the browser:
![alt text](/doc/images/screenshot_hypnolog-csharp-example.png "HypnoLog UI screenshot")

## About HypnoLog-C# Library
Logging using *HypnoLog* means sending your data as JSON HTTP request to
HypnoLog server. This library wraps all of those into simple easy to use
functions.

## Installation
The easiest way to get *HypnoLog-CSharp* is via [NuGet](https://www.nuget.org/):
```
Install-Package HypnoLog
```
Or use Nuget Package Manager UI (Visual Studio).

If you haven't use *HypnoLog* before, [setup HypnoLog server](https://github.com/SimonLdj/hypnolog-server#setup-hypnolog-server) on your machine:
```bash
npm install -g hypnolog-server
```
*Note:* you will need [Node.js](https://nodejs.org/en/) installed on your machine first.

## Usage
1. Start [HypnoLog Server]:
    ```bash
    hypnolog-server
    ```
2. View output: open [`http://127.0.0.1:7000/client.html`](http://127.0.0.1:7000/client.html) in your browser.
3. Add `using` for HypnoLog in your code:
    ```csharp
    using HL = HypnoLog.HypnoLog;
    ```
4. Log:
    ```csharp
    // Log a string
    HL.Log("Hello HypnoLog from C#!");

    // log array of numbers as a graph (plot)
    HL.Log(new[] { 1, 2, 3 }, "plot");
    ```

For more examples, see [Basic Example](/HypnoLogExample/BasicExample.cs) and [Advanced Example](HypnoLogExample/AdvancedExample.cs) code files.

Read how to view the log and more about *HypnoLog* in [HypnoLog main repo page](https://github.com/SimonLdj/hypnolog-server).

___

### Troubleshooting
See [Troubleshooting] page.


[Troubleshooting]: doc/Troubleshooting.md
[HypnoLog Server]: https://github.com/SimonLdj/hypnolog-server
