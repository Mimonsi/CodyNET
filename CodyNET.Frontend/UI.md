# Cody Emulator – Debugger Design Notes

## Goals

The debugger should make it easy to:

- Understand what the emulator is currently executing
- Inspect CPU and memory state
- Control execution (run, pause, step)
- Offer breakpoints to inspect program behavior at specific points
- Track down bugs in programs running on the Cody computer

---

# High-Level Design

The debugger UI should be split into two conceptual modes.

## 1. Normal Emulation Mode

Purpose: Run Cody programs normally.

Main elements:

- Cody screen output
- Basic controls:
    - Run
    - Pause
    - Reset
    - Speed (50%, 100%, 200%, Max)
    - Load Uart Source

The debugger panels are hidden or minimized.

---

## 2. Debug Mode (-debug flag)

Purpose: Inspect and control emulator execution.

The UI switches to a **workspace with dockable panels** similar to an IDE.

Possible panels:
- Code panel showing currently executed code in assembly (disassembled if binary is loaded)

- CPU Registers
- Memory Viewer
- Breakpoints
- Debug Console

---

# Core Debugger Features

## Execution Control
Either as overlay in the screen window, as menu options or an entirely new panel.

Basic execution control features:

- Run
- Pause
- Single Step (execute one instruction)
- Current FPS
- Current Emulation Speed and Target

| \> Run/Pause | \>\| Step | Reset | - | - | - | 60 FPS | 1.33/2MHz |
|--------------|-----------|-------|---|---|---|--------|-----------|

---

# CPU Inspection

The CPU panel should display the current state of the processor.
Registers and Flags will be displayed individually.

### Registers

| REG | Hex   | Dec  |
|-----|-------|------|
| A   | 0x00  | 0    |
| X   | 0x00  | 0    |
| Y   | 0x00  | 0    |
| SP  | 0xFF  | 255  |
| PC  | 0x539 | 1337 |

## Processor Flags

Display flags individually:

| FLAG       | Value |
|------------|-------|
| 00100111   | 0x27  |
| Carry      | 1     |
| Zero       | 0     |
| IrqDisable | 0     |
| Decimal    | 1     |
| Break      | 1     |
| Overflow   | 0     |
| Negative   | 0     |
---
