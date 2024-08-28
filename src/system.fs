: ^ ( n -- )
  [ 93 . 89 . 89 . 00 . ] ;				    \ s3 += 8

: .>x ( n -- n )
  [ 03 . BA . 09 . 00 . ] ;				    \ x@s4 = [s3]                           ld s4, 0(s3)
: x>. ( ? -- n )
  [ 23 . B0 . 49 . 01 . ] ;				    \ [s3] = x@s4                           sd s4, 0(s3)

: x: ( ul ur -- )
  [ 03 . B3 . 09 . 00 . ] ^				    \ ur@t1 = [s3++8]                       ld t1, 0(s3)
  [ 83 . B2 . 09 . 00 . ] ^				    \ ul@t0 = [s3++8]                       ld t0, 0(s3)
  [ 33 . 5A . 6A . 00 . ]				    \ x@s4 >>= ur@t1                        srl s4, s4, t1
  [ B3 . 82 . 62 . 40 . ]				    \ mask@t1 = (0xFF << (ul - ur)) >> 7    sub t0, t0, t1
  [ 13 . 03 . F0 . 0F . ]				    \                                       addi t1, zero, 0xFF
  [ 33 . 13 . 53 . 00 . ]				    \                                       sll t1, t1, t0
  [ 13 . 53 . 73 . 00 . ]				    \                                       srli t1, t1, 7
  [ 33 . 7A . 6A . 00 . ] ;				    \ x@s4 &= mask@t1                       and s4, s4, t1

: x<< ( u -- )
  [ 83 . B2 . 09 . 00 . ] ^				    \ u@t0 = [s3++8]                        ld t0, 0(s3)
  [ 33 . 1A . 5A . 00 . ] ;				    \ x@s4 <<= u@t0                         sll s4, s4, t0

: x| ( u -- )
  [ 83 . B2 . 09 . 00 . ] ^				    \ u@t0 = [s3++8]                        ld t0, 0(s3)
  [ 33 . 6A . 5A . 00 . ] ;				    \ x@s4 |= u@t0                          or s4, s4, t0

: >x ( n -- n )
  \ ld s4, n(s3)
  \ {{{off(0x??.?)}}} {{{rs1(1001.1)}}} {{{fn3(011)}}} {{{rd(1010.0)}}} {{{op(0000011)}}}
  \ 03 BA ?9 ??
  03 . BA .				                    \ [s1++] = 0x03; [s1++] = 0xBA
  .>x  3 0 x:  4 x<<  09 x|  0 x>. .
  .>x  B 4 x:                  x>. . ;

: x> ( ? -- n )
  \ ld s4, n(s3)
  \ {{{off(???????)}}} {{{rs2(1.0100)}}} {{{rs1(1001.1)}}} {{{fn3(011)}}} {{{off(????.?)}}} {{{op(0100011)}}}
  \ ?3 B? 49 ??
  .>x  0 0 x:  7 x<<  23 x|  0 x>. .
  .>x  4 1 x:         B0 x|  0 x>. .
  49                               .
  .>x  B 5 x:  1 x<<  01 x|    x>. . ;

