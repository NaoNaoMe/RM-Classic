# RM Classic — Remote Interface Guide (for Scripts & AI Agents)

RM Classic exposes a TCP text interface so a script or AI agent can read/write
live MCU variables and stream them as time-series logs, without touching the
GUI or the C# source. The protocol itself is deliberately simple — a client
needs nothing beyond stdlib sockets in any language.

## Quick start

The wire format (full details in `RM_Protocol_Specification.md`): send one
command per line (`\r\n`-terminated), get back one line of JSON —
`{"status":"ok"[,"data":...]}` or `{"status":"error","reason":"..."}`. A
minimal Python client is a dozen lines:

```python
import socket, json

sock = socket.create_connection(("127.0.0.2", 4001))

def send(command):
    sock.sendall(command.encode() + b"\r\n")
    line = b""
    while not line.endswith(b"\r\n"):
        line += sock.recv(4096)
    reply = json.loads(line)
    if reply["status"] != "ok":
        raise RuntimeError(reply["reason"])
    return reply.get("data")

print(send("help"))
```

**No hardware yet?** A companion MCU-simulator mock speaks the MCU side of
this same protocol over TCP/LocalNet — point `connect` at it instead of a
real serial port, and a procedure built against it carries over to real
hardware unchanged. Published at https://github.com/NaoNaoMe/rm_mock.

## Discover the interface at runtime — don't hardcode it

The command set, argument formats, and current session state are all
queryable live, and are the authoritative source — more so than this document,
which does not track field-by-field detail (that duplication has gone stale
before). Query them instead of assuming:

- **`help`** → JSON catalog of every command: name, args, a description
  (including any field-level detail, e.g. `status`'s), and whether it requires
  an open link or stops logging.
- **`status`** → current session state as JSON (open/logging/current page,
  timestep, loaded view/map, etc).
- **`symbols`** → the resolved symbol table (name → address/offset/size).
- **`view`** → the current view page's rows exactly as a human sees them in
  the grid (1-based, including blank/unchecked rows) — use this to resolve
  "the Nth row" unambiguously when picking up a session from a human.

```python
print(send("help"))    # what can I do, and how
print(send("status"))  # what's the current state
```

## The view (rmxml) is the meaning layer — work by name, not address

A `.rmxml` view groups MCU variables into pages and gives each row a human
`Name`, `Description`, a basic `Type` (`Bin`/`Dec`/`UsD`/`Hex`/`FLT`/`DBL`),
and optionally a preset `WriteValue`. A row with both `Name` and `WriteValue`
is a **named write macro** — a one-shot semantic action (e.g. an enable/disable
toggle) triggerable by name alone, without knowing its address or value. The
paired `.rmmap` supplies the actual symbol→address resolution and is
regenerated on every firmware build, so **write scripts against names, not
addresses** — a rebuild will not break them. The mock repo above ships a
concrete, minimal example of this shape (`MockTest--RmSample.rmxml` /
`MockTest.rmmap`: `Group` = page, `Check` = logged — a row can still be
written while unchecked, `Name`+`WriteValue` = macro, `Description` = human
context).

If you generate or edit a view programmatically, read the CURRENT live state
with `downloadviewdata` (not the on-disk file, which can be stale relative to
unsaved GUI edits) before round-tripping it back with `uploadviewdata`.

## Operational patterns

- **Logging persists once started.** `logstart` is the only thing that starts
  it; it is never auto-resumed. `config`/`timestep`/`dump` each stop logging
  as a side effect if it was active — call `logstart` again afterward if you
  need it back. You do not need `logstop` between drains, only when you
  actually want to stop (commonly paired with `disconnect`).
- **Drain-to-empty, not one row at a time.** Use `logreadall` to pull every
  currently-queued row in one round-trip, on a cycle faster than your
  configured `timestep`. The buffer (`status().logLength`) is a generous cap,
  not a latency knob — if it fills, the OLDEST undrained rows are silently
  discarded to make room for new ones.
- **Always `disconnect` when done.** It releases the underlying serial/TCP
  resource; skipping it blocks the next `connect` (including a later script's).
- **The map auto-resync that a human GUI session gets on Comm-Open is
  suppressed for remote sessions** (a remote client would get no event for a
  silent one). Call `resyncmap` explicitly after loading/changing a map, or
  any time addresses may have shifted after a rebuild.

## Writing safe, unattended scripts

- **Ask the user for environment-specific inputs — never guess.** The COM
  port, IP/port of a bridge, baud rate, and pass key depend on physical setup
  the tool cannot discover. Scanning/probing for them is not a substitute for
  asking.
- **RM Classic reports only what it can actually observe.** A dead link and a
  powered-off MCU look identical from here; do not read more into an error
  `reason` than it states.
- **Safety limits live on the MCU, not in your script.** A script that hangs,
  disconnects, or sends a bad value should be caught by firmware-side
  invariants (range clamps, a watchdog/heartbeat), not by PC-side discipline —
  the PC side is not guaranteed to be fast or reliable. Before trusting a
  limit in an unattended run, deliberately trigger the condition once and
  confirm it actually trips.

## References

- Full wire protocol: `RM_Protocol_Specification.md`
- Hardware-free mock: https://github.com/NaoNaoMe/rm_mock
