using JetFly.Sapphire60.Common;
using JetFly.Sapphire60.Assembler.Common;

namespace JetFly.Sapphire60.Instructions;

public class NopInstr : Instruction
{
    public NopInstr(byte[] bytes) : base(bytes, 1)
    {
        AddBehavior(0x00, Utils.ArgumentOrder.None, NoneOpcode);
    }
}

public class LdaInstr : Instruction
{
    // TODO: Replace ArgumentOrder with byte
    public LdaInstr(byte[] bytes) : base(bytes, 2)
    {
        AddBehavior(0x10, Utils.ArgumentOrder.None, NoneOpcode);
        AddBehavior(0x11, Utils.ArgumentOrder.Word, WordOpcode);
    }

    protected override void NoneOpcode()
    {
        ushort addr = MemoryUtils.GetAddress(state.BCC, state.BAK);
        state.ACC = state.MEMORY[addr];
        state.NIL = state.ACC == 0;
        state.OnMemoryRead(this, addr, state.ACC);
        base.NoneOpcode();
    }

    protected override void WordOpcode()
    {
        ushort addr = MemoryUtils.GetAddress(bytes[1], bytes[2]);
        state.ACC = state.MEMORY[addr];
        state.NIL = state.ACC == 0;
        state.OnMemoryRead(this, addr, state.ACC);
        base.WordOpcode();
    }
}

public class StaInstr : Instruction
{
    // TODO: Replace ArgumentOrder with byte
    public StaInstr(byte[] bytes) : base(bytes, 2)
    {
        AddBehavior(0x14, Utils.ArgumentOrder.None, NoneOpcode);
        AddBehavior(0x15, Utils.ArgumentOrder.Word, WordOpcode);
    }

    protected override void NoneOpcode()
    {
        ushort addr = MemoryUtils.GetAddress(state.BCC, state.BAK);
        state.MEMORY[addr] = state.ACC;
        state.OnMemoryWritten(this, addr, state.ACC);
        base.NoneOpcode();
    }

    protected override void WordOpcode()
    {
        ushort addr = MemoryUtils.GetAddress(bytes[1], bytes[2]);
        state.MEMORY[MemoryUtils.GetAddress(bytes[1], bytes[2])] = state.ACC;
        state.OnMemoryWritten(this, addr, state.ACC);
        base.WordOpcode();
    }
}

public class StsInstr : Instruction
{
    public StsInstr(byte[] bytes) : base(bytes, 8)
    {
        AddBehavior(0x18, Utils.ArgumentOrder.String, StringOpcode);
        if(bytes.Length >= 2)
            cost += bytes[1];
    }

    protected override void StringOpcode()
    {
        ushort addr = MemoryUtils.GetAddress(state.BCC, state.BAK);
        bytes[2..Math.Min(2 + bytes[1], bytes.Length)].CopyTo(state.MEMORY, addr);
        state.PRC += bytes[1];
        for(int i = 0; i < Math.Min(bytes[1], bytes.Length - 2); i++)
            state.OnMemoryWritten(this, (ushort)(addr + i), state.MEMORY[addr + i]);
        base.StringOpcode();
    }

}

public class JmpInstr : Instruction
{
    public JmpInstr(byte[] bytes) : base(bytes, 2)
    {
        AddBehavior(0x20, Utils.ArgumentOrder.Word, WordOpcode);
    }

    protected override void WordOpcode()
    {
        state.PRC = MemoryUtils.GetAddress(bytes[1], bytes[2]);
    }
}

public class JlzInstr : Instruction
{
    public JlzInstr(byte[] bytes) : base(bytes, 3)
    {
        AddBehavior(0x24, Utils.ArgumentOrder.Word, WordOpcode);
    }

    protected override void WordOpcode()
    {
        if(state.LTZ || state.ACC < 0)
            state.PRC = MemoryUtils.GetAddress(bytes[1], bytes[2]);
        else
        {
            state.PRC++;
            base.WordOpcode();
        }
    }
}

public class JgzInstr : Instruction
{
    public JgzInstr(byte[] bytes) : base(bytes, 3)
    {
        AddBehavior(0x28, Utils.ArgumentOrder.Word, WordOpcode);
    }

    protected override void WordOpcode()
    {
        if(!state.LTZ && state.ACC > 0)
            state.PRC = MemoryUtils.GetAddress(bytes[1], bytes[2]);
        else
        {
            state.PRC++;
            base.WordOpcode();
        }
    }
}

