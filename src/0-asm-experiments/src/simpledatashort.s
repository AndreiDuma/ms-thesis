	.globl _start

	.section .data
first:	.quad 4
second:	.quad 6

	.section .text
_start:	movq first, %rdi
	addq second, %rdi

	movq $60, %rax
	syscall
