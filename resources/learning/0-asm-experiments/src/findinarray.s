	.globl _start

	.section .data
numbers_len:
	.quad 5
numbers:
	.quad 12, 18, 41, 25, 30
find:
	.quad 25

	.section .text
_start:
	movq find, %rdx
	movq numbers_len, %rcx	# loop counter
	movq $-1, %rdi

	cmp $0, %rcx
	je done

iter:
	movq numbers-8(,%rcx,8), %rax

	## If current > max, save it
	cmp %rax, %rdx
	jne iter_next

	## Save index
	leaq -1(%rcx), %rdi
iter_next:
	loopq iter

done:
	movq $60, %rax
	syscall
