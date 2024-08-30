: DBG   dbg BYE ;
: REG   reg BYE ;

\ Navigate the stack.
: ^ ( n -- )
  93 . 89 . 89 . 00 . ;                                     \ s3 += 8                 addi s3, s3, 8
: v ( n -- )									      
  93 . 89 . 89 . FF . ;                                     \ s3 -= 8                 addi s3, s3, -8

\ Load current stack item into register `t0` or `t1`.
: >t ( n -- n )									      
  83 . B2 . 09 . 00 . ;	   			            \ t0 = [s3]               ld t0, 0(s3)
: >u ( n -- n )									      
  03 . B3 . 09 . 00 . ;	   			            \ t1 = [s3]               ld t1, 0(s3)

\ Store register `t0` or `t1` over current stack item.
: t> ( ? -- n )									      
  23 . B0 . 59 . 00 . ;		                            \ [s3] = t0               sd t0, 0(s3)
: u> ( ? -- n )									      
  23 . B0 . 69 . 00 . ;		                            \ [s3] = t1               sd t1, 0(s3)

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

: DROP ( n -- )  [ ^ ] ;
: DUP  ( n -- n n )  [ >t v t> ] ;
: SWAP ( n1 n2 -- n2 n1 )  [ % v ] ;
: OVER ( n1 n2 -- n1 n2 n1 )  [ ^ >t v v t> ] ;
: ROT  ( n1 n2 n3 -- n2 n3 n1 )  [ ^ % v v % v ]  ;
: 2SWAP ( d1 d2 -- d2 d1 )  [ ^ % % v v v % % v v ] ;
: 2DUP ( d -- d d )  [ ^ >t v >u v t> v u> ] ;
: 2OVER ( d1 d2 -- d1 d2 d1 )  [ ^ ^ ^ >t v >u v v v t> v u> ] ;
: 2DROP ( d1 d2 -- d1 )  [ ^ ^ ] ;

