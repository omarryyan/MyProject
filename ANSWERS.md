# Part B — Reliability thinking

Context: a component receives ~1,000 messages/sec from the exchange. Each message has an increasing sequence number. The component writes every message to the bank queue and to a DB table. Messages must not be lost, and must not appear twice.

---

## 1. Component restarts mid-day

**How does it know which message to continue from? Where is that stored?**

I would keep a **checkpoint** in the database: the sequence number of the last message that was fully handled successfully. On startup the component reads that checkpoint and resumes from **checkpoint + 1**.

“Fully handled” should be defined clearly — for example, after the row is in the DB **and** the bank queue write is done (or after whichever step we treat as the commit point). The checkpoint is updated only when that commit point succeeds, so a crash does not advance past work that did not finish.

To avoid losing messages that arrived but were not yet checkpointed, I would also **persist incoming messages** in durable storage (e.g. a DB inbox table or an append-only durable log/queue) before or as part of processing, so a restart can replay from storage rather than relying on memory alone.

---

## 2. Crash after DB write, before “processed” is saved

**What happens on restart? How do you prevent duplicates in the table?**

On restart the component will likely **retry** the same sequence. If that message was already inserted into the DB, a normal insert would duplicate the row.

To prevent that, I would use the message **sequence number as the primary key (or a UNIQUE constraint)** on the messages table. A retry then hits a uniqueness violation; the component treats that as **idempotent success** (skip / continue) instead of inserting again or failing the pipeline.

I would also keep clear progress markers, for example:
- what has been **persisted** to the DB, and  
- what has been marked **fully processed** (including bank queue / checkpoint),  

so a crash between “written to DB” and “marked done” is safe: restart re-attempts processing, the UNIQUE constraint blocks a second row, and we finish the remaining steps.

For the **bank queue**, I would rely on a unique message id / sequence and **idempotent** consumers on the bank side (or dedupe there) so a second send of the same logical message does not cause a duplicate business operation.

---

## 3. Database unavailable for a few minutes

**Keep sending to the bank queue? What about messages not written to the DB?**

I would **not** keep sending to the bank queue while the DB is down if we require the DB row and the bank message to stay aligned. Instead I would accept inbound messages into a **durable local queue** (or similar durable buffer), apply **backpressure** if that buffer grows too large, and **pause** bank sends until the DB can accept writes again.

When the DB recovers, the component drains the durable buffer: write each message to the DB, send to the bank queue, and advance checkpoints as usual.

**Tradeoff:** the bank may lag during the outage (availability of bank updates is delayed), but we avoid the harder case where the bank is ahead of the DB and we must reconcile later.  

**Alternative** (not chosen here): keep sending to the bank and catch up the DB later — better bank freshness, higher risk of inconsistency and more complex recovery.

Monitoring durable-queue depth and alerting on growth is part of making this safe in practice.

---
