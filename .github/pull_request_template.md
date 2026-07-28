## What does this change?

<!-- A sentence or two on the change and why it is needed. -->

## Notes for the reviewer

<!-- Anything non-obvious: behaviour changes, things you decided against, areas you want a
     second opinion on. Delete if there is nothing to say. -->

## Checklist

- [ ] `dotnet test Jumoo.Json.slnx` passes
- [ ] Behaviour changes to public API are called out above and covered by a test
- [ ] `CHANGELOG.md` updated under **Unreleased**
- [ ] If a dependency changed, `dotnet restore Jumoo.Json.slnx --force-evaluate` was run and all
      updated `packages.lock.json` files are committed
- [ ] If this could move performance, benchmarks were run and the numbers are in the description
