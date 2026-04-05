# Debugger Notes

This file contains design notes and implementation ideas for debugger-related features in CodyNET.

## Current goals

- Debugging tools for assembly programming
- Real-time emulator execution
- Easy-to-use debug commands with minimal side effects

## Open ideas

- Just-in-time assembly to display instruction context in the debugger
- Buttons and register values at the top of the debugger UI

## Implementation options

### Option A: artificial mnemonics

The 65C02 has unused opcodes that could be repurposed for debugger mnemonics.

Examples:

- `DRS #index` -> dump registers in logger
- `DBP` -> create breakpoint and enter debugger
- `DMP` -> dump memory in logger

Pros:

- Easy to remember
- Straightforward emulator implementation

Cons:

- May differ from real hardware behavior
- Existing assemblers may not support the mnemonics
- Likely requires a preprocessor or custom assembler support

### Option B: writes to unmapped memory / ROM range

The Cody Computer does not map the full 16-bit address space. One idea is to use writes to `$FF00` to `$FFFF` to trigger debugger actions. On real hardware, writes to ROM are ignored, so this approach should avoid side effects there.

Pros:

- No expected side effects on real hardware
- No custom assembler syntax required
- Maps naturally to a memory-mapped device in the emulator

Cons:

- Multiple instructions per debugger command
- Less intuitive than dedicated mnemonics

## Possible macro-based approach

```assembly
DBG_DBP  = $FF00
DBG_DRS  = $FF01
DBG_DMP  = $FF02

DBP .macro param
        LDA #\param
        STA DBG_DBP
.endmacro

DRS .macro
        LDA #$01
        STA DBG_DRS
.endmacro

DMP .macro param
        LDA #\param
        STA DBG_DMP
.endmacro

start:
        DBP $01
        DRS
        DMP $02
        BRK
```

Shorter form:

```assembly
DBP .macro param
        LDA #\param
        STA $FF00
.endmacro

start:
        DBP $01
        BRK
```

## Debugger UI sketch

```text
[Step] [Continue] [Exit]
A = 0x50, X = 0x10, PC = 0x2000
-------------------------------
STA $2002 <- Current instruction
LDA $2000 <- Next instruction
JSR $FFD2
[...]
```
