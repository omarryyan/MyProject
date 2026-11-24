# BankFoundation — Agent Context

Read this file at the start of every session. Keep it updated when progress changes.

## What this project is

Practice rewrite of a Junior C# / .NET take-home assignment: a bank **Foundation** layer that sends messages to a queue, tracks exchange **Session** state, and writes **Audit** records.

- Goal: the learner **codes while understanding**. Prefer guidance over full solutions.
- Full assignment text: `../מבחן-בית-Junior-CSharp-NET.md` (extracted from the PDF).
- Remote (learner pushes): `https://github.com/omarryyan/MyProject.git`

### What is / isn’t relevant

| Relevant                                                            | Not relevant             |
| ------------------------------------------------------------------- | ------------------------ |
| Part A–B deliverables (done); understanding & defense-style changes | Formal grading / weights |
| Optional polish when learner asks                                   | Timeline / hours spent   |

## How to work with the learner

- Explain requirements; do **not** solve whole steps unless asked.
- If asked to implement something: do it, then **explain** the C# ideas.
- Stay on **`net11.0`** (chosen for practice; not switching to net8 unless asked).
- Prefer Hebrew only if the learner asks; otherwise English.
- **Do not commit or push** unless the learner explicitly asks.

## Solution layout

```
MyProject/
  docs/
    AGENTS.md
    event-refactor-guide.md
  .cursor/rules/bankfoundation-practice.mdc
  README.md                       # run, review, design answers, “didn’t finish”
  ANSWERS.md
  AI_NOTES.md
  BankFoundation.slnx
  BankFoundation/                 # net11.0 Console
    Program.cs, MessageSender.cs, appsettings.json
    Queues/, Session/, Audit/
  BankFoundation.Tests/           # xUnit — Session, MessageSender, QueueConfiguration
```

## Assignment status — COMPLETE

| Item                         | Status         |
| ---------------------------- | -------------- |
| Part A steps 1–9             | Done           |
| Part B ANSWERS.md            | Done           |
| AI_NOTES.md                  | Done           |
| README (incl. didn’t finish) | Done           |
| Tests                        | **17** passing |

## Important design choices

- `IMessageQueue` + DI from `Queue:Type` (`Sim` / `DATQ`).
- `DatqOptions` bound from `Queue:Datq` (Host/User/Password); `DatqMessageQueue` fakes a write using those values — no real connection.
- Session: enum + transition map; `StateChanged` on success.
- `MessageSender`: Active → deliver; Suspended → buffer (max **100** default, reject when full); flush on **Active**; clear on **LoggingOut** / **Disconnected**; **Reconnecting** does not clear pending.
- Audit: `IAuditMessage` → `FileAudit` (hardcoded local `Audits.txt`, append) / `InMemoryAudit`.
- Namespaces: `BankFoundation`, `.Queues`, `.Sessions`, `.Audit`.
- Host: `ContentRootPath = AppContext.BaseDirectory`.

### Optional later (only if learner asks)

- Configurable / relative audit path.
- Explicit policy if Suspended → Reconnecting should clear pending.
- Move DATQ password out of committed appsettings (user secrets / env).

## Next likely work

1. Defense practice (explain / small change without AI).
2. Learner commits/pushes local changes when ready.

## Do not

- Rewrite working code without asking.
- Change `net11.0` unless asked.
- Commit/push unless asked.
- Treat grading weights or hours as requirements.
