	.globl exponent
	.type exponent, @function

	.section .note.GNU-stack, "", @progbits
	.section .text
exponent:
	## %rdi has the base
	## %rsi has the exponent

	enter $0, $0		# create stack frame
	## Equivalent:
	## pushq %rbp
	## movq %rsp, %rbp

	movq $1, %rax		# result register
	cmpq $0, %rsi		# if exponent is 0, we're done
	je exponent_done
exponent_loop:
	imul %rdi, %rax		# multiply by base
	decq %rsi
	jnz exponent_loop

exponent_done:
	leave			# destroy stack frame
	## Equivalent:
	## movq %rbp, %rsp
	## popq %rbp
	ret
	## %rax has the result
