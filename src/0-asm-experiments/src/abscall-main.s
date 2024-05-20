	.globl main
	.section .text
main:
	movq $-5, %rdi
	call abs
	ret
