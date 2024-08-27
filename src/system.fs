: >_ ( n -- )  93 . 89 . 89 . 00 . ;

: DROP ( n -- )
  [ >_ ] ;					      \ addi s3, s3, 8


1 2 3 DROP 4
dbg BYE


: >r ( n -- )
  [ 03 . BA . 09 . 00 . ] DROP ;    \ s4 ← [STACK@s3]     ld s4, 0(s3)
: >s ( n -- )
  [ 83 . BA . 09 . 00 . ] DROP ;    \ s5 ← [STACK@s3]     ld s5, 0(s3)
: r> ( -- n )
  0 [ 23 . B0 . 49 . 01 . ] ;	    \ [STACK@s3] ← s4     sd s4, 0(s3)
: s> ( -- n )
  0 [ 23 . B0 . 59 . 01 . ] ;	    \ [STACK@s3] ← s5     sd s5, 0(s3)

: R>r ( -- )

: DUP  ( n -- n n )  >r r> r> ;
: SWAP ( n1 n2 -- n2 n1 )  >s >r s> r> ;
: OVER ( n1 n2 -- n1 n2 n1 )  >s >r r> s> r> ;
: ROT  ( n1 n2 n3 -- n2 n3 n1 )  >r SWAP >s r> s> ;

1 2 3 >r SWAP 
dbg BYE

: >>1 ( n -- n' )  >r [ 13 . 5A . 1A . 00 . ] r> ;
: >>2 ( n -- n' )  >r [ 13 . 5A . 2A . 00 . ] r> ;
: >>4 ( n -- n' )  >r [ 13 . 5A . 4A . 00 . ] r> ;
: >>8 ( n -- n' )  >r [ 13 . 5A . 8A . 00 . ] r> ;

: <<1 ( n -- n' )  >r [ 13 . 1A . 1A . 00 . ] r> ;
: <<2 ( n -- n' )  >r [ 13 . 1A . 2A . 00 . ] r> ;
: <<4 ( n -- n' )  >r [ 13 . 1A . 4A . 00 . ] r> ;
: <<8 ( n -- n' )  >r [ 13 . 1A . 8A . 00 . ] r> ;

: & ( n1 n2 -- n ) >s >r [ 33 . 7A . 5A . 01 . ] r> ;
: | ( n1 n2 -- n ) >s >r [ 33 . 6A . 5A . 01 . ] r> ;

: rinstr ( op rd fn3 rs1 rs2 fn7 -- )
  >r  DUP >>4 >s  | .					    \ 0000000 r
  



: @ ( addr -- n )
  >r [ 03 . 3A . 0A . 00 . ] r> ;			\ ld s4, 0(s4)

: 1+ ( n|u -- n'|u' )  >r
     [ 13 . 0A . 1A . 00 . ]	\ addi s4, s4, 1
     r> ;


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
