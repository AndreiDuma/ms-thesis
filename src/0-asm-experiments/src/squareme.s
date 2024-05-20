	.globl squareme

	.section .text
squareme:
	movq %rdi, %rax
	imul %rax, %rax
	ret
