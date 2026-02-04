; -----------------------------
; Debug memory-mapped interface
; Usage: Include file in your code by using
; .include "debug_macros.inc"
; then use the macros DBP, DRS, and DMP to interact with the debugger.
; Example:
;
; .include "debug_macros.inc"
; DBP 0x01 ; Set a breakpoint with parameter 0x01
; DRS 0x02 ; Dump registers with parameter 0x02
; DMP 0x03 ; Dump memory with parameter 0x03
; -----------------------------

DBG_DBP = $FF00
DBG_DRS = $FF01
DBG_DMP = $FF02

; Sets a breakpoint, giving control to the debugger. The parameter is a single byte that can be used to identify the breakpoint.
DBP .macro param
        LDA #\param
        STA DBG_DBP
.endmacro

; Dumps the current register values to the debugger. The parameter is a single byte that can be used to identify the dump.
DRS .macro
        LDA #$01
        STA DBG_DRS
.endmacro

; Dumps the contents of the Memory Dump area to the debugger. The parameter is a single byte that can be used to identify the dump.
DMP .macro param
        LDA #\param
        STA DBG_DMP
.endmacro
