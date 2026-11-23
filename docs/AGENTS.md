# BankFoundation — Agent Context

Read this file at the start of every session. Keep it updated when progress changes.

## What this project is

Practice rewrite of a Junior C# / .NET take-home assignment: a bank **Foundation** layer that sends messages to a queue, tracks exchange **Session** state, and writes **Audit** records.

- Goal: the learner **codes while understanding**. Prefer guidance over full solutions.
- Full assignment text: `../מבחן-בית-Junior-CSharp-NET.md` (extracted from the PDF).

### What is / isn’t relevant

| Relevant (do eventually)                                              | Not relevant             |
| --------------------------------------------------------------------- | ------------------------ |
| Part A code steps 1–9                                                 | Formal grading / weights |
| Part B thinking questions                                             | Timeline / time spent    |
| Understanding & being able to change the code (defense-style)         |                          |
| `AI_NOTES.md` (tools used, tasks, one rejected/changed AI suggestion) |                          |
| `README.md`, `ANSWERS.md`, repo hygiene                               |                          |

`AI_NOTES`, README answers, Part B, and “can you change this code” are **in scope** — just **not yet**. Don’t start them until the learner asks or code steps are ready.

## How to work with the learner

- Explain requirements; do **not** solve whole steps unless asked.
- If asked to implement something: do it, then **explain** the C# ideas.
- Infrastructure / DI / config boilerplate may be done for them with explanation.
- Core learning pieces (interfaces, session logic, tests): guide first; fill in only when they ask.
- Stay on **`net11.0`** (assignment says .NET 8; learner chose net11 for practice).
- Prefer Hebrew explanations only if the learner asks; otherwise English is fine.

## Solution layout

```
MyProject/
  docs/
    AGENTS.md                     # this file
  BankFoundation.slnx
  BankFoundation/                 # Console app (Exe), net11.0
    Program.cs                    # Host + DI + config wiring
    appsettings.json              # Queue:Type = Sim | DATQ
    Queues/
      IMessageQueue.cs
      DatqMessageQueue.cs         # fake DATQ (console)
      SimulatorMessageQueue.cs    # in-memory List + Messages property
    Session/
      SessionState.cs             # enum incl. Reconnecting (ns BankFoundation.Sessions)
      Session.cs                  # transition map + ChangeState + StateChanged
    Audit/
      IAuditMessage.cs
      FileAudit.cs                # file audit (local hardcoded path Audits.txt)
      InMemoryAudit.cs
      AuditEntry.cs
  BankFoundation.Tests/           # xUnit, references BankFoundation
```

## Assignment steps (Part A) — progress

| #   | Topic                                            | Status                          |
| --- | ------------------------------------------------ | ------------------------------- |
| 1   | Code review of sample ChannelManager (≥4 issues) | Done (in README)                |
| 2   | Queue interface + DATQ + Simulator               | Done                            |
| 3   | Config + DI choose queue                         | Done                            |
| 4   | Session states + allowed transitions             | Done                            |
| 5   | Audit interface + implementations                | Done (file + in-memory)         |
| 6   | xUnit tests (4 required)                         | Done                            |
| 7   | README: abstract class vs interface              | Done (in README)                |
| 8   | Add Reconnecting state + tests                   | Done                            |
| 9   | Send while Suspended (choose behavior + test)    | Done (buffer + flush on Active) |

Later (relevant): `AI_NOTES.md`, practice changing code without AI. Part B `ANSWERS.md` — Done.
README scaffold at repo root (`README.md`) — run/switch-queue filled; answer sections blank for learner.

Sender glue: `MessageSender` (Active → queue + audit) — Done.

## Important design choices already made

- Queues in `BankFoundation.Queues`: `IMessageQueue`; DI registers one concrete type from `Queue:Type`.
- Simulator stores cloned messages only (no console); exposes `Messages` for tests.
- Session: enum + `Dictionary` transition table; `ChangeState` returns bool; starts `Disconnected`.
- `Session.StateChanged` event; `MessageSender` flushes pending buffer when state becomes `Active`.
- Audit interface: `IAuditMessage.Record(byte[] message)`; `FileAudit` / `InMemoryAudit`.
- Namespaces: `BankFoundation`, `.Queues`, `.Sessions`, `.Audit`.

- Host uses `ContentRootPath = AppContext.BaseDirectory` so `appsettings.json` loads from output dir.

### TODO (deferred)

- Pending buffer when leaving Suspended without returning to Active (e.g. LoggingOut/Disconnected): clear, fail, or timeout — not implemented yet.

## Next likely work

1. `AI_NOTES.md`.
2. Optional: polish README “what I didn’t finish”.
3. Deferred: pending-buffer policy when Suspended never returns to Active.

## Do not

- Rewrite working learner code without asking.
- Jump ahead and implement later steps (6–9, Part B, AI_NOTES) unsolicited.
- Change target framework away from `net11.0` unless asked.
- Treat grading percentages or time estimates as requirements.
