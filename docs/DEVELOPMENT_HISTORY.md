# CodyNET - Development History

Overview of the development phases of the CodyNET project (a 6502-based emulator in C#/.NET).

Total timeframe: **January 26, 2026 - April 6, 2026** (~10 weeks, 163 commits)

---

## Phase 1: Project Setup & CPU Foundations (Jan 26 - Feb 3)

The basic emulator structure was set up: project scaffolding, initial test infrastructure (first xUnit, later switched to NUnit), and the first CPU opcodes (ADC). A disassembler was added early on. Addressing modes were implemented step by step, a test generator was created, and basic helper tools (Cody Calc Helper) were built.

**Milestones:**
- Initial commit & basic structure
- First SingleStep tests
- Disassembler
- All addressing modes implemented
- Test framework switch from xUnit to NUnit
- Download scripts for test data

## Phase 2: Complete Opcode Implementation (Feb 4 - Feb 6)

All 6502 opcodes were implemented and tested in a short timeframe. Branching, BIT, CLV, BRK, CMP, JSR and other instructions were added. The assembler (64tass) was integrated as a bundled resource.

**Milestones:**
- Branching instructions
- All tests generated
- BRK and interrupts
- Minimal debugger class
- Memory-mapped devices (Read/Write flag)
- "All Opcodes working" on Feb 6

## Phase 3: Speed Limiter & Performance (Feb 6 - Feb 8)

The CPU clock was realistically throttled. From the first "crude speed limiter" (up to 0.8 MHz) through several iterations to >90% accuracy in the 1 kHz - 3 MHz range. Performance tests and benchmarks were introduced.

**Milestones:**
- First speed limiter (0.8 MHz)
- Improved speed limiter (>95% accuracy)
- Max performance test
- PR #2: Bundled Assembler merged

## Phase 4: CLI & Early Frontend Work (Feb 8 - Feb 14)

The command-line interface was built with autocompletion and aliases. A dummy frontend was imported and initial video display constants were defined.

**Milestones:**
- First experimental CLI
- Autocompletion
- List command
- Dummy frontend imported
- VID constants and colors

## Phase 5: System Architecture, Logging & Screen (Feb 14 - Feb 27)

Major architecture rework: device system overhauled, logger introduced, screen and debugger started as WIP. The host project (Avalonia-based GUI) was created. The ScreenHostBridge connected the emulator core with the frontend. A massive performance improvement from 2 MHz to 18 MHz was achieved.

**Milestones:**
- Logger system
- Working border color (first visible screen output)
- Host project (Avalonia)
- ScreenHostBridge
- Profiler
- Performance leap: 2 MHz → 18 MHz
- PR #3: Host merged on Feb 27

## Phase 6: Release Pipeline & CI/CD (Feb 27 - Feb 28)

GitHub Actions for automatic releases on tag push were set up. Versioning, debug symbols, and patch notes were configured. Embedded resources (codybasic) were added and CPU cycle accuracy was improved.

**Milestones:**
- GitHub Action for release
- Automatic version sync
- Embedded resources (codybasic)
- CPU cycle accuracy improved

## Phase 7: Video Display & Keyboard (Feb 28 - Mar 6)

The video display (VID) was brought to life ("WORKING SCREEN" on March 3). Keyboard implementation followed: US/DE mapping, VIA chip, physical and logical keyboard. The first test release v0.1.0 was published.

**Milestones:**
- Working screen (March 3)
- US/DE keyboard mapping
- VIA chip
- Interrupt foundations
- Physical + logical keyboard
- Release v0.1.0 (March 6)

## Phase 8: UART Interface (Mar 9 - Mar 11)

The UART1 interface was implemented to load source code into the emulator at runtime. A Mandelbrot program was added as a demonstration. The CLI was refactored.

**Milestones:**
- UART1 implementation
- Live source loading
- Mandelbrot demo
- CLI refactoring

## Phase 9: Debug UI & Frontend Overhaul (Mar 16 - Mar 28)

The most extensive phase: a complete debug UI was built with register inspection, breakpoint management, code editor (initially custom implementation, then switched to AvaloniaEdit), toolbar, frequency selection, and source code stepping. The switch to AvaloniaEdit was accompanied by the comment "Life could have been so much easier."

**Milestones:**
- Register UI with live inspection
- Breakpoint panel (backend + UI)
- Thread-safe debugger
- Pause/Resume/Step mechanics
- Code editor with breakpoint markers
- Switch to AvaloniaEdit (March 28)
- First experimental source code stepping
- Assembler refactoring
- Toolbar with emulator controls

## Phase 10: Mac Support & Cross-Platform (Mar 20 - Apr 1)

In parallel with the debug UI, Mac support was added. The architecture was unified across all platforms, 64tass for Mac was included, and a Mac release target was added to the CI/CD pipeline.

**Milestones:**
- Platform unification
- Mac release target
- Embedded 64tass for Mac

## Phase 11: Finalization & Optimization (Apr 4 - Apr 6)

Source code stepping was completed (PR #5). Rendering options (AutoScale), debug UI toggle, and breakpoint improvements were added. Performance optimizations for memory taps, interrupts, and opcode lookup. Documentation was updated.

**Milestones:**
- PR #5: Source code stepping merged
- AutoScale for screen
- Rendering options
- Profiler improvements
- Performance optimizations (memory, interrupts, FromOpcode)
- Log level command
- Documentation updated

---

## Summary

| Phase | Timeframe | Focus |
|-------|-----------|-------|
| 1 | Jan 26 - Feb 3 | CPU foundations, tests, addressing modes |
| 2 | Feb 4 - Feb 6 | All opcodes, assembler integration |
| 3 | Feb 6 - Feb 8 | Speed limiter, performance tests |
| 4 | Feb 8 - Feb 14 | CLI, dummy frontend |
| 5 | Feb 14 - Feb 27 | Host/screen architecture, 18 MHz performance |
| 6 | Feb 27 - Feb 28 | CI/CD, release pipeline |
| 7 | Feb 28 - Mar 6 | Video display, keyboard, v0.1.0 |
| 8 | Mar 9 - Mar 11 | UART, live loading |
| 9 | Mar 16 - Mar 28 | Debug UI, AvaloniaEdit, source stepping |
| 10 | Mar 20 - Apr 1 | Mac support, cross-platform |
| 11 | Apr 4 - Apr 6 | Optimization, finalization, docs |
