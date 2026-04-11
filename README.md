# CodyNET

CodyNET is a .NET-based emulator for the [Cody Computer](https://codycomputer.org), a retro-style computer built around the WDC65C02 microprocessor.

The project is based on ideas and behavior from [iTitus' Cody Emulator](https://github.com/iTitus/cody_emulator) and extends it with additional tooling such as debugging support and a .NET frontend.

## Quick start

Prerequisite: .NET SDK installed.

1. Download the [latest release](https://github.com/Mimonsi/CodyNET/releases/latest) for your platform and extract it.
2. Execute the file by double-clicking or via command line. When no parameters are provided, the program will launch an interactive shell. Enter `help` to list available commands
3. Enter `boot` to start the emulator with default parameters. Add the `--debug` parameter to start the integrated editor and other debugging tools.


## Usage

Inside the shell, use `help` to list commands.

## Loading programs

### BASIC over UART

Attach a BASIC (.bas) source file either:

- on startup with `--uart1-source <path>`
- in the frontend via `File -> Load UART1 Source`

Then load and run it in Cody BASIC:

```text
LOAD 1,0
RUN
```

### Binary over UART

Binary files (.bin) can also be attached via UART. In Cody BASIC, load them with:

```text
LOAD 1,1~~~~
```

Additional documentation:

- [CLI reference](./docs/CLI.md)