public class JezInstr : Instruction
{
    public JezInstr(byte[] bytes) : base(bytes, 3)
    {
        AddBehavior(0x2B, Utils.ArgumentOrder.Word, WordOpcode);
    }

    protected override void WordOpcode()
    {
        if(state.ACC == 0)
            state.PRC = MemoryUtils.GetAddress(bytes[1], bytes[2]);
        else
        {
            state.PRC++;
            base.WordOpcode();
        }
    }
}

public class JcsInstr : Instruction
{
    public JcsInstr(byte[] bytes) : base(bytes, 3)
    {
        AddBehavior(0x2F, Utils.ArgumentOrder.Word, WordOpcode);
    }

    protected override void WordOpcode()
    {
        if(state.CRY)
            state.PRC = MemoryUtils.GetAddress(bytes[1], bytes[2]);
        else
        {
            state.PRC++;
            base.WordOpcode();
        }
    }
}

public class NotInstr : Instruction
{
    public NotInstr(byte[] bytes) : base(bytes, 1)
    {
        AddBehavior(0x30, Utils.ArgumentOrder.None, NoneOpcode);
    }

    protected override void NoneOpcode()
    {
        state.ACC = unchecked((byte)~state.ACC);
        // state.LTZ = unchecked((sbyte)state.ACC < 0);
        state.NIL = state.ACC == 0;
        base.NoneOpcode();
    }
}

public class AndInstr : Instruction
{
    public AndInstr(byte[] bytes) : base(bytes, 1)
    {
        AddBehavior(0x34, Utils.ArgumentOrder.None, NoneOpcode);
    }

    protected override void NoneOpcode()
    {
        state.ACC &= state.BCC;
        // state.LTZ = unchecked((sbyte)state.ACC < 0);
        state.NIL = state.ACC == 0;
        base.NoneOpcode();
    }
}

public class OrInstr : Instruction
{
    public OrInstr(byte[] bytes) : base(bytes, 1)
    {
        AddBehavior(0x38, Utils.ArgumentOrder.None, NoneOpcode);
    }

    protected override void NoneOpcode()
    {
        state.ACC |= state.BCC;
        // state.LTZ = unchecked((sbyte)state.ACC < 0);
        state.NIL = state.ACC == 0;
        base.NoneOpcode();
    }
}

public class XorInstr : Instruction
{
    public XorInstr(byte[] bytes) : base(bytes, 1)
    {
        AddBehavior(0x3B, Utils.ArgumentOrder.None, NoneOpcode);
    }

    protected override void NoneOpcode()
    {
        state.ACC ^= state.BCC;
        // state.LTZ = unchecked((sbyte)state.ACC < 0);
        state.NIL = state.ACC == 0;
        base.NoneOpcode();
    }
}

public class MovInstr : Instruction
{
    public MovInstr(byte[] bytes) : base(bytes, 1)
    {
        AddBehavior(0x40, Utils.ArgumentOrder.RegisterByte, RegisterByteOpcode);
        AddBehavior(0x41, Utils.ArgumentOrder.RegisterRegister, RegisterRegisterOpcode);
    }

    protected override void RegisterByteOpcode()
    {
        switch(bytes[1])
        {
            case 0x00:
                state.ACC = bytes[2];
                state.LTZ = unchecked((sbyte)state.ACC < 0);
                state.NIL = state.ACC == 0;
                break;
            case 0x01:
                state.BCC = bytes[2];
                state.LTZ = unchecked((sbyte)state.BCC < 0);
                break;
            default:
                throw new InvalidOperationException("Invalid target register");
        }
        base.RegisterByteOpcode();
    }

    protected override void RegisterRegisterOpcode()
    {
        if(bytes[2] > 0x1)
            throw new InvalidOperationException("Invalid source register");
        switch(bytes[1])
        {
            case 0x00:
                if(bytes[2] == 0x01)
                    state.ACC = state.BCC;
                state.LTZ = unchecked((sbyte)state.ACC < 0);
                state.NIL = state.ACC == 0;
                break;
            case 0x01:
                if(bytes[2] == 0x00)
                    state.BCC = state.ACC;
                state.LTZ = unchecked((sbyte)state.BCC < 0);
                break;
            default:
                throw new InvalidOperationException("Invalid target register");
        }
        base.RegisterRegisterOpcode();
    }
}

