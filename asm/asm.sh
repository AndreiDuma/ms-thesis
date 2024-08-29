#!/bin/bash

repl() {
    while IFS= read -p '> ' -r line; do
	julia -e "include(\"instructions.jl\");  \
                  e = encode(parse(\"$line\")...; fmt=:emacs);  \
                  hb = encode(parse(\"$line\")...; fmt=:hex_bytes);  \
                  h = encode(parse(\"$line\")...; fmt=:hex);  \
                  print(\"\$e  |  \$hb  |  \$h\")" 2> /dev/null | tee >(pbcopy)
	echo; echo
    done
}

export -f repl
rlwrap bash -c repl
