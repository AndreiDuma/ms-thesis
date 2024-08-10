	.globl main

	.section .data
filename:
	.ascii "myfile.txt\0"
openmode:
	.ascii "w\0"

formatstring1:
	.ascii "The age of %s is %d.\n\0"
sallyname:
	.ascii "Sally\0"
sallyage:
	.quad 53

formatstring2:
	.ascii "%d and %d are %s's favorite numbers.\n\0"
joshname:
	.ascii "Josh\0"
joshfavorite1:
	.quad 7
joshfavorite2:
	.quad 13

	.section .text
	.equ LOCAL_FILE, -8
main:
	pushq %rbp		# create stack frame
	movq %rsp, %rbp
	subq $16, %rsp

	## open file
	movq $filename, %rdi
	movq $openmode, %rsi
	call fopen
	movq %rax, LOCAL_FILE(%rbp)

	## write first string
	movq LOCAL_FILE(%rbp), %rdi
	movq $formatstring1, %rsi
	movq $sallyname, %rdx
	movq sallyage, %rcx
	movq $0, %rax
	call fprintf
	
	## write second string
	movq LOCAL_FILE(%rbp), %rdi
	movq $formatstring2, %rsi
	movq joshfavorite1, %rdx
	movq joshfavorite2, %rcx
	movq $joshname, %r8
	movq $0, %rax
	call fprintf

	## close the file
	movq LOCAL_FILE(%rbp), %rdi
	call fclose

	leave			# destroy stack frame
	ret
