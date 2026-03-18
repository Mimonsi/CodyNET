; Konstantin Schwarze 2025
; Tic Tac Toe for Cody Computer

; Controls: Use keys Q-O to select positions 1-9
; |1|2|3|    | | | |
; |4|5|6| -> | |X| | -> ...
; |7|8|9|    | | | |
; Player x goes first, o goes second
; 64tass --mw65c02 --nostart -o TicTacToe.bin TicTacToe.asm

ADDR    = $3000                 ; The actual loading address of the program
SCRRAM  = $C400                 ; The default location of screen memory

VIA_BASE  = $9F00               ; VIA base address and register locations
VIA_IORB  = VIA_BASE+$0
VIA_IORA  = VIA_BASE+$1
VIA_DDRB  = VIA_BASE+$2
VIA_DDRA  = VIA_BASE+$3
VIA_T1CL  = VIA_BASE+$4
VIA_T1CH  = VIA_BASE+$5
VIA_SR    = VIA_BASE+$A
VIA_ACR   = VIA_BASE+$B
VIA_PCR   = VIA_BASE+$C
VIA_IFR   = VIA_BASE+$D

STRPTR    = $D0                 ; Pointer to string (2 bytes)
SCRPTR    = $D2                 ; Pointer to screen (2 bytes)

KEYROW0   = $DA                 ; Keyboard row 0
KEYROW1   = $DB                 ; Keyboard row 1
KEYROW2   = $DC                 ; Keyboard row 2
KEYROW3   = $DD                 ; Keyboard row 3
KEYROW4   = $DE                 ; Keyboard row 4
KEYROW5   = $DF                 ; Keyboard row 5

BOARD_X   = 26
BOARD_Y   = 1
PLAYER_TURN = $E0 ; 0 = Player 1 (X), 1 = Player 2 (O)

; Program header for Cody Basic's loader (needs to be first)

