	.section .text
	.globl _start
_start:
	movq $0, %rax
	movq $3, %rbx
	movq $4, %rcx

	cmp $0, %rcx
	jz exit

multiply:
	addq %rbx, %rax
	loopq multiply

exit:
	movq %rax, %rdi
	movq $60, %rax
	syscall
