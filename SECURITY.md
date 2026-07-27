# Security Policy

## Supported versions

Jumoo.Json ships one branch per Umbraco major. Fixes go to the branch for the current Umbraco
major, and to the previous one where the issue is serious and the fix is practical.

| Version | Branch | Supported |
| --- | --- | --- |
| 18.x | `v18/main` | Yes |
| 17.x | `v17/main` | Security fixes only |
| 16.x and earlier | — | No |

## Reporting a vulnerability

Please **do not** open a public issue for a security problem.

Email **kevin@jumoo.co.uk** with a description of the issue, the version affected, and steps to
reproduce it. We'll acknowledge within a few working days and keep you updated as we work on it.

If the issue affects one of the packages that consume this library (uSync and friends), say so —
it changes how we sequence the fix.