public class SwpInstr : Instruction
{
    public SwpInstr(byte[] bytes) : base(bytes, 1)
    {
        AddBehavior(0x44, Utils.ArgumentOrder.None, NoneOpcode);
    }

    protected override void NoneOpcode()
    {
        (state.BAK, state.BCC) = (state.BCC, state.BAK);
        // state.LTZ = unchecked((sbyte)state.BCC < 0);
        base.NoneOpcode();
    }
}

public class DupInstr : Instruction
{
    public DupInstr(byte[] bytes) : base(bytes, 1)
    {
        AddBehavior(0x48, Utils.ArgumentOrder.None, NoneOpcode);
    }

    protected override void NoneOpcode()
    {
        state.BAK = state.BCC;
        base.NoneOpcode();
    }
}

public class PushInstr : Instruction
{
    public PushInstr(byte[] bytes) : base(bytes, 2)
    {
        AddBehavior(0x4B, Utils.ArgumentOrder.Byte, ByteOpcode);
        AddBehavior(0x4C, Utils.ArgumentOrder.Register, RegisterOpcode);
    }

    protected override void ByteOpcode()
    {
        if(state.STACK.Count >= Sapphire60.MAX_STACK)
            throw new SapphireException(state, "stack overflow", false);
        state.STACK.Push(bytes[1]);
        base.ByteOpcode();
    }
    protected override void RegisterOpcode()
    {
        if(state.STACK.Count >= Sapphire60.MAX_STACK)
            throw new SapphireException(state, "stack overflow", false);
        byte val = bytes[1] switch
        {
            0x00 => state.ACC,
            0x01 => state.BCC,
            _ => throw new InvalidOperationException("Invalid source register")
        };
        state.STACK.Push(val);
        base.RegisterOpcode();
    }
}

public class PopInstr : Instruction
{
    public PopInstr(byte[] bytes) : base(bytes, 2)
    {
        AddBehavior(0x4D, Utils.ArgumentOrder.None, NoneOpcode);
    }

    protected override void NoneOpcode()
    {
        state.ACC = state.STACK.Count > 0 ? state.STACK.Pop() : (byte)0x00;
        base.NoneOpcode();
    }
}

public class PeekInstr : Instruction
{
    public PeekInstr(byte[] bytes) : base(bytes, 2)
    {
        AddBehavior(0x4F, Utils.ArgumentOrder.None, NoneOpcode);
    }

    protected override void NoneOpcode()
    {
        state.ACC = state.STACK.Count > 0 ? state.STACK.Peek() : (byte)0x00;
        base.NoneOpcode();
    }
}

public class AddInstr : Instruction
{
    public AddInstr(byte[] bytes) : base(bytes, 1)
    {
        AddBehavior(0x50, Utils.ArgumentOrder.Byte, ByteOpcode);
        AddBehavior(0x51, Utils.ArgumentOrder.Register, RegisterOpcode);
    }

    protected override void ByteOpcode()
    {
        ushort result = (ushort)(state.ACC + bytes[1]);
        state.ACC = (byte)(result & 0x00FF);
        state.CRY = (result & 0xFF00) > 0;
        state.LTZ = unchecked((sbyte)state.ACC < 0);
        state.NIL = state.ACC == 0;
        base.ByteOpcode();
    }

    protected override void RegisterOpcode()
    {
        ushort result = bytes[1] switch
        {
            0x00 => (ushort)(state.ACC + state.ACC),
            0x01 => (ushort)(state.ACC + state.BCC),
            _ => throw new InvalidOperationException("Invalid source register")
        };
        state.ACC = (byte)(result & 0x00FF);
        state.CRY = (result & 0xFF00) > 0;
        state.LTZ = unchecked((sbyte)state.ACC < 0);
        state.NIL = state.ACC == 0;
        base.RegisterOpcode();
    }
}

public class NegInstr : Instruction
{
    public NegInstr(byte[] bytes) : base(bytes, 1)
    {
        AddBehavior(0x54, Utils.ArgumentOrder.Register, RegisterOpcode);
    }

    protected override void RegisterOpcode()
    {
        switch(bytes[1])
        {
            case 0x00:
                state.ACC = unchecked((byte)-(sbyte)state.ACC);
                state.LTZ = unchecked((sbyte)state.ACC < 0);
                state.NIL = state.ACC == 0;
                break;
            case 0x01:
                state.BCC = unchecked((byte)-(sbyte)state.BCC);
                state.LTZ = unchecked((sbyte)state.BCC < 0);
                break;
            default:
                throw new InvalidOperationException("Invalid target register");
        }
        
        base.RegisterOpcode();
    }
}

