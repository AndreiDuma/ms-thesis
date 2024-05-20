	.globl main

	.section .rodata
printf_fmt:
	.ascii "%s\n\0"

	.section .text
main:
	enter $0, $0
	pushq %rbx		# save registers
	pushq %r12

	## allocate buffer
	movq $500, %rdi
	call malloc
	movq %rax, %rbx

	## read from stdin until EOF
	movq $0, %r12		# initialize counter
read_char:	
	call getchar
	cmpl $-1, %eax
	jne copy_char

	## encountered EOF: add \0, print and return
	movb $0, (%rbx,%r12)
	movq $printf_fmt, %rdi
	movq %rbx, %rsi
	movq $0, %rax
	call printf
	jmp done

copy_char:
	## copy char and loop
	movb %al, (%rbx,%r12)
	incq %r12
	jmp read_char

done:	
	## free buffer
	movq %rbx, %rdi
	call free

	popq %r12		# restore registers
	popq %rbx
	leave
	ret
