# CodyNET
CodyNET is a .NET based emulator for the [Cody Computer](https://codycomputer.org), a retro-style computer based on the WDC65C02 microprocessor.

This implementation is using [iTitus' Cody Emulator](https://github.com/iTitus/cody_emulator) as a reference.
It aims to enhance the original emulator with extended features.

## Original features
- Emulation of Cody Basic and Assembly
- ...

## New features
- Debugging tools for assembly programming
- Emulator can be set to run in real time

## Artificial mnemonics
- DRS #index → Dump Registers in Logger
- DBP → Create breakpoint, gives control to debugger when reached
- DMP → Dump Memory in Logger

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

```
[Step] [Continue] [Exit]
A = 0x50, X = 0x10, PC = 0x2000
-------------------------------
STA $2002 <- Current instruction
LDA $2000 <- Next instruction
JSR $FFD2
[...]
```