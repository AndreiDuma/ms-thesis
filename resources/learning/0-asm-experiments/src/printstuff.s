	.globl printstuff

	.section .data
fmt:
	.ascii "hello world\n\0"

	.section .text
printstuff:
	enter $0, $0
	
	movq stdout@GOTPCREL(%rip), %rdi
	movq (%rdi), %rdi
	leaq fmt(%rip), %rsi
	movq $0, %rax
	call fprintf@plt

	leave
	ret
