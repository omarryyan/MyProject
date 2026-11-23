# Guide: Session event → flush buffer (you implement)

No new `.cs` files needed. Change these four places.

Goal: remove `MessageSender.ChangeState`. After a successful `Session.ChangeState`, if the new state is `Active`, `MessageSender` flushes the buffer by listening to an event.

---

## 1. `BankFoundation/Session/Session.cs`

**Add**
- A public **event** that fires only after a **successful** transition.
- Suggested shape (you choose exact names):
  - `public event Action<SessionState>? StateChanged;`  
    (handler receives the **new** state), or
  - `public event Action<SessionState, SessionState>? StateChanged;`  
    (old state, new state)

**Change `ChangeState`**
1. Remember the old state (optional if you only pass the new one).
2. If transition not allowed → return `false` (do **not** raise the event).
3. If allowed → update `state`, then **invoke** the event, then return `true`.

**Hint — raise an event safely:**
```csharp
StateChanged?.Invoke(newState);
```

`Session` must not mention `MessageSender`, queues, or flush. It only announces “state changed.”

---

## 2. `BankFoundation/MessageSender.cs`

**In the constructor** (you already receive `Session session`)
- Subscribe: `_session.StateChanged += ...` your handler method.

**Add a private handler** e.g. `OnSessionStateChanged(...)`
- If the new state is `Active` → call your existing `FlushPending()`.
- Otherwise do nothing.

**Remove**
- The whole `MessageSender.ChangeState` method.

**Keep**
- `Send`, `_pending`, `FlushPending`, `Deliver`, `PendingCount`, `Session` property.

**Optional but good**
- If the process lived forever you’d unsubscribe; for this app, constructor subscribe is enough.

---

## 3. `BankFoundation/Program.cs`

Replace every `sender.ChangeState(...)` with:

```csharp
sender.Session.ChangeState(...);
```

Flush should still happen because creating `MessageSender` via DI already subscribed to the event.

---

## 4. `BankFoundation.Tests/MessageSenderTests.cs`

Same idea: use `session.ChangeState(...)` or `sender.Session.ChangeState(...)` instead of `sender.ChangeState(...)`.

Order that must still work:
1. Create `session`, then `new MessageSender(session, queue, audit)` (**sender must exist before** transitions you care about, so it is already subscribed).
2. Drive to Active → Suspended → `Send` (buffers) → `session.ChangeState(Active)` → assert flush.

`SessionTests.cs` can stay as-is (they only use `Session`).

---

## Check yourself

```bash
dotnet test
dotnet run --project BankFoundation
```

- Suspended send → `PendingCount == 1`, queue empty  
- Then `session.ChangeState(Active)` → pending 0, message in queue + audit  

---

## Mini mental model

```
session.ChangeState(Active)
        │
        ▼
 Session raises StateChanged(Active)
        │
        ▼
 MessageSender handler → FlushPending()
```

When you’re stuck on syntax, ask — don’t ask for the full pasted solution unless you want it.
