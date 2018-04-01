HypnoLog-csharp TODO list
================================

- Add some ready-to-use dll to the repo, write about it in the readme.
- create nuget

- Make it work in Visual Studio Code

- let user provide handling method when HypnoLog is disabled (let's say server
  is down, so HypnoLog diabled, and all logging just ignored)

- Do not use Nuget at all (just reference to dll files) because when working off-line (without nuget server) VS still display error.
- Handle errors when logging null value
- Handle object which throw exceptions while serialized to JSON (log "error while serializing" / ignore problematic members)
- Use same JSON serialization setting is Sync and Async logging (now setting apply only in Sync)
- set default serialization settings to serialize objects only one level deep.
  This will avoid creating too much big objects which then fail being sent to
  the server.



