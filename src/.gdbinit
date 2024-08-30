set pagination off

layout asm
layout regs


# Useful aliases
alias num    = x/d 0x10000000
alias tib    = x/x 0x10000008
alias into   = x/d 0x10000010
alias state  = x/d 0x10000020
alias latest = x/x 0x10000028
define globals
  printf "#IN\t%d\n", *0x10000000
  printf "TIB\t0x%.8x\n", *0x10000008
  printf ">IN\t%d\n", *0x10000010
  printf "STATE\t%d\n", *0x10000020
  printf "LATEST\t0x%.8x\n", *0x10000028
end


# Stop in binary interpreter just before jumping to `ti`.
break *0xEC
commands 1
  echo => bi (before `ti`) \n
end

# Advance to breakpoint above.
run


# Now define breakpoints for these Forth words (addresses may change).
break *0x100008e4
commands 2
  echo => ti \n
end

break *0x10000886
commands 3
  echo => SVAL \n
end

break *0x100003b6
commands 4
  echo => pname \n
end

break *0x100002f6
commands 5
  echo => seek \n
end

break *0x10000356
commands 6
  echo => PARSE \n
end

break *0x10000676
commands 7
  echo => FIND \n
  x/s $a0
end

break *0x10000614
commands 8
  printf "=> xt= addr=%.8x u=%d xt=%.8x\n", $a0, $a1, $a2
end

break *0x10000726
commands 9
  printf "=> miss   addr=... u=%d xt=%.8x\n", $a1, $a2
  x/s $a0
end

break *0x10000834
commands 10
  printf "=> hit   xt=%.8x\n", $a2
end

break *0x100007f6
commands 11
  printf "=> compl   xt=%.8x imm+state=0x%x\n", $a0, $a1
end

break *0x100007b6
commands 12
  printf "=> exec   xt=%.8x imm+state=0x%x\n", $a0, $a1
end

break *0x10000776
commands 13
  printf "=> EXEC   xt=%.8x\n", $a0
end

break *0x100004a2
commands 14
  printf "=> `:`\n"
end

break *0x10000462
commands 15
  printf "=> `(`\n"
end

break *0x100006d4
commands 16
  printf "=> Num addr=%.8x u=%d\n", $a0, $a1
end

break *0x100005c4
commands 17
  printf "=> LIT addr=%.8x u=%d\n", $a0, $a1
end  


# Disable breakpoints we don't currently need.
# disable 4
# disable 5
# disable 6
# disable 8

disable
enable 7
condition 7 *(char*)$a0=='X'
