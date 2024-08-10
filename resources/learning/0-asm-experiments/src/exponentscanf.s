	.globl main
	.type main, @function
	.type main_scanf_fmt, @object
	.type main_printf_fmt, @object

	.section .rodata
main_scanf_fmt:
	.ascii "%d %d\0"
main_printf_fmt:
	.ascii "%d^%d = %d\n\0"
	
	.section .text
	.equ LOCAL_BASE, -8
	.equ LOCAL_EXP, -16
main:
	pushq %rbp		# setup stack frame
	movq %rsp, %rbp
	subq $16, %rsp

	## read
	movq stdin, %rdi
	movq $main_scanf_fmt, %rsi
	leaq LOCAL_BASE(%rbp), %rdx
	leaq LOCAL_EXP(%rbp), %rcx
	movq $0, %rax
	call fscanf

	## compute exponent
	movq LOCAL_BASE(%rbp), %rdi
	movq LOCAL_EXP(%rbp), %rsi
	call exponent

	## print
	movq stdout, %rdi
	movq $main_printf_fmt, %rsi
	movq LOCAL_BASE(%rbp), %rdx
	movq LOCAL_EXP(%rbp), %rcx
	movq %rax, %r8
	movq $0, %rax
	call fprintf

	movq $0, %rax
	leave			# destroy stack frame
	ret
