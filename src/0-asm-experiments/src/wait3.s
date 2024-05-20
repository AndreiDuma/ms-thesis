	.globl _start

	.section .data
curtime:
	## The time will be stored here.
	.quad 0

	.section .text
_start:
	## Initialize

	## Get the initial time
	movq $curtime, %rdi
	movq $0xc9, %rax
	syscall

	## Store it in %rdx
	movq curtime, %rdx

	## Add 3 seconds
	addq $3, %rdx

timeloop:
	## Check the time
	movq $curtime, %rdi
	movq $0xc9, %rax
	syscall

	## If I haven't reached the time stored in %rdx, do it again
	cmpq %rdx, curtime
	jb timeloop

finish:
	## Exit
	movq $0x3c, %rax
	movq $0, %rdi
	syscall
