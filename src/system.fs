: DBG   dbg BYE ;
: REG   reg BYE ;

\ Navigate the stack.
: ^ ( n -- )
  93 . 89 . 89 . 00 . ;                                     \ s3 += 8                                  addi s3, s3, 8
: v ( n -- )											       
  93 . 89 . 89 . FF . ;                                     \ s3 -= 8                                  addi s3, s3, -8

\ Load current stack item into register `t0` or `t1`.
: >t ( n -- n )											       
  83 . B2 . 09 . 00 . ;	   			            \ t0 = [s3]                                ld t0, 0(s3)
: >u ( n -- n )											       
  03 . B3 . 09 . 00 . ;	   			            \ t1 = [s3]                                ld t1, 0(s3)

\ Store register `t0` or `t1` over current stack item.
: t> ( ? -- n )											       
  23 . B0 . 59 . 00 . ;		                            \ [s3] = t0                                sd t0, 0(s3)
: u> ( ? -- n )											       
  23 . B0 . 69 . 00 . ;		                            \ [s3] = t1                                sd t1, 0(s3)

\ "Float" current stack item "up" the stack, exchanging with item
\ above.  Stack pointer follows the item.
: % ( n1 n2 -- n2 .. )
  >u ^  >t v
  t> ^  u> ;

\ Shorthand for compiling 4 bytes to `OUTPUT`, ensuring correct
\ endianness.
: :: ( ub1 ub2 ub3 ub4 -- )
  [ ^ ^ ^ >t v v v v t> ] .                                 \ [s1++] = ub1  ( -- ub1 ub2 ub3 ub4 )
  [ ^ ^   >t   v v v t> ] .                                 \ [s1++] = ub2  ( -- ub1 ub2 ub3 ub4 )
  [ ^     >t     v v t> ] .                                 \ [s1++] = ub3  ( -- ub1 ub2 ub3 ub4 )
  [                     ] .                                 \ [s1++] = ub4  ( -- ub1 ub2 ub3 ub4 )
  [ ^ ^ ^ ] ;		                                    \ s3 += 32      ( -- )  

\ Compiles a 32-bit instruction (four bytes in the form of a 32-bit
\ unsigned integer) to `OUTPUT`, ensuring correct endianness.
: ` ( uw -- )
  [ 83 C2 09 00 :: ] 	   			            \ t0 = [s3 + 0]                            lbu t0, 0(s3)
  [ v t> ] .                                                \ [s3] = t0                                ---
  [ 83 C2 19 00 :: ]				            \ t0 = [s3 + 1]                            lbu t0, 1(s3)
  [ v t> ] .                                                \ [s3] = t0                                ---
  [ 83 C2 29 00 :: ]				            \ t0 = [s3 + 2]                            lbu t0, 2(s3)
  [ v t> ] .                                                \ [s3] = t0                                ---
  [ 83 C2 39 00 :: ]			         	    \ t0 = [s3 + 3]                            lbu t0, 3(s3)
  [   t> ] . ;                                              \ [s3] = t0                                ---

: SWAP ( n1 n2 -- n2 n1 )
  [ >u ^ >t ]
  [ u> v t> ] ;

: << ( n u -- n' )
  [ >u ^ ]
  [ >t ^ ]
  [ B3 92 62 00 :: ]		\ sll t0, t0, t1
  [ v t> ] ;

: | ( n1 n2 -- n )
  [ >u ^ ]
  [ >t ^ ]
  [ B3 E2 62 00 :: ]		\ or t0, t0, t1
  [ v t> ] ;

: & ( n1 n2 -- n )
  [ >u ^ ]
  [ >t ^ ]
  [ B3 F2 62 00 :: ]		\ and t0, t0, t1
  [ v t> ] ;

: + ( n1 n2 -- n )
  [ >u ^ ]
  [ >t ^ ]
  [ B3 82 62 00 :: ]		\ add t0, t0, t1
  [ v t> ] ;

: - ( n1 n2 -- n )
  [ >u ^ ]
  [ >t ^ ]
  [ B3 82 62 40 :: ]		\ sub t0, t0, t1
  [ v t> ] ;

: 1+ ( n -- n' )  1 + ;
: 1- ( n -- n' )  1 - ;

: [:] ( n i j -- n' )
  [ >u ]			\ t1 = j
  [ ^ ^ >t ]			\ t0 = n                   ( -- n .. )
  [ B3 D2 62 00 :: ]		\ srl t0, t0, t1
  [ t> v v ]		        \ m = t0                   ( -- m i j )
  - 1+			        \ len = i - j + 1          ( -- m len )
  1 SWAP << 1-		        \ mask = (1 << len) - 1    ( -- m mask )
  & ;                           \ n' = m & mask            ( -- n' )

: % ( n1 n2 -- n2 .. )  SWAP [ ^ ] ;

\ R-type instructions.
: rinstr ( rd rs1 rs2 fn7 fn3 op -- instr )
  % % % % % [ v v v v v ]  ( -- op rd rs1 rs2 fn7 fn3 )
  % % %     [     v v v ]  ( -- op rd fn3 rs1 rs2 fn7 )
  5 << | 5 << | 3 << | 5 << | 7 << | ;
: `add  ( rd rs1 rs2 -- instr )  00 0 33 rinstr ` ;
: `sub  ( rd rs1 rs2 -- instr )  20 0 33 rinstr ` ;
: `sll  ( rd rs1 rs2 -- instr )  00 1 33 rinstr ` ;
: `slt  ( rd rs1 rs2 -- instr )  00 2 33 rinstr ` ;
: `sltu ( rd rs1 rs2 -- instr )  00 3 33 rinstr ` ;
: `xor  ( rd rs1 rs2 -- instr )  00 4 33 rinstr ` ;
: `srl  ( rd rs1 rs2 -- instr )  00 5 33 rinstr ` ;
: `sra  ( rd rs1 rs2 -- instr )  20 5 33 rinstr ` ;
: `or   ( rd rs1 rs2 -- instr )  00 6 33 rinstr ` ;
: `and  ( rd rs1 rs2 -- instr )  00 7 33 rinstr ` ;
\ RV64 instructions
: `addw ( rd rs1 rs2 -- instr )  00 0 3B rinstr ` ;
: `subw ( rd rs1 rs2 -- instr )  20 0 3B rinstr ` ;
: `sllw ( rd rs1 rs2 -- instr )  00 1 3B rinstr ` ;
: `srlw ( rd rs1 rs2 -- instr )  00 5 3B rinstr ` ;
: `sraw ( rd rs1 rs2 -- instr )  20 5 3B rinstr ` ;


: +' ( n1 n2 -- n )
  [ >u ^ >t ]
  [ 05 05 06 `add ]
  [ t> ] ;

3 4 +' 5 +'
DBG

  


: add ( rd rs1 rs2 -- )
  [ 0000000 000 0110011 rinstr ] ;
: sub ( rd rs1 rs2 -- )
  [ 0100000 000 0110011 rinstr ] ;

: beq ( rs1 rs2 offset -- )
  [ ... .. binstr ] ;





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
