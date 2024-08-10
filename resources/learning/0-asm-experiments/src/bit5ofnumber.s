	.global _start

	.section .data
number:	.quad 127

	.section .text
_start:
	## result is initially 1
	movq $1, %rdi
	
	movq number, %rax
	andq $0b10000, %rax
	jnz print

	## bit was 0, so set result to 0
	xorq %rdi, %rdi
	
print:
	## print result
	movq $60, %rax
	syscall
