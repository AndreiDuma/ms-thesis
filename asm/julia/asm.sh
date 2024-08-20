#!/bin/bash

while IFS= read -r line; do
    julia -e "include(\"instructions.jl\"); encode(parse(\"$line\")...; fmt=:hex) |> print" | tee >(pbcopy)
    echo
    julia -e "include(\"instructions.jl\"); encode(parse(\"$line\")...; fmt=:emacs) |> print" | tee >(pbcopy)
    echo; echo
done
