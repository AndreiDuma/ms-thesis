	.section .text
	.globl _start
_start:
	movq $7, %rdi
	jmp nextplace

	## These two instructions are skipped
	movq $8, %rbx
	addq %rbx, %rdi

nextplace:
	movq $60, %rax
	syscall			# exit
