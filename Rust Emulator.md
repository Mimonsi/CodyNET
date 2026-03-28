# Rust Emulator Findings

## BASIC File

`cargo run --release -- --uart1-source file.bas codybasic.bin`
LOAD 1,0

## Binary File
Codybros works for this.
`cargo run --release -- --uart1-source file.bin codybasic.bin`
LOAD 1,1

OR 
`cargo run --release -- --as-cartridge file.bin`