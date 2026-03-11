## Subcommands
- list (ls, dir)
- run
- boot
- assemble
- disassemble
- logtest

## Global Options
- --verbose (-v)

## list-options
- --recursive (-r)

## run-options
- arg: file
- --as-cartridge
- --load-address <address> (decimal, 0x-prefixed hex, or bare hex like E000)
- --reset-vector <address>
- --irq-vector <address>
- --nmi-vector <address>
- --uart1-source <path>
- --uart2-source <path>
- --fix-newlines
- --physical-keyboard
- --debug (-d)
- --clock <frequency>
- --fast
- --headless

## boot-options
- --uart1-source <path>
- --uart2-source <path>
- --fix-newlines
- --physical-keyboard
- --debug (-d)
- --clock <frequency>
- --fast
- --headless

## assemble-options
- arg: file
- --output <path>
- --format <raw|cartridge>
- --load-address <address> (used for cartridge output)
- --warn-as-error (currently unsupported and fails explicitly)

## disassemble-options
- arg: file
- --as-cartridge
- --load-address <address> (used for raw binaries)
- --output <path>
