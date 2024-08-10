	.globl _start

	.section .text
	.equ LOCAL_N, -8
factorial:
	## %rdi has the factorial argument `n`
	
	pushq %rbp		# create stack frame
	movq %rsp, %rbp
	subq $16, %rsp		# 16-byte stack alignment!

	cmpq $0, %rdi
	jnz factorial_recurse

	## base case
	movq $1, %rax
	jmp factorial_done
	
factorial_recurse:	
	movq %rdi, LOCAL_N(%rbp) # save argument `n` on stack
	## call recursively
	decq %rdi
	call factorial

	imul LOCAL_N(%rbp), %rax # multiply with `n` from stack to get result

factorial_done:	
	leave			# destroy stack frame
	ret

_start:
	movq $5, %rdi
	call factorial

	movq %rax, %rdi
	movq $60, %rax
	syscall
