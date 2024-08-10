	.globl _start

	.section .data
numbers_len:
	.quad 5
numbers:
	.quad 12, 18, 41, 25, 30

	.section .text
_start:
	movq numbers_len, %rcx	# loop counter
	movq $numbers, %rbx	# pointer to current number
	movq $0, %rdi		# initial maximum

	cmp $0, %rcx
	je done

iter:
	movq (%rbx), %rax

	## If current > max, save it
	cmp %rax, %rdi
	cmovbq %rax, %rdi

	## Advance pointer and loop
	addq $8, %rbx
	loopq iter

done:
	movq $60, %rax
	syscall
