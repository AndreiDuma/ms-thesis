	.global _start
	
	.section .data
source:
	.quad 9, 23, 55, 1, 3
dest:
	.quad 0, 0, 0, 0, 0

	.section .text
_start:
	movq $source, %rsi
	movq $dest, %rdi
	movq $3, %rcx
	rep movsq

	movq dest+2*8, %rdi
	movq $60, %rax
	syscall
