# CLI Reference

CodyNET provides a command-line interface for running, assembling, and disassembling programs for the Cody Computer.

When launched without arguments (or with `-i` / `--interactive`), an interactive shell starts. Enter `help` to list commands, `exit` or `quit` to leave.

## Global Options

| Option | Description |
|---|---|
| `--log-level <level>` | Console log level: `Error`, `Warn`, `Info`, `Debug`, `Verbose` (default: `Info`) |
| `--verbose`, `-v` | Shorthand for `--log-level Verbose` |

## Commands

### boot

Boot the emulator with the built-in CodyBASIC.

```
boot [OPTIONS]
```

| Option | Description |
|---|---|
| `--debug`, `-d` | Enable the integrated debugger |
| `--uart1-source <path>` | File to fill UART1 receive buffer |
| `--uart2-source <path>` | File to fill UART2 receive buffer |
| `--fix-newlines` | Normalize newlines in UART text input (CRLF to LF) |
| `--physical-keyboard` | Use physical Cody keyboard mapping (ignores host layout) |
| `--clock <frequency>` | Target CPU clock rate (default: `1MHz`) |
| `--fast` | Run as fast as possible (ignores `--clock`) |
| `--headless` | Run without video output |
| `--start-paused` | Start the emulator in paused state |

**Examples:**

```
boot --debug --clock 2MHz
boot --uart1-source program.bas
```

### run

Run a binary file with the emulator.

```
run <file> [OPTIONS]
```

| Argument | Description |
|---|---|
| `file` | Binary file path (required) |

| Option | Description |
|---|---|
| `--as-cartridge` | Treat file as cartridge image (expects cartridge header) |
| `--load-address`, `-l <address>` | Load address for raw binaries (default: `0xE000`) |
| `--reset-vector <address>` | Override Reset Vector (`0xFFFC`) |
| `--irq-vector <address>` | Override IRQ Vector (`0xFFFE`) |
| `--nmi-vector <address>` | Override NMI Vector (`0xFFFA`) |
| `--debug`, `-d` | Enable the integrated debugger |
| `--uart1-source <path>` | File to fill UART1 receive buffer |
| `--uart2-source <path>` | File to fill UART2 receive buffer |
| `--fix-newlines` | Normalize newlines in UART text input |
| `--physical-keyboard` | Use physical Cody keyboard mapping |
| `--clock <frequency>` | Target CPU clock rate (default: `1MHz`) |
| `--fast` | Run as fast as possible |
| `--headless` | Run without video output |
| `--start-paused` | Start the emulator in paused state |

**Examples:**

```
run program.bin --load-address 0x8000 --debug
run cartridge.bin --as-cartridge --fast
```

### assemble

Assemble a source file into a binary.

```
assemble <file> [OPTIONS]
```

| Argument | Description |
|---|---|
| `file` | Assembly source file (required) |

| Option | Description |
|---|---|
| `--output`, `-o <path>` | Output binary path (default: `<input>.bin`) |

**Examples:**

```
assemble program.asm
assemble program.asm --output compiled.bin
```

### disassemble

Disassemble a binary into assembly.

```
disassemble <file> [OPTIONS]
```

| Argument | Description |
|---|---|
| `file` | Binary file (required) |

| Option | Description |
|---|---|
| `--as-cartridge` | Treat file as cartridge image |
| `--load-address`, `-l <address>` | Base address for raw binaries (default: `0xE000`) |
| `--output`, `-o <path>` | Output assembly path (default: `<input>.asm`) |

**Examples:**

```
disassemble binary.bin --load-address 0x8000
disassemble cartridge.bin --as-cartridge --output program.asm
```

### list

List `.asm` and `.bin` files in the current directory.

**Aliases:** `ls`, `dir`

```
list [OPTIONS]
```

| Option | Description |
|---|---|
| `--recursive`, `-r` | Include file counts from subdirectories |

## Address Formats

All address parameters accept the following formats:

| Format | Example |
|---|---|
| Decimal | `32768` |
| Hex with `0x` prefix | `0x8000` |
| Hex with `$` prefix | `$8000` |
| Bare hex | `E000` |

## Clock Formats

The `--clock` parameter accepts:

| Format | Example |
|---|---|
| Raw Hz | `1000000` |
| MHz | `1MHz`, `2MHz` |
| kHz | `500kHz` |
| Hz | `1000Hz` |

Default: `1MHz`
