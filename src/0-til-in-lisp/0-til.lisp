(defparameter *I* nil)
(defparameter *WA* nil)
(defparameter *CA* nil)
(defparameter *RS* nil)
(defparameter *SP* nil)
(defparameter *PC* nil)

(defparameter *memory* (make-array 16 :initial-element 0))

(macrolet ((register->parameter (r)
	     `(symbol-value (intern (format nil "*~A*" ,r)))))
  (defun register (r)
    (register->parameter r))
  (defun (setf register) (value r)
    (setf (register->parameter r) value)))

(defun memory (addr)
  (aref *memory* addr))
(defun (setf memory) (value addr)
  (setf (aref *memory* addr) value))

(defun op-code (instr)
  (car instr))
(defun op-1 (instr)
  (cadr instr))
(defun op-2 (instr)
  (caddr instr))

(defun advance-pc ()
  (incf (register 'PC)))

(defun execute (instr)
  (ecase (op-code instr)
    ('@A->B
     ;; The contents of the memory location word whose address
     ;; is in register A are loaded into register B.
     (progn (setf (register (op-2 instr))
		  (memory (register (op-1 instr))))
	    (advance-pc)))
    ('A=A+n
     ;; The contents of register A are incremented by the constant n.
     (progn (setf (register (op-1 instr))
		  (op-2 instr))
	    (advance-pc)))
    ('POP-S->A
     ;; The S push down stack top entry is loaded to register A
     ;; and the stack pointer is adjusted.
     (progn (setf (register (op-2 instr))
		  (memory (register (op-1 instr))))
	    (incf (register (op-1 instr)))
	    (advance-pc)))
    ('PSH-A->S
     ;; The A register contents are loaded to the S push down
     ;; stack and the stack pointer is adjusted.
     (progn (setf (memory (register (op-2 instr)))
		  (register (op-1 instr)))
	    (decf (register (op-2 instr)))
	    (advance-pc)))
    ('A->PC
     ;; The contents of the A register are loaded into the PC.
     ;; The processor will fetch its next instruction from this location.
     (setf (register 'PC)
		  (register (op-1 instr))))
    ('JMP-XX
     ;; Unconditional jump to the address contained in the word
     ;; following the jump instruction.
     (setf (register 'PC)
		   (op-1 instr)))))

(defun tick ()
  (execute (memory (register 'PC))))

(defun dump ()
  (loop for v across *memory*
	for addr from 0
	do (format t "~2d: ~a~%" addr v)))

;; Inner interpreter
;; (setf ())
