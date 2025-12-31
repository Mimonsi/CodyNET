# CodyNET
CodyNET is a .NET based emulator for the [Cody Computer](https://codycomputer.org), a retro-style computer based on the WDC65C02 microprocessor.

This implementation is using [iTitus' Cody Emulator](https://github.com/iTitus/cody_emulator) as a reference.
It aims to enhance the original emulator with extended features.

# Original features
- Emulation of Cody Basic and Assembly
- ...

# New features
- Debugging tools for assembly programming
- Emulator can be set to run in real time

# single_step_tests

Uses the [65x02 SingleStepTests](https://github.com/SingleStepTests/65x02) created by Thomas Harte et al., licensed under MIT.

Download the test definitions for the WDC65C02 from [here](https://github.com/SingleStepTests/65x02/archive/refs/heads/main.zip) and unpack them in this directory to run the tests. The project copies everything under `wdc65c02/` to the test output directory so tests can run from the compiled bin folder.

This folder should contain the path `wdc65c02/v1/*.json`