public class MulInstr : Instruction
{
    public MulInstr(byte[] bytes) : base(bytes, 2)
    {
        AddBehavior(0x58, Utils.ArgumentOrder.Byte, ByteOpcode);
        AddBehavior(0x59, Utils.ArgumentOrder.Register, RegisterOpcode);
    }

    protected override void ByteOpcode()
    {
        ushort result = (ushort)(state.ACC * bytes[1]);
        state.ACC = (byte)(result & 0x00FF);
        state.CRY = (result & 0xFF00) > 0;
        state.LTZ = unchecked((sbyte)state.ACC < 0);
        state.NIL = state.ACC == 0;
        base.ByteOpcode();
    }

    protected override void RegisterOpcode()
    {
        ushort result = bytes[1] switch
        {
            0x00 => (ushort)(state.ACC * state.ACC),
            0x01 => (ushort)(state.ACC * state.BCC),
            _ => throw new InvalidOperationException("Invalid source register")
        };
        state.ACC = (byte)(result & 0x00FF);
        state.CRY = (result & 0xFF00) > 0;
        state.LTZ = unchecked((sbyte)state.ACC < 0);
        state.NIL = state.ACC == 0;
        base.RegisterOpcode();
    }
}

public class DivInstr : Instruction
{
    public DivInstr(byte[] bytes) : base(bytes, 2)
    {
        AddBehavior(0x5B, Utils.ArgumentOrder.Byte, ByteOpcode);
        AddBehavior(0x5C, Utils.ArgumentOrder.Register, RegisterOpcode);
    }

    protected override void ByteOpcode()
    {
        ushort result = unchecked((ushort)(state.ACC / bytes[1]));
        byte quot = unchecked((byte)(state.ACC % bytes[1]));

        state.ACC = (byte)(result & 0x00FF);
        state.BCC = quot;
        state.CRY = (result & 0xFF00) > 0;
        state.LTZ = unchecked((sbyte)state.ACC < 0);
        state.NIL = state.ACC == 0;
        base.ByteOpcode();
    }

    protected override void RegisterOpcode()
    {
        ushort result = bytes[1] switch
        {
            0x00 => unchecked((ushort)(state.ACC / state.ACC)),
            0x01 => unchecked((ushort)(state.ACC / state.BCC)),
            _ => throw new InvalidOperationException("Invalid source register")
        };
        byte quot = bytes[1] switch
        {
            0x00 => unchecked((byte)(state.ACC % state.ACC)),
            0x01 => unchecked((byte)(state.ACC % state.BCC)),
            _ => throw new InvalidOperationException("Invalid source register")
        };

        state.ACC = (byte)(result & 0x00FF);
        state.BCC = quot;
        state.CRY = (result & 0xFF00) > 0;
        state.LTZ = unchecked((sbyte)state.ACC < 0);
        state.NIL = state.ACC == 0;
        base.RegisterOpcode();
    }
}

public class IntInstr : Instruction
{
    public IntInstr(byte[] bytes) : base(bytes, 2)
    {
        AddBehavior(0x60, Utils.ArgumentOrder.None, NoneOpcode);
        AddBehavior(0x61, Utils.ArgumentOrder.Word, WordOpcode);
    }

    protected override void NoneOpcode()
    {
        state.INT = true;
        state.ITA = (ushort)(state.PRC + 1);
        state.PRC = MemoryUtils.GetAddress(state.BCC, state.BAK);
    }

    protected override void WordOpcode()
    {
        state.INT = true;
        state.ITA = state.PRC;
        state.PRC = MemoryUtils.GetAddress(bytes[1], bytes[2]);
    }
}

public class RfiInstr : Instruction
{
    public RfiInstr(byte[] bytes) : base(bytes, 2)
    {
        AddBehavior(0x64, Utils.ArgumentOrder.None, NoneOpcode);
    }

    protected override void NoneOpcode()
    {
        if(state.INT)
        {
            state.PRC = state.ITA;
            state.INT = false;
        }
        else
            base.NoneOpcode();
    }
}

public class EndInstr : Instruction
{
    public EndInstr(byte[] bytes) : base(bytes, 1)
    {
        AddBehavior(0xF0, Utils.ArgumentOrder.None, NoneOpcode);
    }

    protected override void NoneOpcode()
    {
        throw new SapphireException(state, "computer halted", true);
    }
}
