    DBP .macro param
        LDA #\param
        STA $FF00
.endmacro

start:
        DBP $01
        BRK
