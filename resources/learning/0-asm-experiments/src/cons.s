	.globl _start

	.equ CONS_CAR_OFFSET, 0
	.equ CONS_CDR_OFFSET, 8
	
	.section .data
first:	.quad 1, second
second:	.quad 2, third
third:	.quad 3, 0

	.section .text
_start:	leaq first, %rbx

iter:	cmp $0, %rbx
	je done

	movq CONS_CAR_OFFSET(%rbx), %rdi
	movq CONS_CDR_OFFSET(%rbx), %rbx

	jmp iter

done:	movq $60, %rax
	syscall