: NEGATE ( n -- n' )
  [ 83 . B2 . 09 . 00 . ] 				    \ n@t0 = [s3]                           ld t0, 0(s3)
  [ B3 . 02 . 50 . 40 . ]                                   \ n@t0 = -n@t0                          sub t0, zero, t0
  [ 23 . B0 . 59 . 00 . ] ;				    \ [s3] = n@t0                           sd t0, 0(s3)

: test [ 8 >x ]
       [ 0 x> ]
       [ 8 x> ]
       [ 8 NEGATE x> ] ;

1 2 3 test
dbg BYE






: 2* ( n -- n' )
  .>x
  1 x<<
  x>. ;







\ : + ( n1 n2 -- n )
\   [ t0 0 s3 ld ]
\   [ t1 8 s3 ld ]
\   [ t0 t0 t1 add ]
\   [ s3 s3 8 addi ]
\   [ t0 8 s3 sd ] ;

: ROT ( n1 n2 n3 -- n2 n3 n1 )
  10 >x 8 >y 0 >x
  10 z> 8 y> 0 x> ;

: + ( n1 n2 -- n )
  0 >x ^ 0 >y
  [ add ... ]
  0 x> ;



\ : DROP ( n -- )  [ >_ ] ;
\ : DUP ( n -- n n )  [ >x x> x> ] ;
\ : SWAP ( n1 n2 -- n2 n1 )  [ >y >x y> x> ] ;
\ : OVER ( n1 n2 -- n1 n2 n1 )  [ >y >x x> y> x> ] ;
\ \ : ROT  ( n1 n2 n3 -- n2 n3 n1 )  [ >x SWAP >y x> y> ] ;
\ \ : 2SWAP ( d1 d2 -- d2 d1 )
\ \ : 2DUP ( d -- d d )
\ \ : 2OVER ( d1 d2 -- d1 d2 d1 )
\ \ : 2DROP ( d1 d2 -- d1 )



: >>1 ( n -- n' )  >x  13 . 5A . 1A . 00 .  x> ;
: >>2 ( n -- n' )  >x  13 . 5A . 2A . 00 .  x> ;
: >>4 ( n -- n' )  >x  13 . 5A . 4A . 00 .  x> ;
: >>8 ( n -- n' )  >x  13 . 5A . 8A . 00 .  x> ;

: <<1 ( n -- n' )  >x  13 . 1A . 1A . 00 .  x> ;
: <<2 ( n -- n' )  >x  13 . 1A . 2A . 00 .  x> ;
: <<4 ( n -- n' )  >x  13 . 1A . 4A . 00 .  x> ;
: <<8 ( n -- n' )  >x  13 . 1A . 8A . 00 .  x> ;

: & ( n1 n2 -- n ) >y >x  33 . 7A . 5A . 01 .  x> ;
: | ( n1 n2 -- n ) >y >x  33 . 6A . 5A . 01 .  x> ;

: rinstr ( op rd fn3 rs1 rs2 fn7 -- )
  [ >r  DUP >>4 >y  | .					    \ 0000000 r
  



: @ ( addr -- n )
  [ >r  03 . 3A . 0A . 00 .  r> ] ;

: 1+ ( n|u -- n'|u' )  [ >x  13 . 0A . 1A . 00 .  x> ] ;


\ : >REG ( n reg -- )
\   59 . 00 . ;

\ : EMIT ( char -- )
\   [ ??? ] A >REG		\ a0 <- ?
\   1 B >REG			\ a1 <- 1
\   TYPE ;


0E 07 &

dbg BYE














\ : 1+ ( n|u -- n'|u' )  [        
\      83 . B2 . 09 . 00 .	\ [STACK@s3]++        ld t0, 0(s3)
\      93 . 82 . 12 . 00 .	\                     addi t0, t0, 1
\      23 . B0 . 59 . 00 . ] ;	\                     sd t0, 0(s3)

\ 1 2 3 4
\ 1+



( comment works )
\ line comment works.

( empty line ^ works )

\ : ADIOS ( -- )  BYE ;  \ works
\ ADIOS          \ works

\ : PAPA ( -- )  ADIOS ;  \ works
\ PAPA            \ works

\ : ( -- )  DIE-IN-THE-MIDDLE [ BYE ] ;  \ works

\ : 1+ ( n|u -- n'|u' )  [        
\      00 . 09 . B2 . 83 .	\ [STACK@s3]++      ld t0, 0(s3)
\      00 . 12 . 82 . 93 .	\                   addi t0, t0, 1
\      00 . 59 . B0 . 23 . ] ;	\                   sd t0, 0(s3)
\ : 1-        ( n|u -- n'|u' )    [ 49 . FF . 0F . ] ;                \ [r15]--           dec r/m64       REX.W FF /1     00 001 111  OneMinus
\ : 2*        (   x -- x'    )    [ 49 . D1 . 27 . ] ;                \ [r15] <<= 1       sal r/m64, 1    REX.W D1 /4     00 100 111  TwoTimes
\ : 4*        (   x -- x'    )    [ 49 . C1 . 27 . 02 . ] ;           \ [r15] <<= 2       sal r/m64, imm8 REX.W C1 /4 ib  00 100 111
\ : 8*        (   x -- x'    )    [ 49 . C1 . 27 . 03 . ] ;           \ [r15] <<= 3       sal r/m64, imm8 REX.W C1 /4 ib  00 100 111
\ : 2/        (   x -- x'    )    [ 49 . D1 . 3F . ] ;                \ [r15] >>= 1       sar r/m64, 1    REX.W D1 /7     00 111 111  TwoDiv
