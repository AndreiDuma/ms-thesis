( works )
( works )
( also ) ( works )
\ line comment works.

( empty line ^ works )

: ADIOS ( -- )  BYE ;  \ works
\ ADIOS          \ works

: PAPA ( -- )  ADIOS ;  \ works
\ PAPA            \ works

\ : ( -- )  DIE-IN-THE-MIDDLE [ BYE ] ;  \ works

\ 1 2 3 4 dbg BYE  \ pushing to stack works

\ : 1+ ( n|u -- n'|u' )  [        
\      83 . B2 . 09 . 00 .	\ [STACK@s3]++      ld t0, 0(s3)
\      93 . 82 . 12 . 00 .	\                   addi t0, t0, 1
\      23 . B0 . 59 . 00 . ] ;	\                   sd t0, 0(s3)
: 1+ ( n|u -- n'|u' )  [        
     00 . 09 . B2 . 83 .	\ [STACK@s3]++      ld t0, 0(s3)
     00 . 12 . 82 . 93 .	\                   addi t0, t0, 1
     00 . 59 . B0 . 23 . ] ;	\                   sd t0, 0(s3)
\ : 1-        ( n|u -- n'|u' )    [ 49 . FF . 0F . ] ;                \ [r15]--           dec r/m64       REX.W FF /1     00 001 111  OneMinus
\ : 2*        (   x -- x'    )    [ 49 . D1 . 27 . ] ;                \ [r15] <<= 1       sal r/m64, 1    REX.W D1 /4     00 100 111  TwoTimes
\ : 4*        (   x -- x'    )    [ 49 . C1 . 27 . 02 . ] ;           \ [r15] <<= 2       sal r/m64, imm8 REX.W C1 /4 ib  00 100 111
\ : 8*        (   x -- x'    )    [ 49 . C1 . 27 . 03 . ] ;           \ [r15] <<= 3       sal r/m64, imm8 REX.W C1 /4 ib  00 100 111
\ : 2/        (   x -- x'    )    [ 49 . D1 . 3F . ] ;                \ [r15] >>= 1       sar r/m64, 1    REX.W D1 /7     00 111 111  TwoDiv

1 2
dbg BYE
