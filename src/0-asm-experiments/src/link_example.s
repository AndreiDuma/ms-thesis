	.globl main

	.section .data
output:
	.ascii "hello\n\0"

	.section .text
main:
	enter $0, $0

	movq stdout, %rdi
	movq $output, %rsi
	movq $0, %rax
	call fprintf

	movq $0, %rax
	leave
	ret
