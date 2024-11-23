# *Sapphire/60 Assembly Manual*

## Registers

1. **`ACC`**: Accumulator
2. **`BCC`**: Secondary accumulator
3. **`BAK`**: *Non-addressable* [bankable register](#swp)
4. **`PRH`**: *Non-addressable* program counter (high byte)
5. **`PRL`**: *Non-addressable* program counter (low byte)
6. **`CRY`**: Carry flag
7. **`NIL`**: Accumulator zero flag
8. **`LTZ`**: Arithmetic result negative sign flag
9. **`ITH`**: *Non-addressable* interrupt return address (high byte)
10. **`ITL`**: *Non-addressable* interrupt return address (low byte)

## Opcodes

Each opcode should have a line dedicated specifically to it.

A label is a special kind of an opcode. Labels denote memory locations, to which a branching opcode could lead.
Labels are defined as `identifier:` and should satisfy regex `[a-zA-Z0-9_]+`.

The full list of supported opcodes is as follows:

**Idling:** [NOP](#nop)
**Memory:** [LDA](#lda) [STA](#sta) [STS](#sts)
**Branching:** [JMP](#jmp) [JLZ](#jlz) [JGZ](#jgz) [JEZ](#jez)
**Bitwise:** [NOT](#not) [AND](#and) [OR](#or) [XOR](#xor)
**Data:** [MOV](#mov) [SWP](#swp) [DUP](#dup)
**Math:** [ADD](#add) [NEG](#neg) [MUL](#mul) [DIV](div)
**Interrupts:** [INT](#int) [RFI](#rfi)
**Execution:** [END](#end)

### NOP

***No OPcode***

**Syntax:** `NOP`
**Cost:** 1 cycle

Do nothing.

### LDA

***LoaD Address***

**Syntax:** `LDA`, `LDA 0xFFFF`
**Cost:** 2 cycles

Loads byte from a memory address stored in either `BCC, BAK` or a hex literal, into ACC.

### STA

***STore Address***

**Syntax:** `STA`, `STA 0xFFFF`
**Cost:** 2 cycles

Stores ACC into a memory address stored in either `BCC, BAK` or a hex literal.

### STS

***STore String***

**Syntax:** `STS $Hellorld!`
**Cost:** 8 + #string cycles

Stores string literal beginning at an address stored in `BCC, BAK` as a null-terminated C string with zero regard for memory contents and address overflows.

### JMP

***JuMP***

**Syntax:** `JMP LABEL`
**Cost:** 2 cycles

Unconditional jump to a label.

### JLZ

***Jump if Less than Zero***

**Syntax:** `JLZ LABEL`
**Cost:** 3 cycles

Jump to a label if `ACC < 0`.

### JGZ

***Jump if Greater than Zero***

**Syntax:** `JGZ LABEL`
**Cost:** 3 cycles

Jump to a label if `ACC > 0`.

### JEZ

***Jump if Equal to Zero***

**Syntax:** `JEZ LABEL`
**Cost:** 3 cycles

Jump to a label if `ACC == 0`.

### NOT

***bitwise NOT***

**Syntax:** `NOT`
**Cost:** 1 cycle

Invert every bit of ACC.

### AND

***bitwise AND***

**Syntax:** `AND`
**Cost:** 1 cycle

Bitwise AND the ACC and BCC registers, storing the result in ACC.

### OR

***bitwise OR***

**Syntax:** `OR`
**Cost:** 1 cycle

Bitwise OR the ACC and BCC registers, storing the result in ACC.

### XOR

***bitwise XOR***

**Syntax:** `XOR`
**Cost:** 1 cycle

Bitwise XOR the ACC and BCC registers, storing the result in ACC.

### MOV

***MOVe into register***

**Syntax:** `MOV TGT, SRC`, `MOV TGT, 42`
**Cost:** 1 cycle

Assign a value into register `TGT` from register `SRC` or a literal.

### SWP

***SWaP registers***

**Syntax:** `SWP`
**Cost:** 1 cycle

Swap values between `BCC` and `BAK`.

### DUP

***DUPlicate registers***

**Syntax:** `DUP`
**Cost:** 1 cycle

Copy value from `BCC` into `BAK`.

### ADD

***arithmetic ADD***

**Syntax:** `ADD SRC`, `ADD 42`
**Cost:** 1 cycle

Add register `SRC` or a literal to `ACC`, storing result in `ACC`.
**Side effects:** Sets `CRY` if `ACC` overflows as a result. Sets `LTZ` if the number becomes negative. Clears `LTZ` otherwise.

### NEG

***arithmetic NEGating***

**Syntax:** `NEG REG`
**Cost:** 1 cycle

Equivalent to multiplying `REG` by `-1`, stores result in `REG`.
**Side effects:** Sets `CRY` if `REG` was over `127`. Sets `LTZ` if the number becomes negative. Clears `LTZ` otherwise.

### MUL

***arithmetic MULtiplication***

**Syntax:** `MUL REG`, `MUL 42`
**Cost:** 2 cycles

Multiply `ACC` by `REG` or a literal. Stores result in `ACC`.
**Side effects:** Sets `CRY` if result overflows `ACC`. Sets `LTZ` if the number becomes negative. Clears `LTZ` otherwise.

### DIV

***arithmetic DIVision***

**Syntax:** `DIV REG`, `DIV 42`
**Cost:** 2 cycles

Divide `ACC` by `REG` or a literal. Stores dividend in `ACC`, stores quotient in `BCC`. When dividing by zero, `BCC` is set to zero.
**Side effects:** Sets `CRY` when dividing by zero. Sets `LTZ` if the number becomes negative. Clears `LTZ` otherwise.

### INT

***INTerrupt call***

**Syntax:** `INT`, `INT 0x42`
**Cost:** 2 cycles before interrupt

Jump to an interrupt vector defined by `ACC` or a hex literal.
**Side effects:** Don't expect any registers to stay unmodified. They will contain return data.

### RFI

***Return From Interrupt***

**Syntax:** `RFI`
**Cost:** 2 cycles

Jump to `ITH, ITL` to return to the instruction that comes after the initial `INT` call. Only applicable if the program is in an interrupt at the moment of execution.

### END

***END of execution***

**Syntax:** `END`
**Cost:** 1 cycle

Immediate shutdown.
**Side effects:** Someone's gotta turn the thing on again.

