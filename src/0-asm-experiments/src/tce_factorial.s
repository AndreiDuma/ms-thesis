	.globl main

	.section .data
main_fmt:
	.string "%d! = %d\n"
	
	.section .text
factorial:
	## %rdi = parameter `n`
	movq $1, %rsi
	jmp factorial_iter

factorial_iter:
	## %rdi = `n`, %rsi = accumulator
	cmpq $0, %rdi	      # base case?
	je factorial_iter$

	imul %rdi, %rsi	      # recursive "call"
	decq %rdi
	jmp factorial_iter	
factorial_iter$:	
	movq %rsi, %rax	      # we reached 0 -> just return accumulator
	ret

	.equ VALUE, 5
main:
	## compute factorial
	movq $VALUE, %rdi
	call factorial

	## print result
	leaq main_fmt(%rip), %rdi
	movq $VALUE, %rsi
	movq %rax, %rdx
	movq $0, %rax
	call printf

	movq $0, %rax
	ret
