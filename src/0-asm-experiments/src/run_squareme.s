	.globl main

	.section .data
value:
	.quad 6
printf_fmt:
	.ascii "%d^2 = %d\n\0"

	.section .text
main:
	movq value, %rdi
	call squareme

	## print the result
	movq $printf_fmt, %rdi
	movq value, %rsi
	movq %rax, %rdx
	movq $0, %rax
	call printf

	movq $0, %rax
	ret
