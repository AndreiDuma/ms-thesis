(defparameter +abi-registers+
  '((zero . x0)
    (ra . x1)
    (sp . x2)))

(defparameter +arch-registers+
  '((x0 . #*00000)
    (x1 . #*00001)
    (x2 . #*00010)))

(defun encode-register (r)
  (let* ((abi-r (cdr (assoc r +abi-registers+)))
	 (arch-r (or abi-r r)))
    (cdr (assoc arch-r +arch-registers+))))

;; (defun add (rd rs1 rs2)
;;   (concatenate '(simple-bit-vector 32)
;; 	       #*0000000		; funct7
;; 	       (encode-register rs2)
;; 	       (encode-register rs1)
;; 	       #*000			; fn3
;; 	       (encode-register rd)
;; 	       #*0110011))		; opcode

(defmacro definstr-r (instr &key opcode funct7 funct3)
  `(defun ,instr (rd rs1 rs2)
     (concatenate '(simple-bit-vector 32)
		  ,funct7
		  (encode-register rs2)
		  (encode-register rs1)
		  ,funct3
		  (encode-register rd)
		  ,opcode)))

(definstr-r add
  :opcode #*0110011
  :funct3 #*000
  :funct7 #*0000000)

(definstr-r sub
  :opcode #*0110011
  :funct3 #*000
  :funct7 #*0100000)
