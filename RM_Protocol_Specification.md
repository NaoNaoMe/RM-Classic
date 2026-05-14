# RM Protocol Specification

**Version 1.0** | **Status: Open Source Reference**

---

## Table of Contents

1. [Overview](#overview)
2. [Transport Layer](#transport-layer)
3. [Constants](#constants)
4. [CRC-8](#crc-8)
5. [Frame Classification](#frame-classification)
6. [OpCode Encoding](#opcode-encoding)
7. [Address Width Configuration](#address-width-configuration)
8. [Instructions](#instructions)
   - [ReadInfo](#readinfo)
   - [StartLog](#startlog)
   - [StopLog](#stoplog)
   - [SetTimeStep](#settimestep)
   - [Write](#write)
   - [SetAddr](#setaddr)
   - [ReadDump](#readdump)
9. [Log Frame](#log-frame)
10. [Derived Frame](#derived-frame)
11. [Command Behavior During Logging](#command-behavior-during-logging)
12. [Session State Machine](#session-state-machine)
13. [Timing Parameters](#timing-parameters)

---

## Overview

RM is a lightweight binary protocol for real-time variable monitoring and memory access over a
point-to-point serial link between a host PC application and an embedded controller (MCU).

- **Topology:** Master–slave. The host is always the master; the MCU is always the slave.
- **Direction naming:** `Host → MCU` for commands, `MCU → Host` for responses and log frames.
- **Byte order:** Little-endian for all multi-byte integers unless stated otherwise.
- **Transport:** SLIP (Serial Line Internet Protocol, RFC 1055) framing is used over the physical
  serial link. All byte sequences described in this document are the **payload before SLIP encoding (TX) or after SLIP decoding (RX)**.

---

## Transport Layer

Each logical frame described in this document is transmitted as a SLIP-encoded packet.
SLIP wraps the raw byte sequence with `END` bytes (`0xC0`) and escapes two special byte values:

| Raw byte | SLIP encoding |
|----------|---------------|
| `0xC0`   | `0xDB 0xDC`   |
| `0xDB`   | `0xDB 0xDD`   |

All frame layouts in this document refer to the **payload inside the SLIP packet** — after
decoding on receive, before encoding on transmit.

---

## Constants

| Name             | Value | Description                                          |
|------------------|-------|------------------------------------------------------|
| `MaxPayloadSize` | 128   | Maximum payload bytes in a single ReadDump reply     |
| `MaxElementNum`  | 32    | Maximum number of log variables in one SetAddr session |

---

## CRC-8

All command and response frames (except Derived frames) are protected by a CRC-8 appended
as the **last byte** of the frame.

| Parameter       | Value                                        |
|-----------------|----------------------------------------------|
| Algorithm       | CRC-8                                        |
| Polynomial      | `0xD5` (x⁸ + x⁷ + x⁶ + x⁴ + x² + 1)        |
| Initial value   | `0x00`                                       |
| Input/Output    | Non-reflected (MSB-first)                    |
| Scope           | All bytes of the frame **excluding** the CRC byte itself |
| Validation      | `CRC8(all bytes including CRC byte) == 0x00` |

> **Implementation note:** Generate the 256-entry lookup table at compile time or startup.
> For each index `i` (0–255): initialize `temp = i`, then perform 8 rounds of `if (temp & 0x80) temp = (temp << 1) ^ 0xD5; else temp <<= 1;`, keeping only the low 8 bits each round.
> To compute CRC over a byte sequence, initialize `crc = 0x00` then for each byte `b`: `crc = table[crc ^ b]`.

> **MCU behavior:** The MCU validates incoming command frames by computing `CRC8(all bytes) == 0x00`.
> Frames failing this check are silently discarded. The MCU appends CRC to every response frame.
>
> **Host behavior:** The host appends CRC to every command frame it transmits and validates
> `CRC8(rxFrame, all bytes) == 0x00` on every response frame received.

---

## Frame Classification

The **first byte** of every received frame (`frame[0]`) determines the frame type
without requiring any additional context:

```
frame[0] == 0x00              →  Derived frame   (no CRC)
0x01 <= frame[0] <= 0x0F      →  Log frame        (upper nibble == 0x0, SlvCnt in lower nibble)
frame[0] >= 0x10              →  Command response  (MasCnt in upper nibble, SlvCnt in lower nibble)
```

Or equivalently using bitmasks:

```
frame[0] == 0x00                        → Derived frame
(frame[0] & 0xF0) == 0x00 && frame[0] != 0x00   → Log frame
(frame[0] & 0xF0) != 0x00               → Command response
```

**Why this is unambiguous:**

- Command responses always echo the host's `MasCnt` in bits 7–4. `MasCnt` ranges from `0x10` to
  `0xF0` (multiples of 16), so `(frame[0] & 0xF0)` is never `0x00` for a command response.
- Log frames are emitted by the MCU with the upper nibble fixed to `0x0` and `SlvCnt` cycling
  1–15, so `frame[0]` is always in range `0x01`–`0x0F`.
- Derived frames are identified by exactly `frame[0] == 0x00`, which never appears in the other
  two types.

> **MCU behavior:** The MCU produces frames in all three categories. Log frames and derived frames
> are generated autonomously (not in response to a host request). Command responses are generated
> only when a valid authenticated command is received. The MCU never inspects `frame[0]` of
> its own output for classification — it constructs each type directly.
>
> **Host behavior:** The host applies this classification rule to every decoded frame it receives,
> routing each one to the appropriate handler (response validator, log parser, or SCE receiver).

---

## OpCode Encoding

Every command frame sent by the host begins with a 1-byte **OpCode**:

```
OpCode = MasCnt | InstrCode

MasCnt   : bits 7–4 (range 0x10–0xF0, always a multiple of 0x10)
InstrCode: bits 3–0 (range 0x01–0x07; identifies the instruction)
```

`MasCnt` increments by `0x10` per request and wraps `0xF0 → 0x10` (skipping `0x00`).
Because `MasCnt` is always a multiple of 16 and `InstrCode ≤ 7`, addition and bitwise OR are
equivalent.

> **Host behavior:** Maintains `MasCnt` as session state. Increments by `0x10` before each
> request, wrapping `0xF0 → 0x10`. Combines `MasCnt | InstrCode` to form the OpCode byte.
>
> **MCU behavior:** Reads `MasCnt` from bits 7–4 of the received OpCode and echoes it unchanged
> in bits 7–4 of the response's first byte. The MCU does not maintain its own `MasCnt` counter.

The host validates this echo as part of response authentication:

```
(rxFrame[0] & 0xF0) == (txFrame[0] & 0xF0)
```

### Instruction Codes

| Constant      | `InstrCode` | Description                                |
|---------------|-------------|--------------------------------------------|
| `StartLog`    | `0x01`      | Enter log mode / keepalive                 |
| `StopLog`     | `0x02`      | Exit log mode                              |
| `SetTimeStep` | `0x03`      | Set log interval                           |
| `Write`       | `0x04`      | Write value to MCU memory address          |
| `SetAddr`     | `0x05`      | Configure log variable address table       |
| `ReadInfo`    | `0x06`      | Connection handshake / read device name    |
| `ReadDump`    | `0x07`      | Read raw memory block                      |

> **Note:** InstrCode values are defined from the MCU firmware perspective and must match
> exactly between the host and MCU implementations.

### Response Validation Rules

> **Host behavior:** The host applies all of the following checks to every received response frame.
> A frame is accepted only when all checks pass.

A received response frame is valid if **all** of the following hold:

1. `CRC8(rxFrame, all bytes) == 0x00`
2. `(rxFrame[0] & 0xF0) == (txFrame[0] & 0xF0)` — MasCnt echo matches the request.
3. `rxFrame != txFrame` (byte-for-byte) — if equal, **EchoDetected = true** (see [ReadInfo](#readinfo)).


---

## Address Width Configuration

The address field width in `Write`, `SetAddr`, and `ReadDump` frames is fixed at startup:

| Mode     | Value | Address field width |
|----------|-------|---------------------|
| `Byte2`  | 0     | 2 bytes (uint16 LE) |
| `Byte4`  | 1     | 4 bytes (uint32 LE) — default |

> **MCU behavior:** The address width is selected at compile time (`RM_ADDR_2BYTE` /
> `RM_ADDR_4BYTE`). The MCU parses address fields from incoming frames using this fixed width.
>
> **Host behavior:** The host must be configured to use the same address width as the connected
> MCU. Mismatched width causes silent address truncation or padding errors.

Typical mapping:
- **AVR** (16-bit data pointer): `Byte2`
- **ARM Cortex-M**, **RH850**, others: `Byte4`

---

## Instructions

Each section describes the TX frame layout (host → MCU) as an ordered byte list.
`[CRC]` denotes the CRC-8 byte appended at the end.

---

### ReadInfo

**Purpose:** Initial handshake. The host verifies connectivity and reads the MCU device name.
This is the first command sent after opening the port.

#### Request Frame

```
[ OpCode(ReadInfo) | passNumber[0..3] | CRC ]
  1 byte             4 bytes (uint32 LE) 1 byte
Total: 6 bytes
```

- `passNumber`: A 32-bit unlock key configured in both the host and the MCU firmware.
  Only requests with a matching key are accepted by the MCU.

> **MCU behavior:** Validates the received `passNumber` against its compile-time key. Silently
> discards the frame if the key does not match. On a successful match, sets `is_approved = true`,
> which enables command processing and the SCE channel. If the MCU was logging, logging is stopped.
> Responds with the version string.

#### Response Frame

```
[ OpCode_echo | version[0..N] | CRC ]
  1 byte        N+1 bytes (null-terminated ASCII)  1 byte
```

- `version`: Null-terminated ASCII string identifying the MCU firmware build or purpose.


#### Example

```
passNumber = 0x0000FFFF, MasCnt = 0x10
OpCode = 0x10 | 0x06 = 0x16

05:56:19.816 Tx 16-FF-FF-00-00-5D
05:56:19.864 Rx 12-52-6D-53-61-6D-70-6C-65-00-19

TX: 16 FF FF 00 00 5D
RX: 12 52 6D 53 61 6D 70 6C 65 00 19
       R  m  S  a  m  p  l  e  \0
```

---

### StartLog

**Purpose:** Commands the MCU to begin emitting periodic log frames.
Also used as a **keepalive** every 500 ms while the host is in the logging state.

#### Request Frame

```
[ OpCode(StartLog) | CRC ]
  1 byte              1 byte
Total: 2 bytes
```

#### Response

- **Initial StartLog**: The MCU responds with a normal command response.
- **Keepalive StartLog (during logging)**: The MCU sends **no response (NR)**.
  The host sends these keepalive frames fire-and-forget.

> **MCU behavior:** On the first StartLog (when not yet logging), enters logging mode, starts
> emitting log frames at the configured interval, and sends a normal response. On subsequent
> StartLog frames received while already logging, resets its internal keepalive timer
> (`KEEPALIVE_MS` = 2000 ms) and sends **no response**. If no keepalive is received within
> 2000 ms, the MCU stops logging autonomously.
>
> **Host behavior:** Sends the initial StartLog. Once the `Logging` task begins,
> re-sends StartLog every `KEEPALIVE_INTERVAL` (500 ms) on an independent timer.
> The keepalive interval is fixed regardless of the configured log emission interval (`timeStep`).
> Log frames are received asynchronously between keepalives.

#### Example

```
MasCnt = 0x40
TX: 41 48
RX: 4E 47   ← (rxFrame[0] & 0xF0) = 0x40 ✓, SlvCnt = 0x0E = 14
```

---

### StopLog

**Purpose:** Commands the MCU to stop emitting log frames and return to command–response mode.

#### Request Frame

```
[ OpCode(StopLog) | CRC ]
  1 byte             1 byte
Total: 2 bytes
```

> **MCU behavior:** Clears `is_logging`. Stops log frame emission immediately and sends a normal
> command response.
>
> **Host behavior:** Sends StopLog and waits for confirmation before closing the session.

#### Example

```
MasCnt = 0xF0
TX: F2 FB
RX: F5 AF   ← (rxFrame[0] & 0xF0) = 0xF0 ✓, SlvCnt = 0x05
```

---

### SetTimeStep

**Purpose:** Sets the log emission interval on the MCU.

#### Request Frame

```
[ OpCode(SetTimeStep) | timeStep[0..1] | CRC ]
  1 byte                2 bytes (uint16 LE)  1 byte
Total: 4 bytes
```

- `timeStep`: Interval in milliseconds. Valid range: 1–65535.
  Frames with `timeStep == 0` are not accepted by the MCU.

> **MCU behavior:** Stores the new interval. If currently logging, stops logging immediately
> (`is_logging = false`) before applying the new value. Sends a normal command response.
>
> **Host behavior:** On receipt of a successful response, stores the confirmed interval
> for use in computing relative timestamps from `SlvCnt` values in subsequent log frames.

#### Example

```
500 ms, MasCnt = 0x70
TX: 73 F4 01 F7   ← 0x01F4 = 500 in LE
RX: 7D 69
```

---

### Write

**Purpose:** Overwrites a memory location in the MCU.

#### Request Frame

```
[ OpCode(Write) | size | addr[0..N-1] | value[0..size-1] | CRC ]
  1 byte          1 byte  2 or 4 bytes   1/2/4/8 bytes      1 byte
```

| Field   | Description                                                  |
|---------|--------------------------------------------------------------|
| `size`  | Data width in bytes. Must be one of: `1`, `2`, `4`, `8`.    |
| `addr`  | Target address (LE). Width = 2 or 4 bytes per address mode. |
| `value` | Value to write (LE), exactly `size` bytes.                  |

> **Note:** 8-byte (`uint64`) support requires the MCU firmware to be compiled with `RM_SUPPORT_64BIT`.

#### Response

- **Outside logging mode**: Normal command response.
- **During logging mode**: **No response (NR)**. The write is executed silently; logging continues.

> **MCU behavior:** Writes the value to the specified address at the requested width.
> During logging mode, executes the write silently without sending a response and without
> interrupting log emission (logging continues at the configured interval).
>
> **Host behavior:** During the `Logging` task, enqueues Write frames in an internal
> `DataRequest` queue. The logging loop dequeues and sends them fire-and-forget (no response
> validation).

#### Example (Byte4, size=1, addr=0xFEF0214C, value=0x00, MasCnt=0xF0)

```
TX: F4 01 4C 21 F0 FE 00 1A
     ^  ^  ^-----------^  ^----- value (1 byte)
     |  |  addr (4 bytes LE)
     |  size = 1
     OpCode = 0xF4 (MasCnt=0xF0, Write)
```

---

### SetAddr

**Purpose:** Configures the MCU's log variable address table — specifying which memory addresses
(and their data widths) to sample in each log frame.

If the configuration requires more bytes than fit in a single frame, it is split into multiple
SetAddr frames sent sequentially. Each frame is individually acknowledged.

#### Request Frame

```
[ OpCode(SetAddr) | frameCode | element_0 | element_1 | ... | CRC ]
  1 byte             1 byte     variable length               1 byte
```

#### `frameCode` Bit Layout

| Bit(s) | Mask   | Meaning                              |
|--------|--------|--------------------------------------|
| 5      | `0x20` | **End** flag — set on the last frame |
| 4      | `0x10` | **Begin** flag — set on the first frame |
| 3–0    | `0x0F` | Frame index (0-based)                |

Single-frame configuration: `frameCode = 0x30` (Begin + End + index 0).

#### Element Format (per variable)

```
[ size_byte | addr[0..N-1] ]
  1 byte      2 or 4 bytes per address mode
```

- `size_byte`: `1`, `2`, `4`, or `8`.
- Elements are packed consecutively without padding.

#### Frame Capacity

| Address mode | Bytes per element | Max elements per frame | Element bytes per frame |
|--------------|-------------------|------------------------|-------------------------|
| `Byte4`      | 1 + 4 = 5 bytes   | 4                      | 20                      |
| `Byte2`      | 1 + 2 = 3 bytes   | 8                      | 24                      |

#### Example (Byte2, 4 elements, single frame, MasCnt=0x20)

```
Variables:
  {size=1, addr=0x0109}  isCounting
  {size=2, addr=0x0392}  count
  {size=2, addr=0x0390}  debounceCount
  {size=1, addr=0x038F}  isLEDOn
frameCode = 0x30 (Begin | End | idx=0)

TX: 25 30 01 09 01 02 92 03 02 90 03 01 8F 03 B1
     ^  ^  ^-----^  ^-----^  ^-----^  ^-----^  ^-- CRC
     |  |  elem 0   elem 1   elem 2   elem 3
     |  frameCode = 0x30 (Begin=1, End=1, idx=0)
     OpCode = 0x25 (MasCnt=0x20, SetAddr)

RX: 23 0E   ← (rxFrame[0] & 0xF0) = 0x20 ✓, SlvCnt = 3
```

#### Multi-frame Example (Byte4, 18 variables across 5 frames)

```
Frame 1: TX 25-10-[4 elements]-CRC  frameCode=0x10 (Begin, idx=0), RX 28-xx
Frame 2: TX 35-01-[4 elements]-CRC  frameCode=0x01 (idx=1),        RX 39-xx
Frame 3: TX 45-02-[4 elements]-CRC  frameCode=0x02 (idx=2),        RX 4A-xx
Frame 4: TX 55-03-[4 elements]-CRC  frameCode=0x03 (idx=3),        RX 5B-xx
Frame 5: TX 65-24-[2 elements]-CRC  frameCode=0x24 (End, idx=4),   RX 6C-xx
```

---

### ReadDump

**Purpose:** Reads a contiguous block of raw memory from the MCU.

#### Request Frame

```
[ OpCode(ReadDump) | addr[0..N-1] | requestedSize | CRC ]
  1 byte              2 or 4 bytes   1 byte (max 128)  1 byte
```

- `requestedSize` is clamped to `MaxPayloadSize` (128).

#### Response Frame

```
[ OpCode_echo | data[0..N-1] | CRC ]
  1 byte        up to 128 bytes  1 byte
```

- `data`: Raw memory bytes read sequentially from `addr`. The count `N` equals the number of
  bytes actually returned, which may be less than `requestedSize`.

> **MCU behavior:** Stops logging if active. Reads up to `requestedSize` bytes starting at
> `addr`. Returns as many bytes as available up to the requested count. Sends a normal response
> containing the raw memory bytes as payload.
>
> **Host behavior:** If the MCU returns fewer bytes than requested (partial response), the host
> re-issues ReadDump with the adjusted address (`addr += returned_count`) and reduced remaining
> byte count, repeating until all requested data has been collected.


---

## Log Frame

During logging mode, the MCU autonomously emits frames at the configured `timeStep` interval.
These are **unsolicited** — the host reads them from the receive buffer without sending a request.

### Frame Layout

```
[ SlvOpCode | data[0] | data[1] | ... | CRC ]
  1 byte      (sum of configured element sizes)  1 byte
```

### SlvOpCode Byte

| Bits | Meaning                                                             |
|------|---------------------------------------------------------------------|
| 7–4  | Always `0x0` (fixed). Marks this as a log frame.                   |
| 3–0  | **SlvCnt**: slave sequence counter, range `1`–`15`, wraps `15 → 1` |

> **MCU behavior:** Increments `SlvCnt` by 1 each time a log frame is successfully written
> into the TX buffer (only when the TX buffer is free **and** sampled data is available).
> Wraps `15 → 1`, skipping `0`. The upper nibble is always `0x0`.

### Payload Decoding

Values are packed in the order defined by the most recent SetAddr configuration.
For each configured element (in order):

```
raw_bytes = next <size> bytes from payload (little-endian)
value     = zero-extend to uint64
```


---

## Derived Frame

A special frame type for out-of-band communication (e.g., MCU-to-host debug text via the
Serial Communication Emulation channel). **Derived frames do not use CRC.**

### Frame Layout

```
[ 0x00 | modeCode | payload[0] | payload[1] | ... ]
  1 byte  1 byte    variable length
Minimum: 3 bytes
```

| `modeCode` | Name                           | Description                       |
|------------|--------------------------------|-----------------------------------|
| `0x00`     | `Undefined`                    | —                                 |
| `0x01`     | `SerialCommunicationEmulation` | Emulates a serial UART debug channel |

- Identification: `frame[0] == 0x00` exactly.
- No CRC is appended. Without CRC protection, corrupted data reaches the application layer rather than being silently discarded.
- The MCU can send Derived frames at any time, including during logging.
- The host can also **send** Derived frames to the MCU via the same channel.

> **MCU behavior:** SCE transmission is gated by `is_approved`. The MCU only sends derived frames
> after a successful ReadInfo handshake. SCE TX is low-priority: a derived frame is assembled
> and sent only when no protocol response frame is pending in the TX buffer. The MCU queues
> incoming derived frames from the host in a circular receive buffer for the application to read.
>
> **Host behavior:** The host checks `frame[0] == 0x00` first during the logging receive loop,
> before attempting log frame parsing. On receiving an SCE frame (`modeCode == 0x01`), the
> payload bytes are passed to the SCE receive callback. The host may also send derived frames
> to the MCU at any time using `MakeDerivedFrame()`.

### Example (MCU → Host, debug text)

```
00 01 50 72 65 73 73 65 64 21 0D 0A
^  ^  ^-- payload: "Pressed!\r\n" (ASCII) --^
|  modeCode = 0x01 (SerialCommunicationEmulation)
frame[0] = 0x00 → Derived frame
```

---

## Command Behavior During Logging

When the MCU is in logging mode (after a successful `StartLog`), commands from the host
are handled as follows:

| Command       | MCU response          | Effect on logging                                      |
|---------------|-----------------------|--------------------------------------------------------|
| `StartLog`    | **None (NR)**         | Keepalive — resets the MCU logging timeout counter     |
| `Write`       | **None (NR)**         | Value written silently; logging continues              |
| `StopLog`     | Normal response       | Logging stops; MCU returns to idle                     |
| `SetTimeStep` | Normal response       | Logging stops; new interval stored                     |
| `SetAddr`     | Normal response       | Logging stops; log variable table reconfigured         |
| `ReadInfo`    | Normal response       | Logging stops; session re-authenticated                |
| `ReadDump`    | Normal response       | Logging stops; memory block queued for reply           |

**Key rules:**

- Only `StartLog` acts as a keepalive (no response).
- Only `Write` is executed silently without stopping logging (no response).
- All other commands cause an immediate transition out of logging mode and produce a normal response.

> **MCU behavior:** The MCU's response behavior in this table is authoritative. The NR rule for
> `StartLog` and `Write` means the MCU does not allocate a TX slot for these during logging —
> the TX pipeline remains dedicated to log frame emission.
>
> **Host behavior:** When the host needs to detect the end of logging (e.g., after a `StopLog`
> injected mid-session), it uses the Frame Classification rule: response frames satisfy
> `(frame[0] & 0xF0) != 0x00`, distinguishing them from any log frames still in the receive
> buffer.

---

## Session State Machine

> **Host behavior:** The entire session state machine described below runs on the host side. The MCU does not have an
> equivalent task queue; it reacts to each received frame independently.

Tasks are enqueued and executed serially. The normal startup-to-logging flow is:

```
Open → Initialize → SetTimeStep → SetAddr → StartLog → [Logging] → StopLog → Close
```

| Task         | Description                                                              |
|--------------|--------------------------------------------------------------------------|
| `Open`       | Establish the physical serial link                                       |
| `Initialize` | Send `ReadInfo`; fail if EchoDetected; extract device name               |
| `SetTimeStep`| Send `SetTimeStep`; store interval for relative timestamp computation    |
| `SetAddr`    | Send one or more `SetAddr` frames; reset SlvCnt tracking                 |
| `StartLog`   | Send `StartLog`; on success, auto-enqueue the `Logging` task             |
| `Logging`    | Continuous receive loop; inject writes via `Write` frames (NR); keepalives every 500 ms |
| `StopLog`    | Send `StopLog`                                                           |
| `ReadDump`   | Send `ReadDump` with automatic address continuation until all bytes collected |
| `Close`      | Close the physical link                                                  |



### MCU State Machine

The MCU maintains its own minimal state using two flags:

| Flag           | Initial | Description                                                          |
|----------------|---------|----------------------------------------------------------------------|
| `is_approved`  | `false` | Set to `true` after a successful `ReadInfo` handshake                |
| `is_logging`   | `false` | Set to `true` after `StartLog`; cleared by `StopLog`, `SetAddr`, `SetTimeStep`, `ReadInfo`, `ReadDump` |

---

## Timing Parameters

| Parameter                | Value    | Side      | Description                                                  |
|--------------------------|----------|-----------|--------------------------------------------------------------|
| `RX_TIMEOUT_MS`          | 100 ms   | MCU       | Max silence between bytes within an in-progress SLIP frame   |
| `KEEPALIVE_MS`           | 2000 ms  | MCU       | MCU-side logging timeout without a StartLog keepalive        |
| `LOG_INTERVAL_DEFAULT`   | 500 ms   | MCU       | Default log emission interval (`TimeStepDefault`)            |
| `KEEPALIVE_INTERVAL`     | 500 ms   | Host      | Host keepalive send interval during logging                  |
| `LOGGING_TIMEOUT`        | 5000 ms  | Host      | Host-side logging timeout (no log frame received)            |
| `QUERY_TIMEOUT`          | 100 ms   | Host      | Host-side per-attempt response timeout                       |
| `QUERY_TIMEOUT_LOCALNET` | 1000 ms  | Host      | Response timeout over LocalNet transport                     |

---

*Copyright 2026 Naoya Imai. Released under open source license.*