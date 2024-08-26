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

BYE
