	.globl _start

	.section .data
mytext:
	.ascii "This is a string of characters.\0"

	.section .text
_start:
	leaq mytext, %rbx
	movq $0, %rdi

iter:
	movb (%rbx), %al

	## Quit if we hit the null terminator
	cmpb $0, %al
	je finish

	cmpb $'a', %al
	jb loopcontrol
	cmpb $'z', %al
	ja loopcontrol

	## It's lowercase!
	incq %rdi

loopcontrol:
	incq %rbx
	jmp iter

finish:	
	movq $60, %rax
	syscall
