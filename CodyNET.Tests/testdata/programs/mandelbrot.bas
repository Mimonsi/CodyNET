1 REM SPDX-License-Identifier: GPL-3.0-or-later

295 REM Load color palette
300 FOR I = 0 TO 15
310 READ C
320 P(I) = C
390 NEXT

905 REM Start at top left screen corner
910 PRINT AT(0,0);

955 REM Turn off INPUT prompt
960 POKE 14, 0

975 REM Set border color to medium gray
980 POKE 53250, OR(AND(PEEK(53250), 240), 12)

995 REM Outer loop: Y coordinate
1000 FOR I = -12 TO 12
1010 D = I*8

1095 REM Inner loop: X coordinate
1100 FOR J = -30 TO 9
1110 C = J*8

1115 REM Working variables (real and imaginary part)
1120 X = 0
1130 Y = 0
1139 REM Iteration count
1140 K = 1

1210 T = X*X/100 - Y*Y/100 + C
1220 Y = 2*X*Y/100 + D
1230 X = T
1240 K = K + 1

1245 REM Maximum iterations reached, i. e. did not diverge?
1250 IF K > 15 THEN GOTO 1310
1255 REM Value diverges?
1260 IF ABS(X) > 200 THEN GOTO 1310
1270 IF ABS(Y) > 200 THEN GOTO 1310
1285 REM Continue iterating
1290 GOTO 1210

1300 REM Draw "pixel"
1305 REM Draw as black if not diverged
1310 IF K > 15 THEN K = 0
1320 PRINT CHR$(224 + P(K));
1335 REM Hack to prevent empty line at end of screen
1340 IF I=12 THEN IF J=9 THEN GOTO 1500
1350 PRINT " ";

1500 NEXT

1515 REM Hack to prevent empty line at end of screen
1520 IF I=12 THEN GOTO 1600

1600 NEXT

1905 REM At end: wait for key press (so the BASIC prompt doesn't scroll away the graphics)
1910 REM PRINT AT(0,0), CHR$(230);
1920 INPUT T$

1955 REM Reset colors
1960 PRINT CHR$(230);
1970 POKE 53250, OR(AND(PEEK(53250), 240), 7)

5005 REM Color palette data
5010 DATA 0,6,11,9
5020 DATA 4,2,3,5
5030 DATA 12,14,10,8
5040 DATA 7,15,13,1

