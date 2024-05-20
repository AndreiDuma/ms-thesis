	.section .text
	.globl _start
_start:
	movq $60, %rax

another_location:
	movq $8, %rdi

	jmp another_location

	## This never gets executed.
	syscall
