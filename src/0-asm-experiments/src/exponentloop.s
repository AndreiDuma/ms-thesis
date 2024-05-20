	.section .text
	.globl _start
_start:
	movq $2, %rbx
	movq $3, %rcx
	movq $1, %rax
	
	## If counter is already 0, we are done.
	cmpq $0, %rcx
	je complete

exp:
	mulq %rbx
	loopq exp

complete:
	movq %rax, %rdi
	movq $60, %rax
	syscall
