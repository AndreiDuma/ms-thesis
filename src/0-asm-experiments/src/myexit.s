	## My first program. This is a comment.
	.section .text		# switch to the TEXT section
	.globl _start		# keep `_start` after assembling
_start:
	movq 	$60, %rax	# prepare `exit` syscall
	movq 	$3, %rdi
	syscall
