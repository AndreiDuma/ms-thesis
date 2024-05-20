	.globl _start
	.section .text
_start:
	movq $3, %rdi
	movq %rdi, %rax
	addq %rdi, %rax
	mulq %rdi		# 18 in %rax
	movq $2, %rdi
	addq %rdi, %rax		# 20 in %rax
	movq $4, %rdi
	mulq %rdi		# 80 in %rax
	movq %rax, %rdi

	movq $60, %rax
	syscall			# exit
