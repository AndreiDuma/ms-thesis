: DBG   dbg BYE ;
: REG   reg BYE ;

\ Load current stack item into register `t0` or `t1`.
: >t0 ( n -- n )									      
  83 . B2 . 09 . 00 . ;	   			            \ t0 = [s3]               ld t0, 0(s3)
: >t1 ( n -- n )									      
  03 . B3 . 09 . 00 . ;	   			            \ t1 = [s3]               ld t1, 0(s3)

\ Store register `t0` or `t1` over current stack item.
: t0> ( ? -- n )									      
  23 . B0 . 59 . 00 . ;		                            \ [s3] = t0               sd t0, 0(s3)
: t1> ( ? -- n )									      
  23 . B0 . 69 . 00 . ;		                            \ [s3] = t1               sd t1, 0(s3)

\ Navigate the stack.
: ^ ( n -- )
  93 . 89 . 89 . 00 . ;                                     \ s3 += 8                 addi s3, s3, 8
: v ( n -- )									      
  93 . 89 . 89 . FF . ;                                     \ s3 -= 8                 addi s3, s3, -8

\ "Float" current stack item "up" the stack, exchanging with item
\ above.  Stack pointer follows the item.
: % ( n1 n2 -- n2 .. )
  >t1 ^  >t0 v
  t0> ^  t1> ;

: DROP ( n -- )  [ ^ ] ;
: DUP  ( n -- n n )  [ >t0 v t0> ] ;
: SWAP ( n1 n2 -- n2 n1 )  [ % v ] ;
: OVER ( n1 n2 -- n1 n2 n1 )  [ ^ >t0 v v t0> ] ;
: ROT  ( n1 n2 n3 -- n2 n3 n1 )  [ ^ % v v % v ]  ;
: 2SWAP ( d1 d2 -- d2 d1 )  [ ^ % % v v v % % v v ] ;
: 2DUP ( d -- d d )  [ ^ >t0 v >t1 v t0> v t1> ] ;
: 2OVER ( d1 d2 -- d1 d2 d1 )  [ ^ ^ ^ >t0 v >t1 v v v t0> v t1> ] ;
: 2DROP ( d1 d2 -- d1 )  [ ^ ^ ] ;


\ --- RISC-V Assembler --- \

