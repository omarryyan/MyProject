# BankFoundation

Practice project: bank Foundation layer — message queue (DATQ / Simulator), session states, and audit.

## How to run

```bash
dotnet build
dotnet run --project BankFoundation
dotnet test
```

## How to switch queue type

Edit `BankFoundation/appsettings.json`:

```json
{
  "Queue": {
    "Type": "Sim"
  }
}
```

Use `"Sim"` or `"DATQ"`. No code change needed — DI picks the implementation at startup.

---

## Step 1 — Code review (sample `ChannelManager`)

Find at least **4 problems** in the assignment’s sample code. For each: what is wrong, and what can go wrong in practice.

1. **Problem:** `QueueType` is `static`, so it is shared by every `ChannelManager` instance.  
   **What can go wrong:** Changing it for one use (e.g. to `"SIM"` in a test) changes it for everyone else — mixed queues, flaky tests, or production traffic using the wrong queue.

2. **Problem:** Server name, user, and password are hardcoded (`"AS400PRD"`, `"user"`, `"P@ss"`) instead of coming from configuration.  
   **What can go wrong:** Secrets end up in source control; you can’t switch environments without rebuilding; anyone with the code has the password; rotating credentials requires a code change.

3. **Problem:** `State` is a public `string` — no enum and no enforced transition rules. Callers can set any value (e.g. jump straight to `"Active"`).  
   **What can go wrong:** Illegal transitions, silent typos (`"Actve"`), and bypassing the real login/connect lifecycle.

4. **Problem:** Every `Send` creates a new `DatqClient(...)` instead of reusing one injected/shared client.  
   **What can go wrong:** Extra connect/teardown cost on every message; harder to mock or test; `ChannelManager` stays tightly coupled to how the client is constructed.

_(Add more if you want.)_

---

## Step 4 — How Session states were implemented

Session uses a `SessionState` **enum** plus a **dictionary transition table** (`Dictionary<SessionState, HashSet<SessionState>>`): the key is the current state, the value is the set of allowed next states. `ChangeState` checks that table and only updates state when the move is allowed (returns `true`/`false`).

Why this approach: it matches the assignment table directly, keeps all rules in one place, and makes adding a state later (e.g. `Reconnecting`) mostly a matter of updating the enum and the map — without a large chain of `if`s or a full State Pattern (one class per state), which would be heavier for this size of problem.

---

## Step 7 — Abstract class vs interface

**When would you choose an abstract class?**  
When implementations are related and should **share real code** (common fields/methods) or a partial “template” (base class runs shared steps; subclasses fill in the rest). A type can inherit only one class, so this fits a clear is-a hierarchy.

**When would you choose an interface?**  
When you need a **contract** for a capability that can have **multiple independent implementations**, especially for DI and testing. A class can implement many interfaces. You care about _what_ it can do, not a shared base implementation.

**Example from this project:**  
`IMessageQueue` is an interface: `DatqMessageQueue` and `SimulatorMessageQueue` both “can send,” but they don’t share important base code — so an interface + DI fits. An abstract class would make more sense if several queue types needed the same shared helpers (e.g. common logging/retry in a `QueueBase`); we didn’t need that here. Same idea for `IAuditMessage` (file vs in-memory).

---

## Step 8 — Reconnecting

**How many places in the code did you change to add the state, and why?**  
Three main places in production code: (1) `SessionState` enum — add `Reconnecting`; (2) `Session` transition map — allow Active/Suspended → Reconnecting, and Reconnecting → LoggingIn/Disconnected; (3) tests — one legal and one illegal transition. No changes to `MessageSender`, queues, or audit were required, because send already keys off `Active` only. That shows the table-based design localizes state rules instead of scattering them across many methods.

---

## Step 9 — Send while Suspended

**Chosen behavior:**  
While `Suspended`, `Send` **buffers** a copy of the message in memory (does not hit the queue/audit yet). When the session returns to `Active` via `Session.ChangeState`, `Session` raises `StateChanged`; `MessageSender` listens and **flushes** the buffer (each message is sent to the queue and audited, in order).

**Why:**  
Suspended is a pause, not a disconnect — work may still arrive and should not be silently lost. Buffering preserves messages until trading/sending is allowed again.

**Downside:**  
If the session never becomes Active again, messages stay in memory (undelivered, memory can grow). A fuller design would add timeout, alerting, or clearing the buffer on logout/disconnect.

---

## What I didn’t finish / would do differently

---

## Time spent _(optional)_

Approx. hours:
