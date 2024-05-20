	.globl _start

	.section .data
tic:	.ascii "tic!\n"
tic_end:
	.equ tic_len, tic_end - tic
tac:	.ascii "tac!\n"
tac_end:
	.equ tac_len, tac_end - tac

	.section .text
_start:
	## Setup loop
	movq $10, %rcx

loop:
	movq $tic, %rsi

	## On even iterations, tic!
	movq %rcx, %rax
	andq $1, %rax
	jz odd

	## On odd iterations, tac!
	movq $tac, %rsi
odd:
	## Save %rcx
	pushq %rcx
	
	## Do the actual printing
	movq $0x01, %rax
	movq $1, %rdi
	movq $5, %rdx
	syscall

	## Restore %rcx
	popq %rcx
	
	loopq loop

exit:
	movq $0x3c, %rax
	movq $0, %rdi
	syscall
