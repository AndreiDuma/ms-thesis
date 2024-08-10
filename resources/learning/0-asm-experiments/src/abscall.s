	.globl _start
	.section .text
_start:
	movq $-5, %rdi
	call abs

	movq %rax, %rdi
	movq $60, %rax
	syscall
