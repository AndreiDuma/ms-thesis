	.equ SYSCALL_READ, 0x00
	.equ SYSCALL_WRITE, 0x01
	.equ SYSCALL_EXIT, 0x3c

	## standard streams
	.equ STDIN, 0
	.equ STDOUT, 1
	.equ STDERR, 2

	## other constants
	## .equ PARAMETER_STACK_CELLS, 128
	## .equ PARAMETER_STACK_SIZE, 8 * PARAMETER_STACK_CELLS
	## .equ RETURN_STACK_CELLS, 128
	## .equ RETURN_STACK_SIZE, 8 * RETURN_STACK_CELLS
	.equ TOKEN_MAX_LENGTH, 31

	.equ ERROR_READ_INVALID_CHAR, 1
	.equ ERROR_EXCEEDED_MAX_TOKEN_LENGTH, 2

	.equ LF, '\n'

	.section .data
parameter_stack_origin:
	.zero 128 * 8
return_stack_origin:
	.zero 128 * 8


### Exits the process with the given exit code.
	.section .text
die:
	movq %r12, %rdi
	movq $SYSCALL_EXIT, %rax
	syscall


### Prints an error message according to the given error code.
	.section .rodata
error_1:
	.ascii "read invalid character"
error_1_end:
error_2:
	.ascii "exceeded maximum token length"
error_2_end:

	.section .text
error:
	cmpq $ERROR_READ_INVALID_CHAR, %r12
	je error_1_print

	cmpq $ERROR_EXCEEDED_MAX_TOKEN_LENGTH, %r12
	je error_2_print

error_1_print:
	leaq error_1(%rip), %rsi
	movq $(error_1_end - error_1), %rdx
	jmp error_print

error_2_print:
	leaq error_2(%rip), %rsi
	movq $(error_2_end - error_2), %rdx
	jmp error_print

error_print:
	movq $SYSCALL_WRITE, %rax
	movl $STDERR, %edi
	syscall

	movq $LF, %r12
	call _echo

	ret


### Prints and error message and dies (both with same error code).
	.section .text
error_and_die:
	movq %r12, %r13
	call error
	movq %r13, %r12
	call die


### Tests if a character is whitespace (SPACE, LF, TAB).
## 	.section .rodata
## is_whitespace_chars:
## 	.ascii " \n\t"

	.section .text
is_whitespace:
	## movb %r12b, %al
	## movq $is_whitespace_chars, %rdi
	## movq $3, %rcx
	## cld
	## repne scasb
	## je is_whitespace_indeed
	cmpb $' ', %r12b
	je is_whitespace_indeed
	cmpb $'\n', %r12b
	je is_whitespace_indeed
	cmpb $'\t', %r12b
	je is_whitespace_indeed

	movq $0, %r12
	ret
is_whitespace_indeed:
	movq $1, %r12
	ret


### Tests if a character is a valid token character (ASCII 0x21 to 0x7E).
	.section .text
is_valid_token_char:
	cmpb $0x21, %r12b
	jl is_valid_token_char_no
	cmpb $0x7E, %r12b
	jg is_valid_token_char_no

	movq $1, %r12
	ret
is_valid_token_char_no:
	movq $0, %r12
	ret

	
### Outputs the LF (line feed) character.
	.section .rodata
_lf_str:
	.ascii "\n"

	.section .text
_lf:
	movq $SYSCALL_WRITE, %rax
	movl $STDOUT, %edi
	leaq _lf_str(%rip), %rsi
	movq $1, %rdx
	syscall

	ret


### Reads a character (or EOF) from STDIN.
	.section .bss
_key_buf:
	## we'll only read one character at a time
	.skip 1
	
	.section .text
_key:
	## read from STDOUT into buffer
	movq $SYSCALL_READ, %rax
	movl $STDIN, %edi
	leaq _key_buf(%rip), %rsi
	movq $1, %rdx
	syscall

	## return the character in W0
	movzbq _key_buf(%rip), %r12
	ret


### Print a character to STDOUT.
	.section .bss
_echo_buf:
	## we'll only print one character at a time
	.skip 1

	.section .text
_echo:
	## move character from W0 to buffer
	movb %r12b, _echo_buf(%rip)

	## write from buffer to STDOUT
	movq $SYSCALL_WRITE, %rax
	movl $STDOUT, %edi
	leaq _echo_buf(%rip), %rsi
	movq $1, %rdx
	syscall

	ret


### Read a token from STDIN.
	.section .data
_token_buf:	
	.skip TOKEN_MAX_LENGTH

	.section .text
_token:
	## TODO: get delimiter from parameter stack
	movb $' ', %al		# in what register?

_token_key:
	call _key

	## cmpq %rcx, $0
	## jne _token_key		# current key is whitespace

	## if chr == separator:
	## 	jmp _token_key

	## move chr to token buf
	## increment position
	## if position > MAX:
	## 	jmp _token_too_long
	## jmp _token_key

	## ...
	
	
### Testing
	.globl _start
	.section .text
_start:
	## initialize stack pointers
	movq $parameter_stack_origin - 8, %rbp # PSP
	movq $return_stack_origin - 8, %rbx    # RSP

	## jmp _lf

	## call _key

	## call _echo

	## print a newline
	## movb $LF, %r12b
	## call _echo

	## is it whitespace?
	## call is_whitespace

	## is it a valid token char?
	## call is_valid_token_char

	## print an error message
	## movq $ERROR_EXCEEDED_MAX_TOKEN_LENGTH, %r12
	## call error_and_die

	## exit
	call die
