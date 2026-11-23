# BankFoundation — Agent Context

Read this file at the start of every session. Keep it updated when progress changes.

## What this project is

Practice rewrite of a Junior C# / .NET take-home assignment: a bank **Foundation** layer that sends messages to a queue, tracks exchange **Session** state, and writes **Audit** records.

- Goal: the learner **codes while understanding**. Prefer guidance over full solutions.
- Full assignment text: `../מבחן-בית-Junior-CSharp-NET.md` (extracted from the PDF).
- Remote (learner pushes): `https://github.com/omarryyan/MyProject.git`

### What is / isn’t relevant

| Relevant | Not relevant |
| -------- | ------------ |
| Part A code (done), understanding & changing code (defense practice) | Formal grading / weights |
| Part B `ANSWERS.md` (done) | Timeline / hours spent |
| `AI_NOTES.md` (done), `README.md` | |
| Optional polish / deferred TODOs when learner asks | |

## How to work with the learner

- Explain requirements; do **not** solve whole steps unless asked.
- If asked to implement something: do it, then **explain** the C# ideas.
- Infrastructure / DI / config boilerplate may be done for them with explanation.
- Core learning pieces: guide first; fill in only when they ask.
- Stay on **`net11.0`** (assignment says .NET 8; learner chose net11 for practice).
- Prefer Hebrew explanations only if the learner asks; otherwise English is fine.
- **Do not commit or push** unless the learner explicitly asks.

## Solution layout

```
MyProject/
  docs/
    AGENTS.md                     # this file — always read first
    event-refactor-guide.md       # historical guide for StateChanged refactor
  .cursor/rules/
    bankfoundation-practice.mdc   # alwaysApply coaching rule (points here)
  README.md                       # run, switch queue, review, design answers
  ANSWERS.md                      # Part B reliability answers
  AI_NOTES.md                     # tools + rejected AI suggestion example
  BankFoundation.slnx
  BankFoundation/                 # Console app (Exe), net11.0
    Program.cs                    # Host + DI + demo
    MessageSender.cs              # ns BankFoundation; Active/Suspended send
    appsettings.json              # Queue:Type = Sim | DATQ
    Queues/                       # ns BankFoundation.Queues
    Session/                      # ns BankFoundation.Sessions
    Audit/                        # ns BankFoundation.Audit; FileAudit path hardcoded locally
  BankFoundation.Tests/           # xUnit
```

## Assignment status — COMPLETE for core deliverables

| # | Topic | Status |
| - | ----- | ------ |
| 1 | Code review (≥4 issues) | Done — README |
| 2 | Queue interface + DATQ + Simulator | Done |
| 3 | Config + DI choose queue | Done |
| 4 | Session states + transitions | Done |
| 5 | Audit interface + file/in-memory | Done |
| 6 | xUnit tests (4+) | Done (7 tests) |
| 7 | Abstract class vs interface | Done — README |
| 8 | Reconnecting + tests | Done |
| 9 | Send while Suspended (buffer + flush) | Done |
| — | Part B ANSWERS.md | Done |
| — | AI_NOTES.md | Done |
| — | Sender glue + StateChanged flush | Done |

## Important design choices

- Queues behind `IMessageQueue`; DI picks Sim/DATQ from `Queue:Type`.
- Session: enum + transition dictionary; `ChangeState` returns bool; starts `Disconnected`.
- `Session.StateChanged`; `MessageSender` flushes `_pending` when state becomes `Active`.
- Suspended `Send` buffers clones; other non-Active states reject (`false`).
- `FileAudit` uses hardcoded local path `...\Audit\Audits.txt` with `append: true` (intentional for one PC).
- Host: `ContentRootPath = AppContext.BaseDirectory` for `appsettings.json`.

### TODO (deferred — only if learner asks)

- Pending buffer when leaving Suspended **without** returning to Active (LoggingOut/Disconnected): clear, fail, or timeout.
- README section “What I didn’t finish / would do differently” may still be empty — fill when asked.
- Optional: relative/configurable audit path; extra tests (buffer order, Reconnecting reject send).

## Next likely work

1. Fill README “what I didn’t finish” (if learner wants).
2. Defense practice: explain code / small change without AI.
3. Deferred pending-buffer policy (if learner wants).

## Do not

- Rewrite working learner code without asking.
- Implement deferred TODOs or extras unsolicited.
- Change target framework away from `net11.0` unless asked.
- Treat grading percentages or time estimates as requirements.
- Commit or push unless the learner explicitly asks.