: << ( n u -- n' )
  [ >t1 ^ >t0 ]
  [ B3 . 92 . 62 . 00 . ]		\ sll t0, t0, t1
  [       t0> ] ;

: | ( u1 u2 -- u )
  [ >t1 ^ >t0 ]
  [ B3 . E2 . 62 . 00 . ]		\ or t0, t0, t1
  [       t0> ] ;

: & ( u1 u2 -- u )
  [ >t1 ^ >t0 ]
  [ B3 . F2 . 62 . 00 . ]		\ and t0, t0, t1
  [       t0> ] ;

: - ( n1 n2 -- n )
  [ >t1 ^ >t0 ]
  [ B3 . 82 . 62 . 40 . ]		\ sub t0, t0, t1
  [       t0> ] ;

\ Pick bits `i` (high) through `j` (low) from `n`.
: [:] ( u i j -- u' )
  [ >t1 ]		      \ t1 = j
  [ ^ ^ >t0 ]		      \ t0 = u                   ( -- u .. )
  [ B3 . D2 . 62 . 00 . ]     \ srl t0, t0, t1
  [ t0> v v ]		      \ v = t0                   ( -- v i j )
  1 -  -		      \ len = i - (j - 1)        ( -- v len )
  1 SWAP <<  1 -	      \ mask = (1 << len) - 1    ( -- v mask )
  & ;			      \ u' = v & mask            ( -- u' )

\ Compile a 32-bit instruction (in the form of a 32-bit unsigned
\ integer) to `OUTPUT`, ensuring correct endianness.
: ` ( u -- )
  DUP 1F 18 [:] SWAP  ( -- u[31:24] u )
  DUP 17 10 [:] SWAP  ( -- u[31:24] u[23:16] u )
  DUP 0F 08 [:] SWAP  ( -- u[31:24] u[23:16] u[15:8] u )
      07 00 [:]       ( -- u[31:24] u[23:16] u[15:8] u[7:0] )
  . . . . ;

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
  5 << | 7 << | ` ;

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

\ ABI names for registers.
: zero 00 ;   : s0   08 ;   : a6  10 ;   : s8  18 ;
: ra   01 ;   : s1   09 ;   : a7  11 ;   : s9  19 ;
: sp   02 ;   : a0   0A ;   : s2  12 ;   : s10 1A ;
: gp   03 ;   : a1   0B ;   : s3  13 ;   : s11 1B ;
: tp   04 ;   : a2   0C ;   : s4  14 ;   : t3  1C ;
: t0   05 ;   : a3   0D ;   : s5  15 ;   : t4  1D ;
: t1   06 ;   : a4   0E ;   : s6  16 ;   : t5  1E ;
: t2   07 ;   : a5   0F ;   : s7  17 ;   : t6  1F ;


\ --- Common FORTH words --- \

\ Arithmetic.
: NEGATE ( n -- n' )     [ >t0 ]       [ t0 zero t0 `sub ] [ t0> ] ;
: +      ( n1 n2 -- n )  [ >t1 ^ >t0 ] [ t0   t0 t1 `add ] [ t0> ] ;
: *      ( n1 n2 -- n )          ;
: /MOD   ( n1 n2 -- rem quot )   ;
: /      ( n1 n2 -- quot )       ;
: MOD    ( n1 n2 -- rem )        ;
: */    ( n1 n2 n3 -- n )        ; 
: */MOD ( n1 n2 n3 -- rem quot ) ;
: 1+     ( n -- n' )     [ >t0 ]       [ t0 t0   1 `addi ] [ t0> ] ;
: 1-     ( n -- n' )     [ >t0 ]       [ t0 t0 FFF `addi ] [ t0> ] ;
: 2*     ( n -- n' )     [ >t0 ]       [ t0 t0   1 `slli ] [ t0> ] ;
: 2/     ( n -- n' )     [ >t0 ]       [ t0 t0   1 `srai ] [ t0> ] ;
: ABS    ( n -- n' )     [ >t0 >t1 ]   [ t1 t1  3F `srai ]            \ mask@t1 >>= 63
	                               [ t0 t0  t1 `xor  ]            \ t0 = t0 xor mask@t1
				       [ t0 t0  t1 `sub  ] [ t0> ] ;  \ t0 -= mask@t1
: MIN    ( n1 n2 -- nmin )  2DUP -          ( -- n1 n2 n1-n2 )
                            DUP ABS         ( -- n1 n2 n1-n2 |n1-n2| )
			    - 2/ +          ( -- n1 nmin )
			    SWAP DROP ;     ( -- nmin )
: MAX    ( n1 n2 -- n )  2DUP -  DUP ABS  + 2/ +  SWAP DROP ;

\ Memory access.
: C! ( c addr -- )  [ >t1 ^ >t0 ^ ] [ t0 t1 0 `sb  ] ;
: C@ ( addr -- c )  [ >t1         ] [ t0 t1 0 `lbu ] [ t0> ] ;
: !  ( n addr -- )  [ >t1 ^ >t0 ^ ] [ t0 t1 0 `sd  ] ;
: @  ( addr -- n )  [ >t1         ] [ t0 t1 0 `ld  ] [ t0> ] ;
: +! ( n addr -- )  DUP @  ( -- n addr n0 )
                    ROT +  ( -- addr n' )
		    SWAP ! ;

\ Return stack management.
: >R    ( n -- ) ;
: R>    ( -- n ) ;
: R@    ( -- n ) ;

\ Control flow.
: IF ( flag -- ) ;
: = ( n1 n2 -- flag ) ;
: <> ( n1 n2 -- flag ) ;
: < ( n1 n2 -- flag ) ;
: > ( n1 n2 -- flag ) ;
\ : U< ( u1 u2 -- flag ) ;
\ : U> ( u1 u2 -- flag ) ;
: 0= ( n -- flag ) ;
: 0< ( n -- flag ) ;
: 0> ( n -- flag ) ;
: NOT ( n -- n' ) ;
: AND ( n1 n2 -- n ) ;
: OR  ( n1 n2 -- n ) ;
: XOR ( n1 n2 -- n ) ;
: ?DUP ( n -- n n  OR  0 -- 0 ) ;
: ABORT" ( "ccc<DOUBLE-QUOTE>" flag -- ) ;

\ Dictionary.
: VARIABLE ( C: "ccc" -- ) ( -- addr ) ;
: CONSTANT ( C: "ccc" n -- ) ( -- n ) ;

\ I/O.
\ TODO: SYSCALL
: KEY    ( -- c ) ;
: CR     ( -- ) ;
: SPACES ( -- ) ;
: SPACE  ( -- ) ;
: EMIT   ( c -- ) ;
: .      ( n -- ) ;
: ."     ( "ccc<DOUBLE-QUOTE>" -- ) ;
: ?      ( addr -- ) ;
