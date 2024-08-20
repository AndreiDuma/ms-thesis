include("encodings.jl")


### I instructions ###

macro encode_i_instr(mnemonic, subtype, funct3, opcode)
    :(encode(::Val{$mnemonic}, rd::Symbol, rs1::Symbol, imm::Int; fmt::Symbol=:bin) =
        encode(IInstr(unsigned(Int32(imm)), rs1, UInt32($funct3), rd, UInt32($opcode)), Val(fmt), Val($subtype)))
end

# Environment calls
# RV32I
encode(::Val{:ecall}; fmt::Symbol=:bin) =
    encode(IInstr(0, 0, 0, 0, UInt32(0b1110011)), Val(fmt), Val(:env))
encode(::Val{:ebreak}; fmt::Symbol=:bin) =
    encode(IInstr(1, 0, 0, 0, UInt32(0b1110011)), Val(fmt), Val(:env))

# Shifts
# RV32I
@encode_i_instr(:slli, :shift, 0b001, 0b0010011)
@encode_i_instr(:srli, :shift, 0b101, 0b0010011)

# Arithmetic & logic
# RV32I
@encode_i_instr(:addi, :math, 0b000, 0b0010011)
@encode_i_instr(:slti, :math, 0b010, 0b0010011)
@encode_i_instr(:sltiu, :math, 0b011, 0b0010011)
@encode_i_instr(:xori, :math, 0b100, 0b0010011)
@encode_i_instr(:ori, :math, 0b110, 0b0010011)
@encode_i_instr(:andi, :math, 0b111, 0b0010011)

# Loads
# RV32I
@encode_i_instr(:lb, :load, 0b000, 0b0000011)
is_load(::Val{:lb}) = true
@encode_i_instr(:lh, :load, 0b001, 0b0000011)
is_load(::Val{:lh}) = true
@encode_i_instr(:lw, :load, 0b010, 0b0000011)
is_load(::Val{:lw}) = true
@encode_i_instr(:lbu, :load, 0b100, 0b0000011)
is_load(::Val{:lbu}) = true
@encode_i_instr(:lhu, :load, 0b101, 0b0000011)
is_load(::Val{:lhu}) = true

# RV64I
@encode_i_instr(:lwu, :load, 0b110, 0b0000011)
is_load(::Val{:lwu}) = true
@encode_i_instr(:ld, :load, 0b011, 0b0000011)
is_load(::Val{:ld}) = true

# Jalr
# RV32I
@encode_i_instr(:jalr, :jump, 0b000, 0b1100111)
is_jump(::Val{:jalr}) = true


### R instructions ###

macro encode_r_instr(mnemonic, funct7, funct3, opcode)
    :(encode(::Val{$mnemonic}, rd::Symbol, rs1::Symbol, rs2::Symbol; fmt::Symbol=:bin) =
        encode(RInstr(UInt32($funct7), rs2, rs1, UInt32($funct3), rd, UInt32($opcode)), Val(fmt)))
end

@encode_r_instr(:add, 0b0000000, 0b000, 0b0110011)
@encode_r_instr(:or, 0b0000000, 0b000, 0b0110011)
@encode_r_instr(:and, 0b0000000, 0b000, 0b0110011)
@encode_r_instr(:sub, 0b0000000, 0b000, 0b0110011)


### S instructions ###

macro encode_s_instr(mnemonic, funct3, opcode)
    :(encode(::Val{$mnemonic}, rs2::Symbol, rs1::Symbol, offset::Int; fmt::Symbol=:bin) =
        encode(SInstr(Int32(offset), rs2, rs1, UInt32($funct3), UInt32($opcode)), Val(fmt)))
end

@encode_s_instr(:sb, 0b000, 0b0100011)
is_store(::Val{:sb}) = true
@encode_s_instr(:sh, 0b001, 0b0100011)
is_store(::Val{:sh}) = true
@encode_s_instr(:sw, 0b010, 0b0100011)
is_store(::Val{:sw}) = true
@encode_s_instr(:sd, 0b011, 0b0100011)
is_store(::Val{:sd}) = true


### B instructions ###

macro encode_b_instr(mnemonic, funct3, opcode)
    :(encode(::Val{$mnemonic}, rs2::Symbol, rs1::Symbol, offset::Int; fmt::Symbol=:bin) =
        encode(BInstr(Int32(offset), rs2, rs1, UInt32($funct3), UInt32($opcode)), Val(fmt)))
end

@encode_b_instr(:beq, 0b000, 0b1100011)
@encode_b_instr(:bne, 0b001, 0b1100011)
@encode_b_instr(:blt, 0b100, 0b1100011)
@encode_b_instr(:bge, 0b101, 0b1100011)
@encode_b_instr(:bltu, 0b110, 0b1100011)
@encode_b_instr(:bgeu, 0b111, 0b1100011)


### U instructions ###

is_u_instr(instr::Symbol) =
    try
        is_u_instr(Val(instr))
    catch
        false
    end

macro encode_u_instr(mnemonic, opcode)
    :(encode(::Val{$mnemonic}, rd::Symbol, imm::UInt32; fmt::Symbol=:bin) =
        encode(UInstr(imm, rd, UInt32($opcode)), Val(fmt)))
end

@encode_u_instr(:auipc, 0b0010111)
is_u_instr(::Val{:auipc}) = true
@encode_u_instr(:lui, 0b0110111)
is_u_instr(::Val{:lui}) = true


### J instructions ###

macro encode_j_instr(mnemonic, opcode)
    :(encode(::Val{$mnemonic}, rd::Symbol, offset::Int; fmt::Symbol=:bin) =
        encode(JInstr(Int32(offset), rd, UInt32($opcode)), Val(fmt)))
end

@encode_j_instr(:jal, 0b1101111)


# Dispatch...
function encode_hex(instr::Symbol, args...)
    enc_str = encode(instr, args...) |> bswap |> repr

    return string(enc_str[3:4], " ", enc_str[5:6], " ", enc_str[7:8], " ", enc_str[9:10])
end

function encode(instr::Symbol, args...; fmt::Symbol=:bin)
    if fmt == :bin || fmt == :emacs
        encode(Val(instr), args...; fmt=fmt)
    elseif fmt == :hex
        encode_hex(instr, args...)
    end
end


# Parse
is_indirect(instr::Symbol) =
    try
        is_load(Val(instr))
    catch
        try
            is_store(Val(instr))
        catch
            try
                is_jump(Val(instr))
            catch
                false
            end
        end
    end

# TODO: prettify
function parse(instr::String)
    instr_components = Vector{Any}(filter(!isempty, split(instr, [',', ' ', '(', ')'])))
    instr_components[1] = Symbol(instr_components[1])

    if length(instr_components) == 1
        return Tuple(instr_components)
    end

    mnemonic = instr_components[1]
    if is_indirect(mnemonic)
        # swap arguments
        splice!(instr_components, 3:4, [instr_components[4], instr_components[3]])
    end

    # convert registers to symbols
    instr_components[2:end-1] .= Symbol.(instr_components[2:end-1])

    # convert immediate to number
    if is_u_instr(mnemonic)
        instr_components[end] = tryparse(UInt32, instr_components[end])
    else
        imm = tryparse(Int, instr_components[end])
        if !isnothing(imm)
            instr_components[end] = imm
        end
    end

    return Tuple(instr_components)
end
