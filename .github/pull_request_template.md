## What does this change?

<!-- A sentence or two on the change and why it is needed. -->

## Notes for the reviewer

<!-- Anything non-obvious: behaviour changes, things you decided against, areas you want a
     second opinion on. Delete if there is nothing to say. -->

## Checklist

- [ ] `dotnet build Jumoo.Json.slnx -c Release` is clean
- [ ] `dotnet test Jumoo.Json.Tests/Jumoo.Json.Tests.csproj -c Release` passes
- [ ] If a dependency version changed, `dotnet restore --force-evaluate` was run and the
      updated `packages.lock.json` is committed
