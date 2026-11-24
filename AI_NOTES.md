# AI_NOTES

## Tools used

| Tool | What I used it for |
|------|--------------------|
| **Cursor (Composer / agent)** | Scaffolding and iterating on the BankFoundation solution: queues, DI, session states, audit, `MessageSender`, tests, README/ANSWERS polish, and refactors. |
| **ChatGPT** | Explaining C# concepts (e.g. how **events** are declared, subscribed, and raised) so I understood the ideas before / while applying them in code. |

## How I used AI

- **Cursor:** implement and review code step by step; I still had to understand session transitions, DI, and send/audit behavior to change and defend the design.
- **ChatGPT:** concept explanations (events, and similar language topics) — learning aid, not a substitute for writing the project logic myself.

## Example: AI suggestion I did not take as-is

**Flushing the Suspended buffer via `MessageSender.ChangeState`**

Cursor first wired flush by wrapping state changes in `MessageSender.ChangeState` (call the session, then flush if Active). That worked, but it meant two ways to change state and a risk of calling `Session.ChangeState` directly and skipping the flush.

I preferred a **`Session.StateChanged` event**: `Session` stays the only place that changes state; `MessageSender` subscribes and flushes when the new state is `Active`. We refactored to that design.

**Why I changed it:** clearer separation of responsibilities and a single API for transitions (`session.ChangeState`), which matches how I’d want the code to work in a real system.

## Other notes

- Early on I removed `DatqOptions` to keep DATQ simple; later I **brought it back** so connection details come from `appsettings.json` (`Queue:Datq`) while still not opening a real AS/400 connection — matching the assignment wording.
- Assignment answers (code review, Part B) were drafted from my own reasoning; AI helped with wording/structure where useful.