.WORD ADDR                      ; Starting address (just like KIM-1, Commodore, etc.)
.WORD (ADDR + LAST - MAIN - 1)  ; Ending address (so we know when we're done loading)

;
; The actual program.
;

.LOGICAL    ADDR                ; The actual program gets loaded at ADDR

            
;_LOOP       LDA TEXT,X          ; Copies TEXT into screen memory (kept for reference)
;            BEQ _DONE
;            
;            STA SCRRAM,X
;            
;            INX
;            BRA _LOOP

MAIN        
            
            JSR SHOWSCRN
            
_LOOP       JSR KEYSCAN         ; Scan the keyboard

            LDA KEYROW5         ; Pressed P for play?
            AND #%10000
            BNE _PLAY

            LDA KEYROW0         ; Pressed Q for quit?
            AND #%00001
            BNE _QUIT
            
            BRA _LOOP           ; Repeat main loop
            
_QUIT       RTS                 ; Return to BASIC
            
_PLAY       JSR GAMESTART       ; Start the game
            BRA _LOOP
            
_PROG       
            BRA _LOOP

WAIT_HALF_SECOND:
    LDY #200        ; äußere Schleife
W0: LDX #255
W1: DEX
    BNE W1          ; innere Schleife läuft 256
    DEY
    BNE W0          ; äußere Schleife: 200×
    RTS

GAMESTART  ; Starts a new game of Tic Tac Toe
            JSR CLRSCRN ; Clear screen

            LDA #0
            STA PLAYER_TURN     ; Player 1 (X) starts first

            LDX #0
            LDY #0
            JSR MOVESCRN
            
            LDX #MSG_TICTACTOE ; Print title
            JSR PUTMSG

            ; TODO: Game setup
            JSR _PRINT_BOARD

            ; Game Loop -> _LOOP
            JSR _LOOP
            RTS

_LOOP
            JSR WAIT_HALF_SECOND ; Debounce delay
            JSR WAIT_HALF_SECOND ; Debounce delay
            JSR WAIT_HALF_SECOND ; Debounce delay
            JSR WAIT_HALF_SECOND ; Debounce delay
            JSR WAIT_HALF_SECOND ; Debounce delay
            JSR WAIT_HALF_SECOND ; Debounce delay

            LDX #0 ; Player 1: Make your move
            LDY #1
            JSR MOVESCRN


            LDA PLAYER_TURN
            BEQ _P1 ; If Player 1
            LDX #MSG_P2MOVE ; Else code
            BRA _DONE
_P1         LDX #MSG_P1MOVE  ; Player 1 code
_DONE       JSR PUTMSG

            JSR KEYSCAN         ; Scan the keyboard for number
            LDA KEYROW0         ; Pressed Q=1?
            AND #%00001
            BNE _1

            LDA KEYROW5         ; Pressed W=2?
            AND #%00001
            BNE _2

            LDA KEYROW0         ; Pressed E=3?
            AND #%00010
            BNE _3

            LDA KEYROW5         ; Pressed R=4?
            AND #%00010
            BNE _4

            LDA KEYROW0         ; Pressed T=5?
            AND #%00100
            BNE _5

            LDA KEYROW5         ; Pressed Y=6?
            AND #%00100
            BNE _6

            LDA KEYROW0         ; Pressed U=7?
            AND #%01000
            BNE _7

            LDA KEYROW5         ; Pressed I=8?
            AND #%01000
            BNE _8

            LDA KEYROW0         ; Pressed O=9?
            AND #%10000
            BNE _9

            
            BRA _LOOP           ; Repeat main loop


            ; TODO: Exit condition for game over
            BRA _LOOP

_1          LDX #BOARD_X + 1
            LDY #BOARD_Y + 0
            JSR MOVESCRN
            JSR PUTPLAYERCHAR
            JMP _LOOP

_2          LDX #BOARD_X + 5
            LDY #BOARD_Y + 0
            JSR MOVESCRN
            JSR PUTPLAYERCHAR
            JMP _LOOP

_3          LDX #BOARD_X + 9
            LDY #BOARD_Y + 0
            JSR MOVESCRN
            JSR PUTPLAYERCHAR
            JMP _LOOP

_4          LDX #BOARD_X + 1
            LDY #BOARD_Y + 2
            JSR MOVESCRN
            JSR PUTPLAYERCHAR
            JMP _LOOP

_5          LDX #BOARD_X + 5
            LDY #BOARD_Y + 2
            JSR MOVESCRN
            JSR PUTPLAYERCHAR
            JMP _LOOP

_6          LDX #BOARD_X + 9
            LDY #BOARD_Y + 2
            JSR MOVESCRN
            JSR PUTPLAYERCHAR
            JMP _LOOP

_7          LDX #BOARD_X + 1
            LDY #BOARD_Y + 4
            JSR MOVESCRN
            JSR PUTPLAYERCHAR
            JMP _LOOP

_8          LDX #BOARD_X + 5
            LDY #BOARD_Y + 4
            JSR MOVESCRN
            JSR PUTPLAYERCHAR
            JMP _LOOP

_9          LDX #BOARD_X + 9
            LDY #BOARD_Y + 4
            JSR MOVESCRN
            JSR PUTPLAYERCHAR
            JMP _LOOP




_PRINT_BOARD
            ; Print Board Lines 1-3
            LDX #BOARD_X
            LDY #BOARD_Y
            JSR MOVESCRN
            LDX #MSG_BOARD_LINE ; Print board line 1
            JSR PUTMSG

            LDX #BOARD_X
            LDY #BOARD_Y + 1
            JSR MOVESCRN
            LDX #MSG_BOARD_SEPARATOR ; Print board separator
            JSR PUTMSG
            
            LDX #BOARD_X
            LDY #BOARD_Y + 2
            JSR MOVESCRN
            LDX #MSG_BOARD_LINE ; Print board line 2
            JSR PUTMSG

            LDX #BOARD_X
            LDY #BOARD_Y + 3
            JSR MOVESCRN
            LDX #MSG_BOARD_SEPARATOR ; Print board separator
            JSR PUTMSG
            
            LDX #BOARD_X
            LDY #BOARD_Y + 4
            JSR MOVESCRN
            LDX #MSG_BOARD_LINE ; Print board line 3
            JSR PUTMSG
            RTS

;
; SHOWSCRN
;
; Shows the main screen.
;
SHOWSCRN  JSR CLRSCRN
            
          LDX #0
          LDY #0
          JSR MOVESCRN
          
          LDX #MSG_TICTACTOE
          JSR PUTMSG
          
          LDX #0
          LDY #1
          JSR MOVESCRN
          
          LDX #MSG_SUBTITLE
          JSR PUTMSG

          LDX #0
          LDY #3
          JSR MOVESCRN
          
          LDX #MSG_MENUINSTRUCTIONS
          JSR PUTMSG

          LDX #0
          LDY #4
          JSR MOVESCRN
          
          LDX #MSG_MENUPLAY
          JSR PUTMSG
          
          LDX #0
          LDY #5
          JSR MOVESCRN
          
          LDX #MSG_MENUQUIT
          JSR PUTMSG
          
          RTS

;
; KEYSCAN
;
; Scans the keyboard matrix (so that key selections for menu options can be detected).
;
KEYSCAN     PHA                   ; Preserve registers
            PHX
            
            STZ VIA_IORA          ; Start at the first row and first key of the keyboard
            LDX #0
            
_LOOP       LDA VIA_IORA          ; Read the keys for the current row from the VIA port
            EOR #$FF
            LSR A
            LSR A
            LSR A
            STA KEYROW0,X
            
            INC VIA_IORA          ; Move on to the next keyboard row
            INX
            
            CPX #6                ; Do we have any rows remaining to scan?
            BNE _LOOP
            
            PLX                   ; Restore registers
            PLA
            
            RTS


;
; MOVESCRN
;
; Moves the SCRPTR to the position for the column/row in the X and Y
; registers. All registers are clobbered by the routine.
;
MOVESCRN  LDA #<SCRRAM            ; Move screen pointer to beginning
          STA SCRPTR+0
          LDA #>SCRRAM
          STA SCRPTR+1
          
          INY                     ; Increment pointer for each row
_LOOPY    CLC 
          LDA SCRPTR+0
          ADC #40
          STA SCRPTR+0
          LDA SCRPTR+1
          ADC #0
          STA SCRPTR+1
          DEY
          BNE _LOOPY
          
          CLC                     ; Add position on column
          TXA
          ADC SCRPTR+0
          STA SCRPTR+0
          LDA SCRPTR+1
          ADC #0
          STA SCRPTR+1
          
          RTS

;
; CLRSCRN
;
; Clear the entire screen by filling it with whitespace (ASCII 20 decimal).
;
CLRSCRN   LDA #<SCRRAM            ; Move screen pointer to beginning
          STA SCRPTR+0
          LDA #>SCRRAM
          STA SCRPTR+1
          
          LDA #20                 ; Clear screen by filling with whitespaces
          
          LDY #25                 ; Loop 25 times on Y
          
_LOOPY    LDX #40                 ; Loop 40 times on X for each Y
          
_LOOPX    STA (SCRPTR)            ; Store zero

          INC SCRPTR+0            ; Increment screen position
          BNE _NEXT
          INC SCRPTR+1
          
_NEXT     DEX                     ; Next X
          BNE _LOOPX
          
          DEY                     ; Next Y
          BNE _LOOPY
          
          RTS


PUTPLAYERCHAR ; Puts the character for the current player (X or O) on the screen.
            LDA PLAYER_TURN
            BEQ _P1
            LDX #MSG_P2CHAR
            BRA _DONE
_P1         LDX #MSG_P1CHAR
_DONE       JSR PUTMSG

            LDA PLAYER_TURN ; Switch player turn
            EOR #1
            STA PLAYER_TURN

            RTS

;
; PUTMSG
;
; Puts a message string (one of the MSG_XXX constants) on the screen.
;
PUTMSG      PHA
            PHY
            
            LDA MSGS_L,X        ; Load the pointer for the string to print
            STA STRPTR+0
            LDA MSGS_H,X
            STA STRPTR+1
            
            LDY #0
            
_LOOP       LDA (STRPTR),Y      ; Read the next character (check for null)
            BEQ _DONE
            
            JSR PUTCHR          ; Copy the character and move to next
            INY         
            
            BRA _LOOP           ; Next loop
            
_DONE       PLY
            PLA
            
            RTS

;
; PUTCHR
;
; Puts an individual ASCII character on the screen.
;
PUTCHR      STA (SCRPTR)        ; Copy the character   
            
            INC SCRPTR+0        ; Increment screen position
            BNE _DONE
            INC SCRPTR+1
          
_DONE       RTS

;
; IDs for the message strings that can be displayed in the program.
;
MSG_TICTACTOE  = 0
MSG_SUBTITLE  = 1
MSG_MENUINSTRUCTIONS = 2
MSG_MENUPLAY  = 3
MSG_MENUQUIT  = 4
MSG_P1MOVE  = 5
MSG_P2MOVE  = 6
MSG_P1WIN  = 7
MSG_P2WIN  = 8
MSG_BOARD_LINE = 9
MSG_BOARD_SEPARATOR = 10
MSG_P1CHAR = 11
MSG_P2CHAR = 12

;
; The strings displayed by the program.
;
STR_TICTACTOE  .NULL "Tic Tac Toe"
STR_SUBTITLE  .NULL "Welcome to Tic Tac Toe!"
STR_MENUINSTRUCTIONS .NULL "Press 1-9 keys to select position."
STR_MENUPLAY  .NULL "(P)lay"
STR_MENUQUIT  .NULL "(Q)uit"
STR_P1MOVE  .NULL "Player 1: Make your Move"
STR_P2MOVE  .NULL "Player 2: Make your Move"
STR_P1WIN  .NULL "Player 1 wins!"
STR_P2WIN  .NULL "Player 2 wins!"
STR_BOARD_LINE       .NULL "   |   |   "
STR_BOARD_SEPARATOR  .NULL "---+---+---"
STR_P1CHAR  .NULL "X"
STR_P2CHAR  .NULL "O"

;
; Low bytes of the string table addresses.
;
MSGS_L
  .BYTE <STR_TICTACTOE
  .BYTE <STR_SUBTITLE
  .BYTE <STR_MENUINSTRUCTIONS
  .BYTE <STR_MENUPLAY
  .BYTE <STR_MENUQUIT
  .BYTE <STR_P1MOVE
  .BYTE <STR_P2MOVE
  .BYTE <STR_P1WIN
  .BYTE <STR_P2WIN
  .BYTE <STR_BOARD_LINE
  .BYTE <STR_BOARD_SEPARATOR
  .BYTE <STR_P1CHAR
  .BYTE <STR_P2CHAR

;
; High bytes of the string table addresses.
;
MSGS_H
  .BYTE >STR_TICTACTOE
  .BYTE >STR_SUBTITLE
  .BYTE >STR_MENUINSTRUCTIONS
  .BYTE >STR_MENUPLAY
  .BYTE >STR_MENUQUIT
  .BYTE >STR_P1MOVE
  .BYTE >STR_P2MOVE
  .BYTE >STR_P1WIN
  .BYTE >STR_P2WIN
  .BYTE >STR_BOARD_LINE
  .BYTE >STR_BOARD_SEPARATOR
  .BYTE >STR_P1CHAR
  .BYTE >STR_P2CHAR

LAST                            ; End of the entire program

.ENDLOGICAL
