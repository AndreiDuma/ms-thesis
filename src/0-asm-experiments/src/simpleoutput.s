	.globl _start

	.section .data
msg:
	.ascii "hello world!\n"
msg_end:
	.equ msg_len, msg_end - msg

	.section .text
_start:
	## Display the string
	movq $0x01, %rax
	movq $1, %rdi
	movq $msg, %rsi
	movq $msg_len, %rdx
	syscall

	## Exit
	movq $0x3c, %rax
	movq $0, %rdi
	syscall
