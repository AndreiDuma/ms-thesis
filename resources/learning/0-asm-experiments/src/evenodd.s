	.section .text
	.globl _start
_start:
	movq $5, %rax
	movq $2, %rbx
	divq %rbx
	
	xorq $1, %rdx

exit:
	movq %rdx, %rdi
	movq $60, %rax
	syscall
