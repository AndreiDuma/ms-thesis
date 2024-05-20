	.equ SYSCALL_READ, 0x00
	.equ SYSCALL_WRITE, 0x01
	.equ SYSCALL_EXIT, 0x3c

	.equ STDIN, 0
	.equ STDOUT, 1
	.equ STDERR, 2
	
	.section .rodata
lf_str:
	.ascii "\n"

	.section .bss
key_read_buf:
	.skip 2

	.section .text
lf:
	movq $SYSCALL_WRITE, %rax
	movl $STDOUT, %edi
	leaq lf_str(%rip), %rsi
	movq $1, %rdx
	syscall
	
	jmp exit

key:
	## read one character from stdin
	movq $SYSCALL_READ, %rax
	movl $STDIN, %edi
	leaq key_read_buf(%rip), %rsi
	movq $1, %rdx
	syscall

	## return the character
	## TODO: handle EOF
	movzbl key_read_buf(%rip), %eax

	jmp exit
	

### Testing
	.globl _start
_start:
	jmp key

exit:	
	movq %rax, %rdi
	movq $SYSCALL_EXIT, %rax
	syscall
