	.globl _start

	.section .data
string:	.ascii "ana are mere\0"

	.section .text
_start:
	## search for \0 in `string`; first, prepare scas
	movq $string, %rdi
	movq $0, %rax
	cld
	## second, prepare repne
	movq $-1, %rcx
	repne scasb

	## compute length
	movq $-1, %rdi
	subq %rcx, %rdi
	decq %rdi

	## print
	movq $60, %rax
	syscall
	
