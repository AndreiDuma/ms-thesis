\ David Smith 2022 david.a.c.v.smith@gmail.com http://dacvs.neocities.org/
\ This is the 2nd of the 2 files that define SmithForth, a subroutine-threaded Forth for x86-64.
\ Read this file with the Intel manual in one hand and the Forth 2012 specification in the other.

\ dbg BYE

\ SmithForth is meant to be, in the language of the Forth 2012 standard, a:
\     Forth-2012 System
\         Providing the Core Extensions word set ( but SAVE-INPUT and RESTORE-INPUT are useless )
\         Providing the Double-Number Extensions word set
\         Providing the Programming-Tools word set
\         Providing AHEAD BYE FORGET SEE STATE [DEFINED] [UNDEFINED] [IF] [ELSE] [THEN] from the Programming-Tools Extensions word set
\ Elements of Forth-2012 compliance: (see Forth 2012 5.1 system compliance and 4.1 system documentation)
\ 4.1.1 Implementation-defined options:
\     Aligned addresses: Every address is character-aligned. Every address is aligned.
\     EMIT on nongraphic characters: Sends them to syscall write just the same. In my usual environment (Ubuntu, Bash), the output is:
\         0, 5, 7, 8, D-F: nothing
\         1-4, 6, 10-19: a strange glyph
\         9: a horizontal tab of 8 spaces
\         A-C: newline
\     ACCEPT : Character editing works as usual for Linux programs in plain old canonical terminal input mode.
\         Backspace works. Ctrl-D ends the input stream. Ctrl-C kills the process. Ctrl-Z suspends the process.
\     Character set: US-ASCII
\     Character-aligned address requirements: none
\     Character-set extensions matching characteristics: not offered
\     Conditions under which control characters match a space delimiter:
\         PARSE does not treat control characters as a space delimiter.
\         The following words do treat control characters as a space delimiter:
\             two parsing words from SForth.dmp: seek pname
\                 and their applications: PARSE-NAME CREATE ' DEFER POSTPONE [COMPILE] CHAR FORGET [CHAR] [']
\                     MARKER VARIABLE BUFFER: CONSTANT 2CONSTANT VALUE 2VALUE TO +TO -TO SEE DEFER ACTION-OF IS
\             WORD and its applications: [DEFINED] [UNDEFINED] [ELSE]
\     Format of control-flow stack: The control-flow stack is the data stack. Each cell of the control-flow stack is one cell of the data stack.
\     Conversion of digits larger than thirty-five: No BASE beyond thirty-six is supported. There are no digits larger than thirty-five.
\     Display after input terminates in ACCEPT : ok occurs on a new line, the line after the input text.
\     Exception abort sequence: exceptions are not implemented.
\     Input line terminator: LF , the usual Linux line terminator
\     Maximum size of a counted string: see ENVIRONMENT? /COUNTED-STRING
\     Maximum size of a parsed string: two hundred fifty-five bytes.
\     Maximum size of a definition name, in characters: thirty-one.
\     Maximum string length: 2^(2^6) - 1.
\     Method of selecting user  input device: there is only the standard input stream.
\     Method of selecting user output device: there is only the standard output stream.
\     Methods of dictionary compilation:
\         The dictionary is a sequence of entries.
\         Each entry consists of a header (see DICTIONARY FORMAT in file SForth.dmp) followed by an area for code and data.
\         First field of the header is the address of a subroutine (here subroutine is simply an address to call) associated with the entry.
\         Often the subroutine begins immediately after the header, but this is not required.
\         My dictionary entries customarily start at a multiple of sixteen, but this is not required.
\     Number of bits in one address unit: eight.
\     Ranges of number types:
\         -2^(2^6-1) <=  n < +2^(2^6-1)
\                  0 <= +n < +2^(2^6-1)     Note, +n implies n (see 3.1.1)
\                  0 <=  u < +2^(2^6  )
\         -2^(2^7-1) <=  d < +2^(2^7-1)
\                  0 <= +d < +2^(2^7-1)     Note, +d implies d (see 3.1.1)
\                  0 <= ud < +2^(2^7  )
\     Read-only data-space regions: none.
\     Size of WORD buffer: see wbuf
\     Size of one cell in address units: eight.
\     Size of one character in address units: one.
\     Size of the keyboard terminal input buffer: see kbuf
\     Size of the pictured numeric output string buffer: see bufLen
\     Size of the PAD scratch area: see bufLen
\     System case-sensitivity characteristics: case-sensitive, except for the usual case-insensitive treatment of numerals ABC... abc...
\     System prompt: see QUIT
\     Type of division rounding: floored as in FM/MOD
\     Value of STATE when true: 1.
\     Values returned after arithmetic overflow: The usual values under two's complement arithmetic.
\     Whether the current definition can be found after compilation of DOES> : No.
\ Definitions of conditions below:
\     segfault: The Forth system halts and returns control to the Linux system with a Linux return code indicating a segmentation violation.
\     corruption:
\         The operation proceeds without warning. If the operation writes, then possibly something important is disturbed.
\         The system continues to run with unpredictable behavior, possibly normal behavior, segfault, or other fatal error.
\ 4.1.2 Ambiguous conditions:
\     A name is neither a valid definition name nor a valid number: a warning is issued. Stacks and other state are preserved.
\     Addressing a region not listed in 3.3.3 Data space: segfault
\     Argument type incompatible with specified input parameter: There is no data type checking. This is a two's complement system.
\     Attempting to obtain the execution token of a definition (by e.g. Tick or FIND) with undefined interpretation semantics
\     Dividing by zero: floating point exception, system halts with warning.
\     Insufficient data-stack space or return-stack space: segfault.
\     Insufficient space for loop-control parameters: segfault (these parameters are kept on the return stack)
\     Insufficient space in the dictionary: segfault.
\     Interpreting a word with undefined interpretation semantics:
\         For lbracket ( [ ), the effect is its execution semantics.
\         For semicolon ( ; ), the effect is its compilation semantics.
\         For return-stack words ( >R R@ R> 2>R 2R@ 2R> ), string words ( ." C" S" S\" ), control-flow words ( IF etc. ), and some others
\             ( ['] [COMPILE] [CHAR] ), the effect is to compile the word's execution semantics into the dictionary (at HERE).
\         If there are more such words, please remind me. See also the source code.
\     Modifying the contents of the input buffer or a string literal: corruption.
\     Overflow of a pictured numeric output string: corruption.
\     Parsed string overflow: corruption.
\     Producing a result out of range, e.g., multiplication (using *) results in a value too big to be represented by a single-cell integer:
\         numeric overflow warning is issued and system halts.
\     Reading from an empty data stack or return stack: reading an empty data stack shouldn't be a problem
\         (the data will be from the beginning of the data area), but subsequent writes will cause corruption.
\         Reading from an empty return stack should yield the address of the caller of the text interpreter.
\         If the stack pointer advances farther than that, segfault.
\     Unexpected end of input buffer, resulting in an attempt to use a zero-length string as a name: corruption.
\     >IN greater than size of input buffer: there is no range checking. Accesses past the end of the buffer entail corruption.
\         However, >IN is compared to #IN by >= rather than by = . If >IN strays from interval [ 0 , #IN ] , it may be restored ( see REFILL ) .
\     RECURSE appears after DOES> : RECURSE refers to the current word, the word whose definition includes DOES> , not to DOES>
\     Argument input source different than current input source for RESTORE-INPUT : there is only one input source.
\     Data space containing definitions is de-allocated: corruption (without ill effect until the space is rewritten)
\     Data space read/write with incorrect alignment: Impossible. Every address is aligned.
\     Data space pointer not properly aligned: Impossible. Every address is aligned.
\     Less than u+2 stack items on PICK or ROLL : PICK has no range checking. PICK will return whatever happens to be beyond the stack.
\         If PICK attempts to fetch a cell beyond the maximum stack size, segfault. ROLL is similar.
\     Loop-control parameters not available: There is no error checking. The value in the expected place on the return stack is used.
\         I J : These words succeed, leaving meaningless values on the data stack.
\         +LOOP LOOP UNLOOP LEAVE : corruption.
\     Most recent definition does not have a name on IMMEDIATE : IMMEDIATE works nonetheless.
\     TO not followed directly by a name defined by a word with "TO name runtime" semantics: corruption.
\     Name not found: corruption. An invalid xt is returned with value 0 and the system proceeds without warning.
\     Parameters not of the same type ( DO ?DO WITHIN ): The system is unaware of the program's desired type.
\         Each of these words produces the same results whether the type is signed or unsigned.
\     POSTPONE [COMPILE] ' ['] applied to TO : They work on TO as they do on other words.
\     String longer than a counted string returned by WORD : corruption.
\     u >= #bits in a cell on LSHIFT or RSHIFT : the operation proceeds as if u modulo sixty-four were provided instead of u
\     Word not defined via CREATE on >BODY or DOES> :
\         >BODY runs as usual (without warning), even though the resulting xt has no special meaning
\         DOES> in a word not defined via CREATE : corruption.
\     Words improperly used outside <# #> including # #S HOLD HOLDS SIGN : They write as usual without any warning.
\     Access to a DEFERred word which has yet to be assigned to an xt : corruption.
\     Access to a DEFERred word which was not defined by DEFER : corruption.
\     POSTPONE [COMPILE] ' ['] applied to ACTION-OF or IS : They work on ACTION-OF and on IS as they do on other words.
\     \x is not followed by two hex characters on S\" : a (garbage) character is added to the string.
\     \ is placed before a character not defined in S\" : a null (zero) character is added to the string.
\     
\ SmithForth offers no support yet for floating-point arithmetic, local variables, exceptions, dynamic memory allocation (like malloc),
\     or access to blocks or files, other than by the standard input and output streams of the Linux process,
\         which may be redirected from or to files in the usual Unix way.
\ 
\ Environment of text interpreter (see SForth.dmp):
\     RSP: address of end cell of return stack. Return stack grows toward lesser addresses.
\     R15: address of end cell of data stack. Initially R15=10000000. Stack grows toward lesser addresses. 
\     RDI: data space pointer ( HERE )
\ Data space:
\     10000000           Forth data space begins
\     10000000 #IN       contains the number of characters in the current input line (including trailing LF)
\     10000008 TIB       contains the address where the current input line begins.
\     10000010 >IN       contains the number of characters in the current input line parsed.
\     10000018 source_id https://forth-standard.org/standard/core/SOURCE-ID
\     10000020 STATE     contains 0(Interpreting) or 1(Compiling).
\     10000028 latest    contains the address where the latest dictionary entry begins. See SForth.dmp for dictionary layout.
\     10000030           Forth dictionary begins
\     7FFFFFF8           reserved by EXEC
\   < 80000000           Forth data space ends, segment ends

\ TODO Measure speed. We use many instructions that take an operand from memory and return the result to memory. This is somewhat slow.
\ Brad Rodriguez in Moving Forth Part 1 (https://www.bradrodriguez.com/papers/moving1.htm) discusses keeping stack cells in registers.

\ Arithmetic, stack, etc. \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
\ 
\     Forth 2012: signed 2-cell integer is written d = ( lo hi ) ; unsigned 2-cell integer is written ud = ( lo hi ). Either: xd.

: 1+        ( n|u -- n'|u' )    [ 49 . FF . 07 . ] ;                \ [r15]++           inc r/m64       REX.W FF /0     00 000 111  OnePlus
: 1-        ( n|u -- n'|u' )    [ 49 . FF . 0F . ] ;                \ [r15]--           dec r/m64       REX.W FF /1     00 001 111  OneMinus
: 2*        (   x -- x'    )    [ 49 . D1 . 27 . ] ;                \ [r15] <<= 1       sal r/m64, 1    REX.W D1 /4     00 100 111  TwoTimes
: 4*        (   x -- x'    )    [ 49 . C1 . 27 . 02 . ] ;           \ [r15] <<= 2       sal r/m64, imm8 REX.W C1 /4 ib  00 100 111
: 8*        (   x -- x'    )    [ 49 . C1 . 27 . 03 . ] ;           \ [r15] <<= 3       sal r/m64, imm8 REX.W C1 /4 ib  00 100 111
: 2/        (   x -- x'    )    [ 49 . D1 . 3F . ] ;                \ [r15] >>= 1       sar r/m64, 1    REX.W D1 /7     00 111 111  TwoDiv
: NEGATE    (   n -- n'    )    [ 49 . F7 . 1F . ] ;                \ [r15] *= -1       neg r/m64       REX.W F7 /3     00 011 111
: INVERT    (   x -- x'    )    [ 49 . F7 . 17 . ] ;                \ [r15] ^= -1       not r/m64       REX.W F7 /2     00 010 111
: `         (   u --       )    B0 . .                              \ al = u            mov r8, imm8    B0+rb ib                    backtick
                                AA . ;                              \ [rdi++] = al      stos m8         AA
: syscall   (     --       )    [ 0F ` 05 ` ] ;                     \ syscall           syscall         0F 05
: +=        (   n --       )    [ 49 ` 83 ` C7 ` ] . ;              \ r15 += n          add r/m64, imm8 REX.W 83 /0 ib  11 000 111  PlusEquals
: ++=       (   n --       )    [ 48 ` 83 ` C4 ` ] . ;              \ rsp += n          add r/m64, imm8 REX.W 83 /0 ib  11 000 100  PlusPlusEquals
: &         (   u --       )    [ 41 . 8A . 07 .   8 += ]           \ al = [r15]        mov r8, r/m8    REX 8A /r       00 000 111  (adjust?)
                                [ 00 . 47 . FF .        ] ;         \ [rdi-1] += al     add r/m8, r8    00 /r           01 000 111
: UM/MOD ( ud u -- uRem uQuot ) [ 49 . 8B . 57 . 08 .   ]           \ rdx = [r15+8]     mov r64, r/m64  REX.W 8B /r     01 010 111
                                [ 49 . 8B . 47 . 10 .   ]           \ rax = [r15+10]    mov r64, r/m64  REX.W 8B /r     01 000 111
                                [ 49 . F7 . 37 .   8 += ]           \ rdx:rax / [r15]   div r/m64       REX.W F7 /6     00 110 111
                                [ 49 . 89 . 57 . 08 .   ]           \ [r15+8] = rdx     mov r/m64, r64  REX.W 89 /r     01 010 111  rdx=rem
                                [ 49 . 89 . 07 .        ] ;         \ [r15] = rax       mov r/m64, r64  REX.W 89 /r     00 000 111  rax=quot
: 8/mod ( u -- rem quot )       0 8 UM/MOD ;
: ^  ( n u -- ) 8/mod [ 49 ` ] 4* & [ 89 ` 47 ` ] 8* & . ;          \ [r15+n] = rux     mov r/m64, r64  REX.WuB 89 /r   01 uuu 111  up
: v  ( n u -- ) 8/mod [ 49 ` ] 4* & [ 8B ` 47 ` ] 8* & . ;          \ rux = [r15+n]     mov r64, r/m64  REX.WuB 8B /r   01 uuu 111  down
: ^^ ( n u -- ) 8/mod [ 48 ` ] 4* & [ 89 ` 44 ` ] 8* & [ 24 ` ] . ; \ [1*0+rsp+n] = rux mov r/m64, r64  REX.Wu 89 /r    01 uuu 100  00 100 100
: vv ( n u -- ) 8/mod [ 48 ` ] 4* & [ 8B ` 44 ` ] 8* & [ 24 ` ] . ; \ rux = [1*0+rsp+n] mov r64, r/m64  REX.Wu 8B /r    01 uuu 100  00 100 100
: push (   u -- )               [ 50 ` ] & ;                        \ push              push r64        50+rd
: pop  (   u -- )               [ 58 ` ] & ;                        \ pop               pop r64         58+rd
: TYPE ( addr u -- ) [ 7 push   8 6 v   0 2 v   10 += ] TYPE [ 7 pop ] ;

\ 0FFFFFE0 E00 TYPE BYE

: +     ( n1|u1 n2|u2 -- n|u )  [ 0 0 v   8 +=   49 . 01 . 07 . ] ; \ [r15] += rax      add r/m64, r64  REX.W 01 /r     00 000 111  Plus
: -     ( n1|u1 n2|u2 -- n|u )  [ 0 0 v   8 +=   49 . 29 . 07 . ] ; \ [r15] -= rax      sub r/m64, r64  REX.W 29 /r     00 000 111  Minus
: AND   ( x1 x2 -- x )          [ 0 0 v   8 +=   49 . 21 . 07 . ] ; \ [r15] &= rax      and r/m64, r64  REX.W 21 /r     00 000 111
: OR    ( x1 x2 -- x )          [ 0 0 v   8 +=   49 . 09 . 07 . ] ; \ [r15] |= rax      or r/m64, r64   REX.W 09 /r     00 000 111
: XOR   ( x1 x2 -- x )          [ 0 0 v   8 +=   49 . 31 . 07 . ] ; \ [r15] ^= rax      xor r/m64, r64  REX.W 31 /r     00 000 111
: LSHIFT ( x1 u -- x2 )         [ 0 1 v   8 +=   49 . D3 . 27 . ] ; \ [r15] <<= cl      shl r/m64, cl   REX.W D3 /4     00 100 111
: RSHIFT ( x1 u -- x2 )         [ 0 1 v   8 +=   49 . D3 . 2F . ] ; \ [r15] >>= cl      shr r/m64, cl   REX.W D3 /5     00 101 111
: from4  (    n -- n' )         [ 49 . 63 .      07 .   0 0 ^   ] ; \ rax = ..[r15]     movsx r64,r/m32 REX.W 63 /r     00 000 111
: from1  (    n -- n' )         [ 49 . 0F . BE . 07 .   0 0 ^   ] ; \ rax = ..[r15]     movsx r64, r/m8 REX.W 0F BE /r  00 000 111
: D+ ( d1 d2 -- d ) [ 8 0 v  0 1 v  10 +=   49 . 01 . 47 . 08 . ]   \ [r15+8] += rax    add r/m64, r64  REX.W 01 /r     01 000 111
                                [           49 . 11 . 0F .      ] ; \ [r15] += rcx+CF   adc r/m64, r64  REX.W 11 /r     00 001 111
: D- ( d1 d2 -- d ) [ 8 0 v  0 1 v  10 +=   49 . 29 . 47 . 08 . ]   \ [r15+8] -= rax    sub r/m64, r64  REX.W 29 /r     01 000 111
                                [           49 . 19 . 0F .      ] ; \ [r15] -= rcx+CF   sbb r/m64, r64  REX.W 19 /r     00 001 111
: D2/ ( xd -- xd' )          2/ [ 49 . D1 . 5F . 08 . ] ;  \ rot [r15+8] right thru CF  rcr r/m64, 1    REX.W D1 /3     01 011 111
: CHAR+     ( addr -- addr'  )  1+ ;
: CHARS     (    n -- n'     )  ;
: CELL+     ( addr -- addr'  )  8 + ;
: CELLS     (   n1 -- n2     )  8* ;
: ALIGN     (      --        )  ;                                   \ TODO revisit alignment when dealing with speed
: ALIGNED   ( addr -- a-addr )  ;
: ALLOT (    n -- )    [ 0 0 v   8 +=   48 . 01 . C7 .          ] ; \ rdi += rax        add r/m64, r64  REX.W 01 /r     11 000 111
: ,   (      x -- )    [ 0 0 v   8 +=   48 . AB .               ] ; \ [rdi(++8)] = rax  stos m64        REX.W AB                    Comma
: 4,  (      x -- )    [ 0 0 v   8 +=   AB .                    ] ; \ [rdi(++4)] = eax  stos m32        AB                          FourComma
: C,  (   char -- )    [ 0 0 v   8 +=   AA .                    ] ; \ [rdi++] = al      stos m8         AA                          CComma
: !   ( x addr -- )    [ 8 0 v   0 1 v   10 +=   48 . 89 . 01 . ] ; \ [rcx] = rax       mov r/m64, r64  REX.W 89 /r     00 000 001  Store
: 4!  ( x addr -- )    [ 8 0 v   0 1 v   10 +=   89 . 01 .      ] ; \ [rcx] = eax       mov r/m32, r32  89 /r           00 000 001  FourStore
: 4@  (   addr -- x )  [ 0 0 v        8B . 00 .   0 0 ^         ] ; \ eax = [rax]       mov r32, r/m32  8B /r           00 000 000  FourFetch
: @   (   addr -- x )  [ 0 0 v   48 . 8B . 00 .   0 0 ^         ] ; \ rax = [rax]       mov r64, r/m64  REX.W 8B /r     00 000 000  Fetch
: C!  ( char addr -- ) [ 8 0 v   0 1 v   10 +=   40 . 88 . 01 . ] ; \ [rcx] = al        mov r/m8, r8    REX 88 /r       00 000 001  CStore
: C@  ( addr -- char ) [ 0 0 v   48 . 0F . B6 . 00 .   0 0 ^    ] ; \ rax = 0..[rax]    movzx r64, r/m8 REX.W 0F B6 /r  00 000 000  CFetch
: 2!  ( x y addr --  ) [ 10 0 v  8 1 v   0 2 v   48 . 89 . 0A . ]   \ [rdx] = rcx       mov r/m64, r64  REX.W 89 /r     00 001 010  TwoStore
                       [ 18 +=   48 . 89 . 42 . 08 .            ] ; \ [rdx+8] = rax     mov r/m64, r64  REX.W 89 /r     01 000 010      mem: y x
: 2@  (  addr -- x y ) [ 0 2 v   F8 +=   48 . 8B . 42 . 08 .    ]   \ rax = [rdx+8]     mov r64, r/m64  REX.W 8B /r     01 000 010  TwoFetch
                       [ 48 . 8B . 0A .   8 0 ^   0 1 ^         ] ; \ rcx = [rdx]       mov r64, r/m64  REX.W 8B /r     00 001 010      mem: y x
: +!  ( n|u addr --  ) [ 8 0 v   0 1 v   10 +=   48 . 01 . 01 . ] ; \ [rcx] += rax      add r/m64, r64  REX.W 01 /r     00 000 001
: -!  ( n|u addr --  ) [ 8 0 v   0 1 v   10 +=   48 . 29 . 01 . ] ; \ [rcx] -= rax      sub r/m64, r64  REX.W 29 /r     00 000 001
: sgn (          --  ) [ 48 ` C1 ` FA ` 3F `                    ] ; \ rdx = 0,-1 if <0  sar r/m64, imm8 REX.W C1 /7 ib  11 111 010
: S>D (       n -- d ) [ 0 2 v   sgn   F8 +=   0 2 ^            ] ; 
: ABS (       n -- u ) [ 0 2 v   sgn   49 . 31 . 17 .           ]   \ [r15] ^= rdx      xor r/m64, r64  REX.W 31 /r     00 010 111
                       [               49 . 29 . 17 .           ] ; \ [r15] -= rdx      sub r/m64, r64  REX.W 29 /r     00 010 111
: MAX (   n n' -- n" ) [ 0 0 v   8 1 v   8 +=   48 . 3B . C1 .  ]   \ cmp rax, rcx      cmp r64, r/m64  REX.W 3B /r     11 000 001
                       [ 48 . 0F . 4C . C1 .   0 0 ^            ] ; \ rax = rcx if <    cmovl r64,r/m64 REX.W 0F 4C /r  11 000 001
: MIN (   n n' -- n" ) [ 0 0 v   8 1 v   8 +=   48 . 3B . C1 .  ]   \ cmp rax, rcx      cmp r64, r/m64  REX.W 3B /r     11 000 001
                       [ 48 . 0F . 4F . C1 .   0 0 ^            ] ; \ rax = rcx if >    cmovg r64,r/m64 REX.W 0F 4F /r  11 000 001
: * ( n n' -- n" ) [ 0 0 v   8 +=   49 . 0F . AF . 07 .   0 0 ^ ] ; \ rax *= [r15]      imul r64, r/m64 REX.W 0F AF /r  00 000 111  (un)signed
: PICK ( xu .. x0 u -- xu .. x0 xu )
                 [ 0 0 v   49 . 8B . 44 . C7 . 08 .   0 0 ^ ] ;   \ rax = [8*rax+r15+8] mov r64, r/m64  REX.W 8B /r     01 000 100  11 000 111
: COMPILE,  (   xt --        )  [  0 3 v    8 +=  ] COMPL ;
: EXECUTE ( i*x xt -- j*x    )  [  0 3 v    8 +=  ] EXEC ;
: SP@       (      -- addr   )  [ F8 F ^   F8 +=  ] ;               \ See https://forth-standard.org/standard/exception/CATCH
: SP!       ( addr --        )  [  8 +=    F8 F v ] ;               \ See https://forth-standard.org/standard/exception/CATCH
: RP@       (      -- addr   )  [ F8 +=     0 4 ^ ] ;               \ See https://forth-standard.org/standard/exception/CATCH
: RP!       ( addr --        )  [  0 4 v    8 +=  ] ;               \ See https://forth-standard.org/standard/exception/CATCH
: HERE      (      -- addr   )  [ F8 +=     0 7 ^ ] ;
: #IN       (      -- addr   )  1 1C LSHIFT ;
: TIB       (      -- addr   )  #IN CELL+ ;
: >IN       (      -- addr   )  #IN 10 + ;
: source_id (      -- addr   )  #IN 18 + ;
: SOURCE-ID (      -- 0|-1   )  source_id @ ;
: STATE     (      -- addr   )  #IN 20 + ;
: latest    (      -- addr   )  #IN 28 + ;
: SOURCE    (      -- addr u )  #IN 2@ ;
: UNUSED    (      -- u      )  1 1F LSHIFT HERE - ;
: DROP  (       x --             ) [                                      8 +=                                   ] ;
: 2DROP (     x y --             ) [                                     10 +=                                   ] ;
: DUP   (       x -- x x         ) [                    0 0 v            F8 +=    0 0 ^                          ] ;
: 2DUP  (     x y -- x y x y     ) [           8 0 v    0 1 v            F0 +=    8 0 ^   0 1 ^                  ] ;
: OVER  (     x y -- x y x       ) [           8 0 v                     F8 +=    0 0 ^                          ] ;
: 2OVER ( x y z w -- x y z w x y ) [          18 0 v   10 1 v            F0 +=    8 0 ^   0 1 ^                  ] ;
: SWAP  (     x y -- y x         ) [           8 0 v    0 1 v                     8 1 ^   0 0 ^                  ] ;
: 2SWAP ( x y z w -- z w x y     ) [ 18 0 v    8 2 v    8 0 ^   18 2 ^           10 1 v   0 2 v   10 2 ^   0 1 ^ ] ;
: NIP   (     x y -- y           ) [                            0 1 v     8 +=    0 1 ^                          ] ;
: TUCK  (     x y -- y x y       ) [  8 0 v    0 1 v                     F8 +=   10 1 ^   8 0 ^    0 1 ^         ] ;
: ROT   (   x y z -- y z x       ) [ 10 0 v    8 1 v    0 2 v                    10 1 ^   8 2 ^    0 0 ^         ] ; \ TODO local variables
: -ROT  (   x y z -- z x y       ) [ 10 0 v    8 1 v    0 2 v                    10 2 ^   8 0 ^    0 1 ^         ] ;
: 2ROT ( x y z w a b -- z w a b x y )   [ 28 0 v   20 1 v   18 2 v   10 3 v   8 5 v   0 6 v ]
                                        [ 28 2 ^   20 3 ^   18 5 ^   10 6 ^   8 0 ^   0 1 ^ ] ;
: IMMEDIATE (     --     )              latest @ 10 + DUP C@ 80 OR SWAP C! ;
: S>        (   n --     )              [ 41 ` FF ` 77 ` ] . ;      \ push [r15+n]      push r/m64      REX FF /6       01 110 111  cf. vv ^^
: >S        (   n --     )              [ 41 ` 8F ` 47 ` ] . ;      \ pop  [r15+n]      pop  r/m64      REX 8F /0       01 000 111
: >R        (   x --     ) ( R:      -- x       )   0  S>   8 +=                            ; IMMEDIATE                 \ toR
: R>        (     -- x   ) ( R:    x --         )   F8 +=   0 >S                            ; IMMEDIATE                 \ Rfrom
: R@        (     -- x   ) ( R:    x -- x       )   F8 +=   0 >S   0 S>                     ; IMMEDIATE                 \ RFetch
: 2>R       ( x y --     ) ( R:      -- x y     )   8  S>   0 S>   10 +=                    ; IMMEDIATE                 \ TwotoR
: 2R>       (     -- x y ) ( R:  x y --         )   F0 +=   0 >S   8 >S                     ; IMMEDIATE                 \ TwoRfrom
: 2R@       (     -- x y ) ( R:  x y -- x y     )   F0 +=   8 0 vv   0 1 vv   8 0 ^   0 1 ^ ; IMMEDIATE                 \ TwoRFetch
: EXIT      (  -- ) ( R: nestsys ret -- nestsys )   [ 0 pop ] ;
: COUNT                       ( addr -- addr' u )   >R R@ 1+ R> C@ ;
: name                        (   xt -- addr u  )   10 + COUNT 1F AND ;
: stack)                      (      -- u       )   #IN ;
: DEPTH                       (      -- +n      )   SP@ stack) SWAP - 3 RSHIFT ;
: EMIT                        (    x --         )   SP@ 1 TYPE DROP ;
: D>S                         (    d -- n       )   DROP ;
: DNEGATE                     (    d -- d'      )   0 0 2SWAP D- ;
: D2*                         (   xd -- xd'     )   2DUP D+ ;
: 2+!                    ( d|ud addr --         )   >R R@ 2@       D+ R> 2! ;
: 2-!                    ( d|ud addr --         )   >R R@ 2@ 2SWAP D- R> 2! ;
: PARSE-NAME ( "<spaces>name<space>" -- addr u  )   pname [ F0 +=   8 5 ^   0 0 ^ ] ;
: PARSE      (      char "ccc<char>" -- addr u  )   DUP 1+ [ 8 1 v   0 2 v ] PARSE [ 8 5 ^   0 0 ^ ] ;
: '          (         "<spaces>ccc" -- xt      )   pname FIND [ F8 +=   0 3 ^ ] ;                                      \ Tick
: CHAR       (        "<spaces>name" -- char    )   PARSE-NAME DROP C@ ;
: (forget)   (                    xt --         )   [ 0 7 v ] CELL+ @ latest ! ;
: FORGET     (        "<spaces>name" --         )   ' (forget) ;

\ In Forth 2012, the stack notation ( C: ... ) refers to the control-flow stack. However, Forth 2012 allows
\ the control-flow stack to be the data stack, and these control-flow effects occur only during compilation.
\ We want a brief notation for compilation effects. In SmithForth ( C: ... ) denotes compilation effects.

: RECURSE   ( C: -- )                               latest @ COMPILE, ; IMMEDIATE
: :NONAME   ( C: -- colonsys ) ( -- xt )
    15      \ ASCII negative acknowledge, an arbitrary whitespace for a dummy name
    SP@     \ rsi = name_addr
    1       \ al  = name_length
    latest  \ rdx = Latest
    [ 10 6 v   8 0 v   0 2 v   20 += ] Head latest @ 10 + >R R@ @ 40 ( HIDDEN ) OR R> ! ] latest @ ;

\ Comparisons \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

: flag^ ( u -- )        [ 0F ` ] . [ C1 ` ]             \ cl = 0,1 if cc    setcc r/m8      0F uu /?        11 000 001
                        [ F6 ` D9 ` ]                   \ cl *= -1          neg r/m8        F6 /3           11 011 001
                        [ 48 ` 0F ` BE ` C9 ` ] 0 1 ^ ; \ rcx = ..cl        movsx r64, r/m8 REX.W 0F BE /r  11 001 001
: ?1    ( u -- )        [ 49 ` 83 ` 3F ` 00 ` ] flag^ ; \ cmp [r15], 0      cmp r/m64, imm8 REX.W 83 /7 ib  00 111 111  ( eflags )
: ?2    ( u -- ) 0 1 v  8 += [ 49 ` 39 ` 0F ` ] flag^ ; \ cmp [r15], rcx    cmp r/m64, r64  REX.W 39 /r     00 001 111  ( eflags )
: 0=     (      x -- flag ) [ 94 ?1 ] ;                                     \ ZeroEqual
: 0<     (      n -- flag ) [ 9C ?1 ] ;                                     \ Zeroless
: 0<=    (      n -- flag ) [ 9E ?1 ] ;                                     \ 
: 0>     (      n -- flag ) [ 9F ?1 ] ;                                     \ Zeromore
: 0>=    (      n -- flag ) [ 9D ?1 ] ;                                     \ 
: 0<>    (      x -- flag ) [ 95 ?1 ] ;                                     \ Zerone
: =      (   x x' -- flag ) [ 94 ?2 ] ;                                     \ 
: <      (   n n' -- flag ) [ 9C ?2 ] ;                                     \ less
: <=     (   n n' -- flag ) [ 9E ?2 ] ;                                     \ 
: >      (   n n' -- flag ) [ 9F ?2 ] ;                                     \ more
: >=     (   n n' -- flag ) [ 9D ?2 ] ;                                     \ 
: <>     (   x x' -- flag ) [ 95 ?2 ] ;                                     \ ne
: U<     (   u u' -- flag ) [ 92 ?2 ] ;                                     \ Uless
: U<=    (   u u' -- flag ) [ 96 ?2 ] ;                                     \ 
: U>     (   u u' -- flag ) [ 97 ?2 ] ;                                     \ Umore
: U>=    (   u u' -- flag ) [ 93 ?2 ] ;                                     \ 
: D0=    (     xd -- flag ) OR 0= ;                                         \ DZeroEqual
: D0<    (      d -- flag ) NIP 0< ;                                        \ DZeroless
: D=     ( xd xd' -- flag ) ROT = -ROT = AND ;                              \ DEqual
: D<     (  d  d' -- flag ) ROT >R >R  < 2R@ = AND 2R>  < OR ;              \ Dless
: DU<    ( ud ud' -- flag ) ROT >R >R U< 2R@ = AND 2R> U< OR ;              \ DUless
: WITHIN ( n|u n'|u' n"|u" -- flag ) OVER - >R - R> U< ; \ as in https://forth-standard.org/standard/core/WITHIN

\ Conditionals and loops \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
\ 
\     : W ... test IF ... THEN ... ;
\     : W ... test IF ... ELSE ... THEN ... ;
\     : W ... BEGIN ... AGAIN ... ;
\     : W ... BEGIN ... test UNTIL ... ;
\     : W ... BEGIN ... test WHILE ... REPEAT ... ;
\ 
\ In Forth 2012, "orig" denotes a forward reference, and "dest" denotes a backward reference. In SmithForth,
\     dest is an address to jump to, and
\     orig is an address to jump from, so a reference is resolved by: [orig-4] = len = HERE - orig
\ Jump length is measured from the end of the jump instruction.

: zero   ( -- )     8 += [ 49 ` 83 ` 7F ` F8 ` 00 ` ]   \ cmp [r15-8], 0    cmp r/m64, imm8 REX.W 83 /7 ib  01 111 111
                    [ 0F ` 84 ` ] ;                     \ jump if 0         jz rel32        0F 84 cd
: ever   ( -- )     [ E9 ` ] ;                          \ jump              jmp rel32       E9 cd
: jump   (              -- orig      )          0 4, HERE ;
: land   (         orig --           )          >R HERE R@ - R> 4 - 4! ;
: back   (    dest orig --           )          >R HERE    - R> 4 - 4! ;
: AHEAD  ( C:           -- orig      ) (   -- ) ever jump           ; IMMEDIATE
: IF     ( C:           -- orig      ) ( x -- ) zero jump           ; IMMEDIATE
: THEN   ( C:      orig --           ) (   -- ) land                ; IMMEDIATE
: ELSE   ( C:      orig -- orig'     ) (   -- ) ever jump SWAP land ; IMMEDIATE
: BEGIN  ( C:           -- dest      ) (   -- ) HERE                ; IMMEDIATE
: AGAIN  ( C:      dest --           ) (   -- ) ever jump back      ; IMMEDIATE
: UNTIL  ( C:      dest --           ) ( x -- ) zero jump back      ; IMMEDIATE
: WHILE  ( C:      dest -- orig dest ) ( x -- ) zero jump SWAP      ; IMMEDIATE
: REPEAT ( C: orig dest --           ) (   -- ) ever jump back land ; IMMEDIATE

\ Strings etc. \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

: imm ( xt -- flag )                            10 + C@ 80 AND ;
: fnd ( addr u -- addr u 0 | xt 1 | xt -1 )     [ 8 5 v   0 0 v ] FIND [ F8 +=   0 3 ^ ] DUP IF NIP NIP DUP imm IF 1 ELSE FF THEN THEN ;
: FIND  ( addr -- addr 0 | xt 1 | xt -1 )       DUP COUNT fnd DUP IF ROT DROP ELSE NIP NIP THEN ;
: [COMPILE] (   "<spaces>ccc" --  )             ' COMPILE, ; IMMEDIATE \ see AntonErtl @ https://forth-standard.org/standard/core/BracketCOMPILE
: LITERAL  ( C: x -- ) ( -- x )                 DUP  7 RSHIFT IF F8 +=     [ B8 ` ]  4,  0 0 ^ ELSE [COMPILE] LIT     THEN ; IMMEDIATE
: LITERAL  ( C: x -- ) ( -- x )                 DUP 20 RSHIFT IF F8 +=  [ 48 ` B8 ` ] ,  0 0 ^ ELSE [COMPILE] LITERAL THEN ; IMMEDIATE
: 2LITERAL ( C: x x' -- ) ( -- x x' )           SWAP [COMPILE] LITERAL [COMPILE] LITERAL ; IMMEDIATE
: [CHAR]   ( C: "<spaces>name" -- ) ( -- char ) CHAR [COMPILE] LITERAL ; IMMEDIATE   \ BracketChar
: [']      ( C: "<spaces>name" -- ) ( -- xt   ) ' [COMPILE] LITERAL ; IMMEDIATE      \ BracketTick
: POSTPONE (     "<spaces>ccc" -- )             ' DUP imm IF COMPILE, ELSE [COMPILE] LITERAL   ['] COMPILE, COMPILE, THEN ; IMMEDIATE 
: (cmov    ( -- )                               7 push   10 6 v   8 7 v   0 1 v   18 += ;
: cmov)    ( -- )                               [ E3 ` 02 ` ] ( jrcxz rel8 ) [ F3 ` A4 ` ] ( rep movs )   7 pop ;
: CMOVE  ( addr1 addr2 u -- ) [ (cmov cmov) ] ; \ copy 1 to 2, bytes lo to hi
: CMOVE> ( addr1 addr2 u -- ) [ (cmov ]         \ copy 1 to 2, bytes hi to lo
    [ 48 . 8D . 74 . 31 . FF . ]                \ rsi = 1*rsi+rcx-1 lea r64, m      REX.W 8D /r     01 110 100  00 110 001
    [ 48 . 8D . 7C . 39 . FF . ]                \ rdi = 1*rdi+rcx-1 lea r64, m      REX.W 8D /r     01 111 100  00 111 001
    [ FD .      cmov)     FC . ] ;              \ FD : std(reverse) ; FC : cld(forward)
: MOVE   ( addr1 addr2 u -- )                   2 PICK 2 PICK < IF CMOVE> ELSE CMOVE THEN ; \ [addr2, addr2+u) = [addr1, addr1+u)
: s,  ( addr u -- )                             >R HERE R@ ALLOT R> MOVE ;                  \ compile a string
: C"  ( C: "ccc<quote>" -- ) ( -- addr   )      ever jump [CHAR] " PARSE HERE >R DUP C, s, land  R> POSTPONE  LITERAL ; IMMEDIATE
: S"  ( C: "ccc<quote>" -- ) ( -- addr u )      POSTPONE C"   ['] COUNT COMPILE, ; IMMEDIATE
: ."  ( C: "ccc<quote>" -- ) ( -- )             POSTPONE S"   ['] TYPE  COMPILE, ; IMMEDIATE
: .(    ( "ccc<rparen>" -- )                    [CHAR] ) PARSE TYPE ; IMMEDIATE
: ROLL   ( xu ... x0 u -- xu-1 ... x0 xu )      >R R@ PICK SP@ DUP CELL+ R> 1+ CELLS CMOVE> DROP ;
: ?DUP   ( x -- 0 | x x )                       DUP IF DUP THEN ;
: FILL   ( addr u char -- )                     OVER IF 2 PICK C! ( addr u ) 1- OVER 1+ ( addr u-1 addr+1 ) SWAP CMOVE ELSE DROP DROP DROP THEN ;
: ERASE  ( addr u --  )                         0 FILL ;
: DABS   (   d  -- ud )                         DUP 0< IF DNEGATE THEN ;
: DMAX   ( d d' -- d" )                         2OVER 2OVER D< IF      2SWAP THEN 2DROP ;
: DMIN   ( d d' -- d" )                         2OVER 2OVER D< IF ELSE 2SWAP THEN 2DROP ;
: aligned8  ( addr -- addr' )                   7 + 7 INVERT AND ;
: aligned10 ( addr -- addr' )                   F + F INVERT AND ;
: align8    (      --       )                   [ F8 +=   0 7 ^ ] aligned8  [ 0 7 v   8 += ] ;
: align10   (      --       )                   [ F8 +=   0 7 ^ ] aligned10 [ 0 7 v   8 += ] ;
: >BODY     (   xt -- addr  )                   10 + DUP C@ 1F AND 1+ 1+ + aligned8 ; \ 1+ for flag byte, 1+ for return ( C3 )
: (create)  ( -- addr ) ( R: ret -- ret )       R@ 4 - 4@ >BODY ;
: CREATE    ( "<spaces>name" -- )               :   ['] (create) @ latest @ !   POSTPONE ;   align8 ;

\ DOES> \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

\     https://forth-standard.org/standard/core/DOES (see WEIRD especially)
\     https://www.bradrodriguez.com/papers/moving3.htm
\     http://win32forth.sourceforge.net/doc/Forth_Primer.pdf
\     http://galileo.phys.virginia.edu/classes/551.jvn.fall01/primer.htm#create
\     https://archive.org/details/brown_j_w_welcome_forth/page/n25/mode/2up
\     https://archive.org/details/R.G.LoeligerThreadedInterpretiveLanguagesTheirDesignAndImplementationByteBooks1981/page/n83/
\     https://cfhcable.dl.sourceforge.net/project/thinking-forth/reprint/rel-1.0/thinking-forth-color.pdf
\     http://www.forth.org/svfig/Len/arrays.htm
\ 
\     Compilation ( C: colonsys -- colonsys' )
\         append run-time semantics to the current definition   ( CONSTANT )       ( 1: WEIRD )               ( 2: WEIRD.DOES>.1 )
\         consume colonsys ( CONSTANT ) and produce colonsys'   ( CONSTANT.DOES> ) ( 1: WEIRD WEIRD.DOES>.1 ) ( 2: WEIRD.DOES>.1 WEIRD.DOES>.2 )
\         append initiation semantics to the current definition ( CONSTANT.DOES> ) ( 1: WEIRD.DOES>.1 )       ( 2: WEIRD.DOES>.2 )
\     Runtime ( -- ) ( R: nestsys -- )
\         replace execution semantics of most recent definition Name ( FOUR ) by Name execution semantics
\         return control to the calling definition ( CONSTANT ) specified by nestsys
\     Initiation ( i*x -- i*x addr ) ( R: -- nestsys' ) note, i*x are arguments to Name ( FOUR )
\         save implementation-dependent information nestsys' referring to calling definition
\         place onto stack data field address of Name ( FOUR )
\     Name execution: ( i*x -- j*x ) note, i*x and j*x are arguments and results of Name ( FOUR )
\         execute the portion of the definition that begins with the initiation semantics appended by the DOES> which modified Name ( FOUR )

: (run) ( --      ) ( R:    r -- r    )     R@ 1+ ( addr ) latest @ ! ; \ addr: begin code of CONSTANT.DOES> ; extra byte for C3 ; latest: FOUR
: (ini) ( -- addr ) ( R: r r' -- r r' )     2R@ DROP 4 - 4@ >BODY ;     \ addr: data area of FOUR ; r: caller of FOUR ; r': CONSTANT.DOES>
: DOES> ( C: colonsys -- colonsys' )        ['] (run) COMPILE,   [ C3 ` ]   ['] (ini) COMPILE, ; IMMEDIATE

\ Variables etc. \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

: MARKER    (     "<spaces>name" -- )       CREATE latest @ , DOES> @ (forget) ;
: VARIABLE  (     "<spaces>name" -- )       CREATE 0 , ;
: 2VARIABLE (     "<spaces>name" -- )       CREATE 0 , 0 , ;
: BUFFER:   (   u "<spaces>name" -- )       CREATE ALLOT ;
VARIABLE BASE
: DECIMAL   (                    -- )       A  BASE ! ;
: HEX       (                    -- )       10 BASE ! ; HEX
: CONSTANT  (   x "<spaces>name" -- )       CREATE ,   DOES>  @ ;
: 2CONSTANT ( x y "<spaces>name" -- )       CREATE , , DOES> 2@ ;
: VALUE     (   x "<spaces>name" -- )       CREATE 0 ( id ) latest @ >BODY 1- C! ,   DOES>  @ ;
: 2VALUE    ( x y "<spaces>name" -- )       CREATE 1 ( id ) latest @ >BODY 1- C! , , DOES> 2@ ;
: (to)      (             i*x xt -- )       >BODY DUP 1- C@ IF 2! ELSE ! THEN ;
: TO        ( i*x "<spaces>name" -- )       STATE @ IF   ' POSTPONE LITERAL   ['] (to) COMPILE,
            ( C:  "<spaces>name" -- )       ELSE         ' (to)               THEN ; IMMEDIATE
: (+to)     (             i*x xt -- )       >BODY DUP 1- C@ IF 2+! ELSE +! THEN ;
: +TO       ( i*x "<spaces>name" -- )       STATE @ IF   ' POSTPONE LITERAL   ['] (+to) COMPILE,
            ( C:  "<spaces>name" -- )       ELSE         ' (+to)              THEN ; IMMEDIATE
: (-to)     (             i*x xt -- )       >BODY DUP 1- C@ IF 2-! ELSE -! THEN ;
: -TO       ( i*x "<spaces>name" -- )       STATE @ IF   ' POSTPONE LITERAL   ['] (-to) COMPILE,
            ( C:  "<spaces>name" -- )       ELSE         ' (-to)              THEN ; IMMEDIATE
: uChar     (    u -- char )                DUP A < IF [CHAR] 0 + ELSE A - [CHAR] A + THEN ; \ 0..F.. -> '0'..'9', 'A'..'F'..
: charu     ( char -- u    )                DUP [CHAR] 0 [CHAR] 9 1+ WITHIN IF [CHAR] 0 -     ELSE DROP FF THEN ;
: charu     ( char -- u    )                DUP [CHAR] A [CHAR] Z 1+ WITHIN IF [CHAR] A - A + ELSE charu   THEN ;
: charu     ( char -- u    )                DUP [CHAR] a [CHAR] z 1+ WITHIN IF [CHAR] a - A + ELSE charu   THEN ;
: signum    (    n -- n'   )                >R R@ 0< R> 0> - ;
: rstackBytes  (   -- nHard nSoft nRet )    \ nRet : 0(success) or -1(error)
                                            61 \ getrlimit (/usr/include/x86_64-linux-gnu/asm/unistd_64.h)
                                            3  \ RLIMIT_STACK (/usr/include/x86_64-linux-gnu/bits/resource.h) 
                                            SP@ [ 10 0 v   8 7 v   0 6 v   syscall   0 0 ^ ] ;
\ I guess Linux syscalls, in the layout of structs, conform to the System V ABI (general) chapter 7 on network protocol, as "network" (or "RPC")
\ here might simply refer to the situation where two programs attempt to communicate without having been compiled by the same tools and options.
\     http://www.sco.com/developers/devspecs/gabi41.pdf
: rstackCells  ( -- n )                     rstackBytes DROP NIP 3 RSHIFT ; \ getrlimit(RLIMIT_STACK) values vary only if we change them?
VARIABLE s0   VARIABLE u0   VARIABLE match  \ lexicographic order: 0 if str1 == str2
VARIABLE s1   VARIABLE u1                   \ -1 if str1 < str2, +1 if str1 > str2
: known            (        -- 0 | n -1 )   u0 @ u1 @ * IF s0 @ C@ s1 @ C@ - signum ?DUP ELSE u0 @ u1 @ - signum FF THEN ;
: COMPARE ( addr u addr' u' -- n    )       u1 ! s1 ! u0 ! s0 ! BEGIN known 0= WHILE 1 s0 +! 1 u0 -! 1 s1 +! 1 u1 -! REPEAT ;
: streq   ( addr u addr' u' -- flag )       COMPARE 0= DUP IF DUP match ! THEN ;
: max-n            (        -- n    )       1 3F LSHIFT INVERT ;
: bufLen           (        -- u    )       40 4* ;        
: ENVIRONMENT?     ( addr u -- 0 | i*x -1 ) 2>R 0 match ! \ Values must be constant.
                                            S" /COUNTED-STRING"     2R@ streq IF 40 4* 1-    ( n )   THEN
                                            S" /HOLD"               2R@ streq IF bufLen      ( n )   THEN
                                            S" /PAD"                2R@ streq IF bufLen      ( n )   THEN
                                            S" ADDRESS-UNIT-BITS"   2R@ streq IF 8           ( n )   THEN
                                            S" FLOORED"             2R@ streq IF FF         ( flag ) THEN
                                            S" MAX-CHAR"            2R@ streq IF 40 4* 1-    ( u )   THEN
                                            S" MAX-D"               2R@ streq IF FF max-n    ( d )   THEN
                                            S" MAX-N"               2R@ streq IF max-n       ( n )   THEN
                                            S" MAX-U"               2R@ streq IF FF          ( u )   THEN
                                            S" MAX-UD"              2R@ streq IF FF FF       ( ud )  THEN
                                            S" RETURN-STACK-CELLS"  2R@ streq IF rstackCells ( n )   THEN
                                            S" STACK-CELLS"         2R@ streq IF 1 15 LSHIFT ( n )   THEN
                                            2R> 2DROP match @ ;
0       CONSTANT FALSE
0 1-    CONSTANT TRUE
20      CONSTANT BL
: SPACE ( -- )                              BL EMIT ;
: CR    ( -- )                              A  EMIT ;
: WORDS ( -- )                              latest @ BEGIN DUP WHILE SPACE DUP name TYPE CELL+ @ REPEAT DROP CR ;


\ Arithmetic again \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
\ 
\ See Knuth TAOCP Vol 2 Sec 4.3.1. Our approach is to convert negative numbers to nonnegative for multiplication and
\ division. Multiplication and division on 2- and 3-cell numbers is like on 2- and 3-digit numbers. We use all 2^6 bits
\ of each cell and unsigned basic operations. We have only 1-cell divisors, so we can use the "short division" algorithm.
\ I found AMD's manual clearer than Intel's on the meaning of flags CF (carry, unsigned ops) and OF (overflow, signed ops).

: M+ ( d|ud n -- d'|ud' ) S>D D+ ;
: M*  ( n n' -- d  ) [ 8 0 v   49 . F7 . 2F .   8 0 ^    0 2 ^ ] ;  \ rdx:rax=rax*[r15]   imul r/m64      REX.W F7 /5     00 101 111  signed
: UM* ( u u' -- ud ) [ 8 0 v   49 . F7 . 27 .   8 0 ^    0 2 ^ ] ;  \ rdx:rax=rax*[r15]   mul r/m64       REX.W F7 /4     00 100 111  unsigned
: udm* ( ud u -- t ) TUCK 2>R UM* 0 2R> UM* D+ ; \ t: 3-cell prod   \                       ____q_p
\ udm*              H L                                             \      m*/            n ) H M L
\                 x   u                                             \                         n*q
\                   ---                                             \                           s L
\     L*u:          M L'                                            \                           n*p
\     H*u:       H' N                                               \                             r
: m*/ ( ud u +n -- d  r  ) >R udm* R@ UM/MOD -ROT R> UM/MOD -ROT ;
VARIABLE -ve
: sfloor ( ud r -- d ) >R -ve @ IF DNEGATE THEN R> 0<> -ve @ AND S>D D+ ; \ roughly, if quot < 0 and rem != 0, then quot--
: M*/ ( d n +n -- d' ) >R DUP 0< -ve ! ABS >R 2DUP D0< -ve @ XOR -ve ! DABS R> R> m*/ sfloor ;

\ Symmetric division (provided by x86) note, idiv fails if quotient doesn't fit in rax \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

: imul   ( -- )              [ 49 ` F7 ` 6F ` 08 ` ] ;  \ rdx:rax=rax*[r15+8] imul r/m64    REX.W F7 /5     01 101 111
: idiv   ( -- )              [ 49 ` F7 ` 3F ` ] ;       \ rdx:rax / [r15]     idiv r/m64    REX.W F7 /7     00 111 111  rdx=rem rax=quot
: SM/REM (   d  m -- rem quot ) [ 10 0 v   8 2 v       idiv    8 +=   8 2 ^   0 0 ^ ] ;     \   signed d ( lo hi ) / m
: /'     (   n  m -- quot     ) [  8 0 v   8 2 v   sgn idiv    8 +=   0 0 ^         ] ;     \ Divprime
: MOD'   (   n  m -- rem      ) [  8 0 v   8 2 v   sgn idiv    8 +=   0 2 ^         ] ;     \ MODprime
: /MOD'  (   n  m -- rem quot ) [  8 0 v   8 2 v   sgn idiv           8 2 ^   0 0 ^ ] ;     \ DivMODprime
: */'    ( n n' m -- quot     ) [ 10 0 v          imul idiv   10 +=   0 0 ^         ] ;     \ TimesDivprime
: */MOD' ( n n' m -- rem quot ) [ 10 0 v          imul idiv    8 +=   8 2 ^   0 0 ^ ] ;     \ TimesDivMODprime

\ Floored division: (customary in mathematics, 0 <= remainder < m when 0 < m as in modular arithmetic, a little slower)

\ The difference between symmetric (SM/REM) and floored (FM/MOD) division is illustrated in Table 3.3 of Forth 2012
\ (https://forth-standard.org/standard/usage). I prefer floored as in the division algorithm, a.k.a. the division lemma.

: adjust (      rem m -- flag       )   >R R@ 0< IF NEGATE 0 R> NEGATE ELSE 0 R> THEN WITHIN 0= ;
: adjust ( rem quot m -- rem' quot' )   >R OVER R@ adjust >R ( rem quot R: m flag ) R@ + SWAP 2R> AND + SWAP ;
: FM/MOD (        d m -- rem quot   )   >R R@ SM/REM R> adjust ;
: /MOD   (        n m -- rem quot   )   >R S>D R> FM/MOD ;
: /      (        n m -- quot       )   /MOD NIP ;
: MOD    (        n m -- rem        )   /MOD DROP ;
: */MOD  (     n n' m -- rem quot   )   >R R@ */MOD' R> adjust ;
: */     (     n n' m -- quot       )   */MOD NIP ;

\ Numeric output \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

bufLen BUFFER: nbuf         VARIABLE [nbuf
: HOLD  ( char --        )  1 [nbuf -!   [nbuf @ C! ;
: HOLDS ( addr u --      )  BEGIN DUP WHILE 1- 2DUP + C@ HOLD REPEAT 2DROP ; \ as in https://forth-standard.org/standard/core/HOLDS
: SIGN  (    n --        )  0< IF [CHAR] - HOLD THEN ;
: +SIGN (    n --        )  0< IF [CHAR] - ELSE [CHAR] + THEN HOLD ;
: <#    (      --        )  nbuf bufLen +   [nbuf ! ;
: #     (   ud -- ud'    )  1 BASE @ m*/ uChar HOLD ;
: #S    (   ud -- ud'    )  BEGIN # 2DUP D0= UNTIL ;
: #>    (   xd -- addr u )  2DROP [nbuf @   nbuf bufLen +   [nbuf @   - ;
: d.    (    d -- addr u )  TUCK DABS <# #S ROT  SIGN #> ;
: +d.   (    d -- addr u )  TUCK DABS <# #S ROT +SIGN #> ;
: D.    (    d --        )  d. TYPE SPACE ;
: c.    ( char -- addr u )  0 <# # # #> ;
: C.    ( char --        )  c. TYPE ;
: u8.   (    u -- addr u )  0 <# # # # # # # # # #> ;
: U8.   (    u --        )  u8. TYPE ;
: u.    (    u -- addr u )  0 <# #S #> ;
: U.    (    u --        )  u. TYPE SPACE ;
: n.    (    n -- addr u )  S>D d. ;
: +n.   (    n -- addr u )  S>D +d. ;
: N.    (    n --        )  n. TYPE SPACE ;
: ?     ( addr --        )  @ N. ; 
: .S    (      --        )  [CHAR] ( EMIT DEPTH n. TYPE [CHAR] ) EMIT SPACE
                            DEPTH 0 BEGIN 2DUP > WHILE 1+ stack) OVER CELLS - ? REPEAT 2DROP CR ;

\ Reports (nonstandard) \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

: #T 60 ;                   #T BUFFER: T
: Twrite ( addr u vddr -- ) 2>R 2R@ @ T + SWAP MOVE 2R> +! ;    \ suppose buffer T has room for u chars
: Twrite ( addr u vddr -- ) >R #T R@ @ #T MIN - MIN R> Twrite ; \ update variable vddr which keeps a value from 0 to #T

\ SEE \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

: nextLater ( xt -- xt'  )  >R latest @ BEGIN DUP CELL+ @ R@ - WHILE CELL+ @ REPEAT R> DROP ;
: endCode   ( xt -- addr )  DUP latest @ = IF DROP HERE ELSE nextLater THEN ;
: endCode   ( xt -- addr )  DUP IF endCode THEN ;
: begCode   ( xt -- addr )  DUP IF @ THEN ;
: begCode   ( xt -- addr )  DUP IF 10 + DUP C@ 1F AND 1+ + THEN ;

\ TODO extend input facility ( TIB >IN #IN etc. ) to use other input source ( machine code )? Similar issues arise in S\" , binary interpreter.
0 VALUE [&                  \ Note, whenever these are used, assume 1 CHARS is equal to 1. Otherwise there are irritations.
0 VALUE &>                  \ For instance, to tell how many chars are between two addresses, must divide.
0 VALUE &)
VARIABLE err
VARIABLE fin

CREATE >T 3 CELLS ALLOT
: col     (      n -- vddr    ) CELLS >T + ;
: h       ( addr u --         ) 2 col Twrite ;
: ul      (      u --         ) 3 * S" al cl dl bl ah ch dh bh"                                         DROP + 2 h ;
: eux     (      u --         ) 4* S" eax ecx edx ebx esp ebp esi edi"                                  DROP + 3 h ; 
: rux     (      u --         ) 4* S" rax rcx rdx rbx rsp rbp rsi rdi r8  r9  r10 r11 r12 r13 r14 r15 " DROP + 3 h ;
: |       (      n -- char    ) 1 8 LSHIFT MOD ; \ sign extension of literals is inconvenient for byte arithmetic below
: |>      (        --         ) err @ fin @ OR IF ELSE 1 fin !   [& u8. 0 col Twrite   &> TO [&   T #T TYPE CR THEN
                                T #T BL FILL   0 0 col !   B 1 col !   #T 2/ 2 col !   [& TO &>   0 err ! ;
: ~       (        --         ) ."     " &> C@ C. err @ IF ."  E  " ELSE ."     " THEN .S ; \ to debug see
: byte    (        -- u       ) &> C@ DUP c. 1 col Twrite   1 1 col +!   1 +TO &> ;
: 4byte   (        -- u       ) byte byte byte byte 8 LSHIFT + 8 LSHIFT + 8 LSHIFT + ;
: must    (   flag --         ) IF ELSE 1 err ! THEN ;
: [0,8)   (      u -- u'      ) DUP 0 8 WITHIN must 8 MOD ;
: -REX    (      u -- B X R W ) 2 /MOD 2 /MOD 2 /MOD 2 /MOD 4 = must ;
: -REX.wx (      u -- B R     ) -REX 0=  must SWAP 0= must ;
: -REX.Wx (      u -- B R     ) -REX 1 = must SWAP 0= must ;
: -00r/m  (  B R u -- r/m reg ) 8 /MOD 8 /MOD 0 = must ROT 8* + >R SWAP 8* + R> ;
: -01r/m  (  B R u -- r/m reg ) 8 /MOD 8 /MOD 1 = must ROT 8* + >R SWAP 8* + R> ;
: -11r/m  (  B R u -- r/m reg ) 8 /MOD 8 /MOD 3 = must ROT 8* + >R SWAP 8* + R> ; \ TODO -ModR/M ( B R u -- r/m reg mod ) and allow any mod
: see     (        --         ) 1 err !   0 fin !
    |>              byte C3 | = must   S" return" h
    |>              byte 8 /MOD 50 | 8 / = must S" push " h rux
    |>              byte 8 /MOD 58 | 8 / = must S" pop "  h rux
    |>              byte 8B | = must   0 0 byte -00r/m [0,8) eux S"  = [" h [0,8) rux S" ]" h
    |>              byte 2B | = must   0 0 byte -11r/m [0,8) eux S"  -= " h [0,8) eux
    |>              byte -REX.Wx   byte 83 | = must   byte -11r/m 5 = must rux S"  -= " h byte c. h
    |>              byte 83 | = must   0 0 byte -11r/m 5 = must  [0,8) eux S"  -= " h   byte c. h
    |>              byte AA | = must   S" [rdi++] = al" h
    |>              byte AB | = must   S" [rdi(++4)] = eax" h
    |>              byte AC | = must   S" al = [rsi++]" h
    |>              byte AD | = must   S" eax = [rsi(++4)]" h
    |>              byte 8 /MOD 90 | 8 / = must S" xchg eax, " h eux
    |>              byte 8 /MOD B0 | 8 / = must ul  S"  = " h   byte c. h
    |>              byte 8 /MOD B8 | 8 / = must eux S"  = " h   4byte u. h
    |>              byte 75 | = must   S" jump if != to " h   byte from1 +n. h
    |>              byte EB | = must   S" jump to " h   byte from1 +n. h
    |>              byte 6A | = must   S" push " h   byte c. h
    |> byte -REX.wx byte 8F | = must   byte -00r/m 0= must S" pop [" h rux S" ]" h
    |> byte -REX.wx byte FF | = must   byte -01r/m 6 = must S" push [" h rux S" +" h   byte c. h   S" ]" h
    |> byte -REX.Wx byte 83 | = must   byte -01r/m 7 = must S" cmp [" h rux S" +" h   byte c. h   S" ], " h   byte c. h
    |>              byte 0F | = must   byte 05 | = must   S" syscall" h
    |>              byte 0F | = must   byte 84 | = must   S" jump if 0 to " h   4byte from4 +n. h
    |>              byte E8 | = must   S" call " h    4byte &> + from4 +n. h \ TODO call goes to code, not to xt ; get name by begCode endCode ?
    |>              byte E9 | = must   S" jump to " h   4byte from4 +n. h
    |> byte -REX.Wx byte 83 | = must   byte -11r/m 0= must rux S"  += " h   byte c. h
    |>              byte FF | = must   byte 14 | = must   byte 25 | = must S" call " h   4byte err @ IF DROP ELSE name h THEN
    |> byte -REX.Wx byte 8B | = must   byte -01r/m rux S"  = [" h rux S" +" h   byte c. h S" ]" h \ TODO incorrect on output of vv ; where are other use cases? v is one ( search 8B ) ; detect SIB condition
    |> byte -REX.Wx byte 89 | = must   byte -01r/m S" [" h   SWAP rux   S" +" h   byte c. h   S" ] = " h rux
    |>              byte S" (unknown) " h   20 7F WITHIN IF &> 1- 1 h THEN
    |> ;
: SEE ( "<spaces>name" -- ) ' DUP begCode TO [&   DUP endCode TO &)   DUP U8. ."  Code: " DUP @ U8.
    ."  Link: " DUP CELL+ @ U8. ."  ( " DUP CELL+ @ ?DUP IF name TYPE THEN ."  ) Flag: " DUP 10 + C@ C. ."  Name: " name TYPE CR 
    BEGIN [& &) ( ." PASS " .S ) < WHILE see REPEAT ;

\ Counted loops (simple) \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
\ 
\     : W ... limit index DO ... LOOP ... ;
\ 
\ L=loop-sys ( R: limit index )

: DO ( limit index -- ) ( R: -- L ) ( C: -- dest )  POSTPONE 2>R   HERE ; IMMEDIATE
: (loop)    ( -- flag ) ( R: L ret -- L' ret )      [ 2 pop ] R> 1+ >R 2R@ = [ 2 push ] ;
: LOOP      ( --   ) ( R: L -- |L' ) ( C: dest -- ) ['] (loop) COMPILE,   zero jump back   10 ++= ; IMMEDIATE
: I         ( -- n ) ( R: L ret -- L ret )          [ 8 0 vv   F8 +=   0 0 ^ ] ;

\ DUMP \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

: lim_ind ( lo -- lo limit index ) DUP F0 AND DUP 10 + SWAP ;
: within ( hi lo n -- hi lo flag ) 1 PICK 3 PICK WITHIN ;
: DHex ( hi lo -- hi lo ) lim_ind DO I within IF I C@ c. TYPE ELSE SPACE SPACE THEN   I 1 AND IF SPACE THEN LOOP ;
: DRaw ( hi lo -- hi lo ) lim_ind DO I within IF I C@ 20 7F WITHIN IF I C@ ELSE [CHAR] . THEN EMIT ELSE SPACE THEN LOOP ;
: DLine ( hi lo -- ) DUP F0 AND u8. TYPE [CHAR] : EMIT SPACE DHex SPACE DRaw 2DROP CR ;
: DUMP ( addr u -- ) OVER + SWAP BEGIN 2DUP DLine 10 + F0 AND 2DUP <= UNTIL 2DROP ;

\ 0FFFFFE0 120 DUMP BYE

\ CASE \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
\ 
\     : W ...
\         x CASE
\             x1 OF ... ENDOF
\             x2 OF ... ENDOF
\             ( default ) ... ENDCASE ... ;
\ 
\ In SmithForth,
\     casesys (1 cell) is a forward reference (orig) emanating from the previous ENDOF of this case statement, or 0 if none, and
\     ofsys   (1 cell) is a forward reference (orig) emanating from a statement OF, used to jump past the next ENDOF.

: (of)    ( x y -- -1 | x 0 )                            OVER = DUP IF NIP THEN ;
: CASE    (     --    ) ( C:               -- casesys  ) 0 ; IMMEDIATE
: OF      ( x y -- |x ) ( C:               -- ofsys    ) ['] (of) COMPILE,   zero jump ; IMMEDIATE
: ENDOF   (     --    ) ( C: casesys ofsys -- casesys' ) ever jump   SWAP land   TUCK 4 - 4! ; IMMEDIATE
: ENDCASE (   x --    ) ( C:       casesys --          ) ['] DROP COMPILE,   BEGIN DUP WHILE DUP 4 - 4@ SWAP land REPEAT DROP ; IMMEDIATE

\ Strings with escape sequences \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

: getc ( -- 0 | char -1 ) >IN @ #IN @ < DUP IF TIB @ >IN @ + C@ 1 >IN +! SWAP THEN ;
: getq ( -- 0 | char -1 ) getc DUP IF OVER [CHAR] " = IF 2DROP 0 THEN THEN ; \ 0 if double quote
: \c   ( -- ) getc IF CASE
    [CHAR] a OF [  7 `     ] ENDOF
    [CHAR] b OF [  8 `     ] ENDOF
    [CHAR] e OF [ 1B `     ] ENDOF
    [CHAR] f OF [  C `     ] ENDOF
    [CHAR] l OF [  A `     ] ENDOF
    [CHAR] m OF [  D ` A ` ] ENDOF
    [CHAR] n OF [  A `     ] ENDOF
    [CHAR] q OF [ 22 `     ] ENDOF
    [CHAR] r OF [  D `     ] ENDOF
    [CHAR] t OF [  9 `     ] ENDOF
    [CHAR] v OF [  B `     ] ENDOF
    [CHAR] " OF [ 22 `     ] ENDOF
    [CHAR] \ OF [ 5C `     ] ENDOF
    [CHAR] x OF 0 getc IF charu SWAP 4* 4* + THEN getc IF charu SWAP 4* 4* + THEN C, ENDOF
    [ 0 ` ] ENDCASE THEN ;
: S\" ( C: "ccc<quote>" -- ) ( -- addr u ) ever jump BEGIN getq WHILE DUP [CHAR] \ = IF DROP \c ELSE C, THEN REPEAT
    ( orig ) HERE OVER - ( orig u ) OVER land POSTPONE 2LITERAL ; IMMEDIATE
\ TODO Make strings more space-efficient with a better scheme than 2LITERAL. Use only a single 1-byte parameter, length.
\ Address can be retrieved from the return stack. Down from twenty-two bytes to perhaps nine?

\ Counted loops (complete) \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
\ 
\     : W ... limit index  DO ...       LOOP ... ;
\     : W ... limit index  DO ... step +LOOP ... ;
\     : W ... limit index ?DO ...       LOOP ... ;
\     : W ... limit index ?DO ... step +LOOP ... ;
\ 
\ L=loop-sys ( R: addr limit k ) where
\     addr is the location of the DO header
\     k = min-n + index - limit
\     min-n = 8000000000000000
\ The reason to translate the index by min-n - limit is to detect more easily when to exit a DO ... +LOOP.
\ See AntonErtl's comment (https://forth-standard.org/standard/core/PlusLOOP#reply-214).
\ Gist: jump back if k + n does not overflow. There are two cases n|u (signed and unsigned).
\ Each instance of a DO loop gets a DO header, consisting of a 64-bit address where LEAVE statements jump to.
\ Next instruction: call UNLOOP. Right before the DO header is an instruction to jump past the header.
\ See also Dr. C. H. Ting's account of F83 (http://forth.org/OffeteStore/1003_InsideF83.pdf),
\ especially "The New F83 Loops" (p. 48) and "The New Leave" (p. 50).

: min-n ( -- n ) max-n 1+ ;
: (do) ( limit index addr -- ) ( R: ret -- addr limit k ret ) -ROT min-n + OVER -
    [ 10 0 v   8 1 v   0 2 v   18 +=   3 pop   E8 ++=   10 0 ^^   8 1 ^^   0 2 ^^   3 push ] ;
: (DO) ( -- ) ( R: -- L ) ( C: -- addr dest ) EB . 08 . \ skip DO header jmp rel8       EB cb
    HERE ( addr )   HERE ( for POSTPONE LITERAL )   0 ,   POSTPONE LITERAL   ['] (do) COMPILE,   HERE ( dest ) ;
: (q) ( x y -- -1 | x y 0 ) 2DUP = DUP IF NIP NIP THEN INVERT ;
: DO  ( n1|u1 n2|u2 -- ) ( R: -- L ) ( C: -- orig addr dest )                            0 (DO) ; IMMEDIATE
: ?DO ( n1|u1 n2|u2 -- ) ( R: -- L ) ( C: -- orig addr dest ) ['] (q) COMPILE,   zero jump (DO) ; IMMEDIATE \ qDO
: UNLOOP ( -- ) ( R: L ret -- ret ) [ 0 pop   18 ++=   0 push ] ;
: (+loop) ( n -- flag ) ( R: addr limit k ret -- addr limit k' ret )
    [ 0 0 v   48 . 01 . 44 . 24 . 08 . ]    \ [1*0+rsp+8] += rax    add r/m64, r64      REX.W 01 /r         01 000 100  00 100 100
    [ 0F . 90 . C0 .                   ]    \ al = 0, 1 if overflow seto r/m8           0F 90 /?            11 000 000
    [ 48 . 0F . B6 . C0 .        0 0 ^ ] ;  \ rax = 0..0al          movzx r64, r/m8     REX.W 0F B6 /r      11 000 000
: +LOOP ( n -- ) ( R: L -- |L' ) ( C: orig addr dest -- )
    ['] (+loop) COMPILE,   zero jump back   HERE SWAP !   ['] UNLOOP COMPILE,   ?DUP IF land THEN ; IMMEDIATE
: LOOP ( -- ) ( R: L -- |L' ) ( C: orig addr dest -- ) 1 POSTPONE LITERAL   POSTPONE +LOOP ; IMMEDIATE
: I ( -- n|u ) ( R:        L ret -- L        ret )  [  8 0 vv   10 1 vv   F0 +=   0 0 ^   8 1 ^ ] + min-n - ;
: J ( -- n|u ) ( R:    L1 L2 ret -- L1 L2    ret )  [ 20 0 vv   28 1 vv   F0 +=   0 0 ^   8 1 ^ ] + min-n - ;
: K ( -- n|u ) ( R: L1 L2 L3 ret -- L1 L2 L3 ret )  [ 38 0 vv   40 1 vv   F0 +=   0 0 ^   8 1 ^ ] + min-n - ;
: LEAVE ( -- ) ( R: L ret -- L )
    [ 0 pop   10 0 vv   FF . 20 . ] ;       \ jump [rax]            jmp r/m64           FF /4               00 100 000

\ Buffers \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

bufLen BUFFER: PAD
bufLen BUFFER: wbuf
bufLen BUFFER: kbuf
bufLen BUFFER: bbuf

\ Terminal input mode noncanonical \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
\ 
\     The Forth core standard word KEY must not echo the character entered.
\     The usual behavior in a Unix terminal is to echo characters entered.
\     Unix (POSIX) provides a non-echoing terminal mode called noncanonical mode.
\     The terminal mode may be changed by helper programs like `stty`, `stdbuf`, or `unbuffer`.
\     I find it easier to understand what is going on if instead we change the mode from within our own program by syscalls.
\     My approach is to leave and enter canonical mode upon each call of KEY.
\     ACCEPT uses canonical mode and enjoys the standard line-editing features.
\ 
\     Choice of mode is controlled by item ICANON in member c_lflag of struct termios.
\     This is a libc thing, but is also exposed in syscall ioctl ( `man 2 ioctl_tty` ). Useful info:
\         https://www.gnu.org/software/libc/manual/html_node/Terminal-Modes.html
\         https://www.gnu.org/software/libc/manual/html_node/Noncanon-Example.html
\         https://www.gnu.org/software/libc/manual/html_node/Canonical-or-Not.html
\         https://www.gnu.org/software/libc/manual/html_node/Noncanonical-Input.html
\         https://www.gnu.org/software/libc/manual/html_node/Local-Modes.html ( see ECHO and other flags )
\         In these headers and in POSIX, local mode flags are in a 4-byte field after three other 4-byte fields of the struct.
\             /usr/include/x86_64-linux-gnu/bits/termios.h 
\             /usr/include/asm-generic/termbits.h
\     libc has a function isatty() to determine whether stdin is a terminal. We might want to do something like this. `man 2 fstat`

5401 CONSTANT TCGETS \ see /usr/include/asm-generic/ioctls.h
5402 CONSTANT TCSETS \ see /usr/include/asm-generic/ioctls.h
2    CONSTANT ICANON \ see /usr/include/x86_64-linux-gnu/bits/termios.h
8    CONSTANT ECHO   \ see /usr/include/x86_64-linux-gnu/bits/termios.h
24   BUFFER:  tcbuf  \ #bytes written by syscall. I cannot find a definition of termios in Linux that agrees, but it is consistent with POSIX.
: c_lflag ( -- addr ) tcbuf C + ; 
: ioctl? ( cmd -- flag ) 10 0 tcbuf ( cmd ioctl stdin argp )
    [ 7 push   18 6 v   10 0 v   8 7 v   0 2 v   syscall   18 +=   0 0 ^   7 pop ] ;
: ioctl ( cmd -- ) ioctl? DROP ; \ ignore any error
\ #define ENOTTY      25  /* Not a typewriter */
\ ENOTTY fd is not associated with a character special device.
\ ENOTTY The specified request does not apply to the kind of object that the file descriptor fd references.
\ Apparently even isatty() (`man 3 isatty`) just tries syscall TCGETS ioctl; is TTY iff it succeeds (ret=0). 
\     https://stackoverflow.com/questions/41906713/is-there-a-way-to-determine-if-stdin-is-a-tty-through-a-system-call
TCGETS ioctl? VALUE notty
: -echo ( -- ) 0 TO notty ; \ useful in case `cat myprog.fs - | ./SForth` 
: +echo ( -- ) 1 TO notty ;
: tty ( -- flag ) notty 0= ;
: nonca ( -- ) tty IF TCGETS ioctl   c_lflag 4@   ICANON INVERT AND   ECHO INVERT AND   c_lflag 4!   TCSETS ioctl THEN ;
: canon ( -- ) tty IF TCGETS ioctl   c_lflag 4@   ICANON OR           ECHO OR           c_lflag 4!   TCSETS ioctl THEN ;

\ Input etc. \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

: WORD ( char "<chars>ccc<char>" -- addr )  DUP BL = IF DROP PARSE-NAME ELSE [ 0 2 v ] 1+ [ 0 1 v ] seek 1- PARSE THEN
                                            ( addr u ) [ FF ] LITERAL AND   DUP wbuf C!   wbuf 1+ SWAP MOVE   wbuf ;
: more    (    addr u -- flag           )   IF C@ charu 0 BASE @ WITHIN ELSE DROP 0 THEN ;
: >NUMBER ( ud addr u -- ud' addr+1 u-1 )   1- >R DUP 1+ >R C@ charu >R BASE @ 1 M*/ R> 0 D+ R> R> ;
: >NUMBER ( ud addr u -- ud' addr'  u'  )   BEGIN 2DUP more WHILE >NUMBER REPEAT ;
: SPACES  (         u --                )   0 ?DO SPACE LOOP ;
: .r      ( addr u n  -- u              )   OVER - 0 MAX SPACES TYPE ;
: .R      (      n n' --                )   >R n. R> .r ;
: U.R     (      u n  --                )   >R u. R> .r ;
: D.R     (      d n  --                )   >R d. R> .r ;
: read    (   addr u  -- u'             )   0 ( read=stdin ) [ 7 push   10 6 v   8 2 v   0 0 v   0 7 v   syscall   10 +=   0 0 ^   7 pop ] ;
: KEY     (           -- char           )   nonca   0 SP@ 1 read DROP   canon ;
VARIABLE b>IN
VARIABLE b#IN
: brefill (           --                )   0 b>IN !   bbuf bufLen read 0 MAX b#IN ! ;
: bchar   (           -- 0 | addr 1     )   b>IN @ b#IN @ >= IF brefill THEN
                                            b>IN @ b#IN @ >= IF 0 ELSE bbuf b>IN @ +   notty IF DUP C@ EMIT THEN   1 b>IN +!   1 THEN ;
: ACCEPT  (   addr +n -- +n'            )   OVER + TO &)   TO [&   [& TO &>   BEGIN &> &) < WHILE
                                                bchar IF C@ DUP &> !   1 +TO &>    A ( LF ) = IF &> TO &) THEN
                                                ELSE &> TO &) THEN REPEAT &> [& - ;
\ : SAVE-INPUT ( -- xn ... x1 n         )   >IN @ 1 ;
\ : RESTORE-INPUT ( xn ... x1 n -- flag )   0 DO I IF DROP ELSE >IN ! >IN @ 0 #IN @ 1+ WITHIN THEN LOOP ;
\ : RESTORE-INPUT ( xn ... x1 n -- flag )   DUP 0<= IF 0 MAX ELSE RESTORE-INPUT THEN ;
: SAVE-INPUT ( -- xn ... x1 n           )   0 ;                       \ presently we have no text file input ...
: RESTORE-INPUT ( xn ... x1 n -- flag   )   0 ?DO DROP LOOP TRUE ;    \ ... nor do we save lines of standard input

\ Number conversion: Recognize 1-cell numbers, 2-cell numbers (ending with '.'), and non-numbers ("0-cell"). Parameter u is #cells.

: >num   ( -- ud u ) 0 0 [& &) OVER - >NUMBER NIP ( ud u ) IF 0 ELSE 2 THEN ;   \ string bare, nonempty ; u = 0 or 2 only
: >num   ( -- ud u ) [& &) < IF >num ELSE 0 0 0 THEN ;                          \ string bare ; u = 0 or 2 only
: dotted ( -- flag ) &) 1- C@ [CHAR] . = DUP IF 1 -TO &) THEN ;                 \ string nonempty
: dotted ( -- flag ) [& &) < IF dotted ELSE 0 THEN ;
: >num   ( -- ud u ) dotted 0= >R >num R> IF 2/ THEN ;
: neg    ( -- flag ) [& &) < IF [& C@ CASE
    [CHAR] + OF 1 +TO [&   0             ENDOF
    [CHAR] - OF 1 +TO [&   FF            ENDOF
    0 SWAP ENDCASE ELSE 0 THEN ; 
: >num   ( -- d u ) neg >num ( +/- ud u ) >R ROT IF DNEGATE THEN R> ;
: base!  ( -- ) [& &) < IF [& C@ CASE
    [CHAR] # OF 1 +TO [&   DECIMAL       ENDOF
    [CHAR] $ OF 1 +TO [&   HEX           ENDOF
    [CHAR] % OF 1 +TO [&   2 BASE !      ENDOF ENDCASE THEN ;
: >num   ( -- d u ) BASE @ >R base! >num R> BASE ! ;
: cnum   ( -- flag ) &) [& - 3 = DUP IF [& C@ [CHAR] ' = AND [& 1+ 1+ C@ [CHAR] ' = AND THEN ;
: >num   ( addr u -- d u ) OVER + TO &) TO [& cnum IF [& 1+ C@ 0 1 ELSE >num THEN ;
\ TODO regex w/ capture groups ( addr u ) on the stack ? Outer pair of parens to determine match or not ?

\ Forth interpreter \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

: tib    ( -- ) ." tib: #IN: " #IN @ N. ." ; >IN: " >IN @ N. ." ; '" TIB @ #IN @ TYPE ." '" CR ; \ show the TIB ( for debugging )
: trynum ( addr u -- n | d | ) 2DUP >num CASE
    2 OF 0 err ! 2SWAP 2DROP     STATE @ IF POSTPONE 2LITERAL THEN ENDOF
    1 OF 0 err ! 2SWAP 2DROP D>S STATE @ IF POSTPONE  LITERAL THEN ENDOF
    0 OF 1 err ! 2DROP ." Error: no word or number " TYPE CR ENDOF ENDCASE ;
: INTERPRET ( -- ) BEGIN >IN @ #IN @ < WHILE PARSE-NAME
    DUP IF fnd STATE @ 0= IF ABS THEN CASE
    FF OF COMPILE, ENDOF
    1  OF EXECUTE  ENDOF
    0  OF trynum err @ IF #IN @ >IN ! THEN ENDOF
    ENDCASE ELSE 2DROP THEN REPEAT ;
: EVALUATE  ( i*x addr u -- j*x )
    #IN @ >R    TIB @ >R       >IN @ >R     source_id @ >R
    #IN !       TIB !        0 >IN !     FF source_id !     INTERPRET R> R> R> R>
    #IN !       TIB !          >IN !        source_id ! ;
: REFILL ( -- flag ) 0 >IN !   kbuf TIB !   kbuf bufLen ACCEPT   DUP #IN !   0<> ;
RP@      CONSTANT RP0
10000000 CONSTANT SP0
: QUIT   ( -- ) ( R: i*x -- ) 0 source_id !   RP0 [ 0 4 v   8 += ]
    POSTPONE [ BEGIN REFILL WHILE INTERPRET STATE @ 0= IF ."  ok" THEN CR REPEAT BYE ;
: ABORT ( i*x -- ) ( R: j*x -- ) SP0 [ 0 F v ] QUIT ;
: (abort) ( x addr u -- ) ROT IF TYPE ABORT ELSE 2DROP THEN ;
: ABORT" ( C: "ccc<quote>" -- ) ( i*x x -- | i*x ) ( R: j*x -- | j*x ) POSTPONE S"   ['] (abort) COMPILE, ; IMMEDIATE
: DEFER     ( "<spaces>name" --     )                       CREATE ['] ABORT , DOES> ( i*x -- j*x ) @ EXECUTE ;
: DEFER@    (             xt -- xt' )                       >BODY @ ;        \ DEFER DEFER@ DEFER! ACTION-OF IS are implemented ...
: DEFER!    (         xt' xt --     )                       >BODY ! ;        \ ... as in https://forth-standard.org/standard/core/
: ACTION-OF ( Compilation: "<spaces>name" -- ) ( -- xt )    STATE @ IF POSTPONE ['] POSTPONE DEFER@
            ( Interpretation:     "<spaces>name" -- xt )        ELSE ' DEFER@ THEN ; IMMEDIATE
: IS        ( Compilation: "<spaces>name" -- ) ( xt -- )    STATE @ IF POSTPONE ['] POSTPONE DEFER! 
            ( Interpretation:     xt "<spaces>name" -- )        ELSE ' DEFER! THEN ; IMMEDIATE   
: [DEFINED]   ( C: "<spaces>name" -- flag ) BL WORD FIND NIP 0<> ; IMMEDIATE \ as in https://forth-standard.org/standard/tools/BracketDEFINED
: [UNDEFINED] ( C: "<spaces>name" -- flag ) BL WORD FIND NIP 0=  ; IMMEDIATE \ as in https://forth-standard.org/standard/tools/BracketUNDEFINED
: [ELSE] ( -- ) \ used in Forth 2012 test harness                            \ as in https://forth-standard.org/standard/tools/BracketELSE
    1 BEGIN	                                    \ level
        BEGIN BL WORD COUNT DUP WHILE	            \ level adr len
            2DUP S" [IF]" COMPARE 0= IF	            \ level adr len
                2DROP 1+                            \ level'
            ELSE	                            \ level adr len
                2DUP S" [ELSE]" COMPARE 0= IF	    \ level adr len
                    2DROP 1- DUP IF 1+ THEN	    \ level'
                ELSE	                            \ level adr len
                    S" [THEN]" COMPARE 0= IF	    \ level
                        1-	                    \ level'
                    THEN
                THEN
            THEN ?DUP 0= IF EXIT THEN	            \ level'
        REPEAT 2DROP	                            \ level
    REFILL 0= UNTIL	                            \ level
    DROP
; IMMEDIATE
: [IF]   ( flag -- ) 0= IF POSTPONE [ELSE] THEN ; IMMEDIATE                  \ as in https://forth-standard.org/standard/tools/BracketIF
: [THEN] (      -- ) ; IMMEDIATE                                             \ as in https://forth-standard.org/standard/tools/BracketTHEN
: .      (    n -- ) N. ;   \ the usual Forth dot
QUIT                        \ start the Forth interpreter
