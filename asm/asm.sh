#!/bin/bash

repl() {
    while IFS= read -p '> ' -r line; do
	julia -e "include(\"instructions.jl\"); h=encode(parse(\"$line\")...; fmt=:hex); e=encode(parse(\"$line\")...; fmt=:emacs); print(\"\$e | \$h\")" | tee >(pbcopy)
	echo; echo
    done
}

export -f repl
rlwrap bash -c repl