: << ( n u -- n' )
  [ >u ^ >t ]
  [ B3 92 62 00 :: ]		\ sll t0, t0, t1
  [      t> ] ;
: & ( n1 n2 -- n )
  [ >u ^ >t ]
  [ B3 F2 62 00 :: ]		\ and t0, t0, t1
  [      t> ] ;
: | ( n1 n2 -- n )
  [ >u ^ >t ]
  [ B3 E2 62 00 :: ]		\ or t0, t0, t1
  [      t> ] ;
: + ( n1 n2 -- n )
  [ >u ^ >t ]
  [ B3 82 62 00 :: ]		\ add t0, t0, t1
  [      t> ] ;
: - ( n1 n2 -- n )
  [ >u ^ >t ]
  [ B3 82 62 40 :: ]		\ sub t0, t0, t1
  [      t> ] ;

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

\ --- Assembler --- \

\ Common format for R/S/B-type instructions.
: `instr/rsb ( op rd/imm5 fn3 rs1 rs2 fn7/imm7 -- )
  5 << | 5 << | 3 << | 5 << | 7 << | ` ;

\ R-type instructions.
: `instr/r ( rd rs1 rs2 fn7 fn3 op -- )
  [ % % % % % v v v v v ]  ( -- op rd rs1 rs2 fn7 fn3 )
  [ % % %         v v v ]  ( -- op rd fn3 rs1 rs2 fn7 )
  `instr/rsb ;
: `add  ( rd rs1 rs2 -- )  00 0 33 `instr/r ;
: `sub  ( rd rs1 rs2 -- )  20 0 33 `instr/r ;
: `sll  ( rd rs1 rs2 -- )  00 1 33 `instr/r ;
: `slt  ( rd rs1 rs2 -- )  00 2 33 `instr/r ;
: `sltu ( rd rs1 rs2 -- )  00 3 33 `instr/r ;
: `xor  ( rd rs1 rs2 -- )  00 4 33 `instr/r ;
: `srl  ( rd rs1 rs2 -- )  00 5 33 `instr/r ;
: `sra  ( rd rs1 rs2 -- )  20 5 33 `instr/r ;
: `or   ( rd rs1 rs2 -- )  00 6 33 `instr/r ;
: `and  ( rd rs1 rs2 -- )  00 7 33 `instr/r ;
\ RV64 instructions.
: `addw ( rd rs1 rs2 -- )  00 0 3B `instr/r ;
: `subw ( rd rs1 rs2 -- )  20 0 3B `instr/r ;
: `sllw ( rd rs1 rs2 -- )  00 1 3B `instr/r ;
: `srlw ( rd rs1 rs2 -- )  00 5 3B `instr/r ;
: `sraw ( rd rs1 rs2 -- )  20 5 3B `instr/r ;

\ I-type instructions.
: `instr/i ( rd rs1 imm fn3 op -- )
  [ % % % % v v v v ]  ( -- op rd rs1 imm fn3 )
  [ % %         v v ]  ( -- op rd fn3 rs1 imm )
  5 << | 3 << | 5 << | 7 << | ` ;
: `instr/i/shift ( rd rs1 shamt fn7 fn3 op -- )
  2SWAP		 ( -- rd rs1 fn3 op shamt fn7 )
  5 << |	 ( -- rd rs1 fn3 op imm )
  ROT ROT	 ( -- rd rs1 imm fn3 op )
  `instr/i ;
: `ecall ( -- )               0 0 000 0 73 `instr/i ;
: `jalr  ( rd rs1 imm   -- )          0 67 `instr/i ;
: `lb    ( rd rs1 imm   -- )          0 03 `instr/i ;
: `lh    ( rd rs1 imm   -- )          1 03 `instr/i ;
: `lw    ( rd rs1 imm   -- )          2 03 `instr/i ;
: `lbu   ( rd rs1 imm   -- )          4 03 `instr/i ;
: `lhu   ( rd rs1 imm   -- )          5 03 `instr/i ;
: `addi  ( rd rs1 imm   -- )          0 13 `instr/i ;
: `slti  ( rd rs1 imm   -- )          2 13 `instr/i ;
: `sltiu ( rd rs1 imm   -- )          3 13 `instr/i ;
: `xori  ( rd rs1 imm   -- )          4 13 `instr/i ;
: `ori   ( rd rs1 imm   -- )          6 13 `instr/i ;
: `andi  ( rd rs1 imm   -- )          7 13 `instr/i ;
: `slli  ( rd rs1 shamt -- )       00 1 13 `instr/i/shift ;
: `srli  ( rd rs1 shamt -- )       00 5 13 `instr/i/shift ;
: `srai  ( rd rs1 shamt -- )       20 5 13 `instr/i/shift ;
\ RV64 instructions.	          
: `lwu   ( rd rs1 imm   -- )          6 03 `instr/i ;
: `ld    ( rd rs1 imm   -- )          3 03 `instr/i ;
: `addiw ( rd rs1 imm   -- )          0 1B `instr/i ;
: `slliw ( rd rs1 shamt -- )       00 1 1B `instr/i/shift ;
: `srliw ( rd rs1 shamt -- )       00 5 1B `instr/i/shift ;
: `sraiw ( rd rs1 shamt -- )       20 5 1B `instr/i/shift ;

\ S-type instructions.
: `instr/s ( rs2 rs1 offset fn3 op -- )
  [ % % % % v v v v ]  ( -- op rs2 rs1 offset fn3 )
  [ % % %     v v v ]  ( -- op fn3 rs2 rs1 offset )
  DUP 4 0 [:]          ( -- op fn3 rs2 rs1 offset imm5 )
  [ % % % % v v v v ]  ( -- op imm5 fn3 rs2 rs1 offset )
  B 5 [:]              ( -- op imm5 fn3 rs2 rs1 imm7 )
  [ ^ %         v v ]  ( -- op imm5 fn3 rs1 rs2 imm7 )
  `instr/rsb ;
: `sb  ( rs2 rs1 offset -- )  0 23 `instr/s ;
: `sh  ( rs2 rs1 offset -- )  1 23 `instr/s ;
: `sw  ( rs2 rs1 offset -- )  2 23 `instr/s ;
: `sd  ( rs2 rs1 offset -- )  3 23 `instr/s ;

\ B-type instructions.
: `instr/b ( rs1 rs2 offset fn3 op -- )
  [ % % % % v v v v ]   ( -- op rs1 rs2 offset fn3 )
  [ % % %     v v v ]   ( -- op fn3 rs1 rs2 offset )
  DUP DUP  4 1 [:]      ( -- op fn3 rs1 rs2 offset offset offset[4:1] )
  1 << SWAP  B B [:] |  ( -- op fn3 rs1 rs2 offset imm5 )
  [ % % % % v v v v ]   ( -- op imm5 fn3 rs1 rs2 offset )
  DUP  C C [:]	        ( -- op imm5 fn3 rs1 rs2 offset offset[12] )
  6 << SWAP  A 5 [:] |  ( -- op imm5 fn3 rs1 rs2 imm7 )
  `instr/rsb ;
: `beq  ( rs1 rs2 offset -- )  0 63 `instr/b ;
: `bne  ( rs1 rs2 offset -- )  1 63 `instr/b ;
: `blt  ( rs1 rs2 offset -- )  4 63 `instr/b ;
: `bge  ( rs1 rs2 offset -- )  5 63 `instr/b ;
: `bltu ( rs1 rs2 offset -- )  6 63 `instr/b ;
: `bgeu ( rs1 rs2 offset -- )  7 63 `instr/b ;

\ Common format for U/J-type instructions.
: `instr/uj  ( op rd imm20 -- )
  5 << | 7 << |  ;

\ U-type instructions.
: `instr/u ( rd imm opcode -- )
  ROT ROT `instr/uj ;
: `lui   ( rd imm -- )  37 `instr/u ;
: `auipc ( rd imm -- )  17 `instr/u ;

\ J-type instructions.
: `instr/j ( rd offset opcode -- )
  ROT ROT               ( -- opcode rd offset )
  DUP 13 0C [:] SWAP    ( -- opcode rd offset[19:12] offset )
  DUP 0B 0B [:] SWAP    ( -- opcode rd offset[19:12] offset[10] offset )
  DUP 0A 01 [:] SWAP    ( -- opcode rd offset[19:12] offset[10] offset[10:1] offset )
      14 14 [:]	        ( -- opcode rd offset[19:12] offset[10] offset[10:1] offset[20] )
  A << | 1 << | 8 << |  ( -- opcode rd imm20 )
  `instr/uj ;
: `jal ( rd offset -- )  6F `instr/j ;








\ TODO:
\
\ : @ ( addr -- n )
\   [ >r  03 . 3A . 0A . 00 .  r> ] ;
\ : EMIT ( char -- )
\   [ ??? ] A >REG		\ a0 <- ?
\   1 B >REG			\ a1 <- 1
\   TYPE ;

\ TODO:
\
\ : 2*        (   x -- x'    )    [ 49 . D1 . 27 . ] ;                \ [r15] <<= 1       sal r/m64, 1    REX.W D1 /4     00 100 111  TwoTimes
\ : 4*        (   x -- x'    )    [ 49 . C1 . 27 . 02 . ] ;           \ [r15] <<= 2       sal r/m64, imm8 REX.W C1 /4 ib  00 100 111
\ : 8*        (   x -- x'    )    [ 49 . C1 . 27 . 03 . ] ;           \ [r15] <<= 3       sal r/m64, imm8 REX.W C1 /4 ib  00 100 111
\ : 2/        (   x -- x'    )    [ 49 . D1 . 3F . ] ;                \ [r15] >>= 1       sar r/m64, 1    REX.W D1 /7     00 111 111  TwoDiv
