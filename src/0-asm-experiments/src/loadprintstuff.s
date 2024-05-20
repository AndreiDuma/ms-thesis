	.globl main

	.section .rodata
lib_path:
	.string "lib/libprintstuff.so\0"
lib_sym:
	.string "printstuff"
	
	.section .text
main:
	enter $0, $0

	leaq lib_path(%rip), %rdi
	movq $1, %rsi
	call dlopen

	movq %rax, %rdi
	leaq lib_sym(%rip), %rsi
	call dlsym

	call *%rax

	leave
	ret
