DBP .macro param
        LDA #\param
        STA $FF00
.endmacro

DRS .macro
        LDA #$01
        STA $FF01
.endmacro

DMP .macro param
        LDA #\param
        STA $FF02
.endmacro

start:
        DBP $01
        DRS
        DMP $02
        BRK
