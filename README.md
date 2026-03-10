# CodyNET
CodyNET is a .NET based emulator for the [Cody Computer](https://codycomputer.org), a retro-style computer based on the WDC65C02 microprocessor.

This implementation is using [iTitus' Cody Emulator](https://github.com/iTitus/cody_emulator) as a reference.
It aims to enhance the original emulator with extended features.

## Usage
The emulator can be be double clicked (on windows) or started from the command line. Double clicking or starting will open the interactive console.

Use the command `boot` to start the emulator with default parameters into the Cody BASIC interpreter. Use `help` to see a list of available commands.

## Loading BASIC program
BASIC programs are loaded using UART. The source file can either be set as run command using `--uart1-source <path>` or set over UI during runtime by clicking on "File" -> "Load UART1 Source" and selecting the file. After attaching the source, type `LOAD 1,0` in the BASIC interpreter to load the program. Then type `RUN` to execute the program.

# Random Development Notes

## Original features
- Emulation of Cody Basic and Assembly
- ...

## New features
- Debugging tools for assembly programming
- Emulator can be set to run in real time

## single_step_tests

Uses the [65x02 SingleStepTests](https://github.com/SingleStepTests/65x02) created by Thomas Harte et al., licensed under MIT.

Download the test definitions for the WDC65C02 from [here](https://github.com/SingleStepTests/65x02/archive/refs/heads/main.zip) and unpack them in CodyNET.Tests/testdata to run the tests. The project copies everything under `wdc65c02/` to the test output directory so tests can run from the compiled bin folder.

Prefer the helper scripts for minimal effort:

```bash
./CodyNET.Tests/testdata/fetch-singlestep-tests.sh
```

```powershell
./CodyNET.Tests/testdata/fetch-singlestep-tests.ps1
```

This folder `testdata` should contain the path `wdc65c02/v1/*.json`

## Notes

### Ideas
- Just-In-Time Assembly to display instruction context in debugger
- Buttons and register values at top of debugger

## Debugger Implementation

There are multiple ways to implement the Debugger. Ideally, the debug instructions should be easy to use and remember, while not having side effects on the program.

### Option A: Artificial mnemonics
The 65C02 has several unused opcodes that could be used for artificial mnemonics. This might cause side effects on the actual Cody Computer Hardware. Existing assemblers might not support these mnemonics, so a preprocessor would be required, adding an additional step to assembling a program.

- DRS #index → Dump Registers in Logger
- DBP → Create breakpoint, gives control to debugger when reached
- DMP → Dump Memory in Logger

#### Pro Option A
- Mnemonics easy to remember and intuitive to use
- Very easy to implement in the emulator

#### Contra Option A
- Possible side effects on real hardware
- Requires a preprocessor for existing assemblers, or a custom assembler

### Option B: Writing to unmapped memory addresses (FF00 -> ROM)
The Cody Computer does not map the full 16-bit address space. The range $FF00 to $FFFF is reserved for ROM. Writing to these addresses could be used to trigger debugger actions. This approach has no side effects on the actual hardware, as writes to ROM are ignored.

#### Pro Option B
- No side effects on real hardware
- No preprocessor or custom assembler required
- Easy implementation of Debugger as Memory Mapped Device in the Emulator

#### Contra Option B
- Multiple instructions required for command
- Commands less intuitive to use

### Possible Solution
Macros could be used to simplify the usage of Option B in assembly code. For example:

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

or even shorter:

```assembly
    DBP .macro param
        LDA #\param
        STA $FF00
.endmacro

start:
        DBP $01
        BRK
```

## Debugger Context Visuals

```
[Step] [Continue] [Exit]
A = 0x50, X = 0x10, PC = 0x2000
-------------------------------
STA $2002 <- Current instruction
LDA $2000 <- Next instruction
JSR $FFD2
[...]
```

## Running the emulator
After starting basic, type LOAD 1,0 to load the UART1-source