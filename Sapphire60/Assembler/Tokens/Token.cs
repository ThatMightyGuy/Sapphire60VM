using JetFly.Sapphire60.Assembler.Common;

namespace JetFly.Sapphire60.Assembler.Tokens;

public abstract class TokenBase
{
    protected byte cost;
    protected byte[] representation;
    protected readonly Utils.FlagBehavior<Utils.ArgumentOrder> flagBehavior;
    protected readonly string? x, y;
    protected Dictionary<string, ushort> labels;

    public TokenBase(string? x, string? y, byte cost = 1)
    {
        this.cost = cost;
        this.x = x;
        this.y = y;
        representation = [];
        flagBehavior = new();
        labels = new();
    }

    public virtual byte[] GetBytes()
    {
        // flagBehavior.Run(someflag)
        return representation;
    }

    public virtual int GetCost() => cost;

    // This is pretty dumb, but I think this is the best option for now.
    public virtual Utils.ArgumentOrder GetArgumentOrder()
    {
        Utils.ArgumentOrder order = Utils.ArgumentOrder.None;
        
        if(x is null && y is null)
            return Utils.ArgumentOrder.None;

        if(Utils.TryParseRegister(x, out _))
            order = Utils.ArgumentOrder.Register;
        else if(Utils.TryParseLiteral(x, false, out _))
            return Utils.ArgumentOrder.Byte;
        else if(Utils.TryParseLiteral(x, true, out _))
            return Utils.ArgumentOrder.Word;

        if(order is Utils.ArgumentOrder.Register)
        {
            if(Utils.TryParseRegister(y, out _))
                return Utils.ArgumentOrder.RegisterRegister;
            else if(Utils.TryParseLiteral(y, false, out _))
                return Utils.ArgumentOrder.RegisterByte;
        }        

        if(x is not null)
        {
            if(x.StartsWith('$'))
                return Utils.ArgumentOrder.String;
            else
                return Utils.ArgumentOrder.Label;
        }
        return Utils.ArgumentOrder.Invalid;
    }

    public virtual void SetLabels(Dictionary<string, ushort> labels) => this.labels = labels;
    public virtual bool Run()
    {
        Utils.ArgumentOrder order = GetArgumentOrder();
        if(order is Utils.ArgumentOrder.Invalid)
            return false;
        return flagBehavior.Run(order);
    }
    public virtual Utils.ArgumentOrder GetArgumentOrders() => flagBehavior.GetFlags();

    protected virtual bool AddBehavior(Utils.ArgumentOrder flag, Action action) => flagBehavior.Add(flag, action);
    protected virtual bool RemoveBehavior(Utils.ArgumentOrder flag) => flagBehavior.Remove(flag);
    protected virtual bool HasBehavior(Utils.ArgumentOrder flag) => flagBehavior.IsDefined(flag);
}

public abstract class Token : TokenBase
{
    protected byte none;

    public Token(string? x, string? y, byte opcode, byte cost) : base(x, y, cost)
    {
        none = opcode;
        AddBehavior(Utils.ArgumentOrder.None, () => representation = [opcode]);
    }

    protected virtual void OpcodeNone()
    {
        representation = [none];
    }
}

public abstract class TokenNoneOrWord : Token
{
    protected byte word;

    public TokenNoneOrWord(string? x, string? y, byte none, byte word, byte cost = 1) : base(x, y, none, cost)
    {
        this.word = word;
        AddBehavior(Utils.ArgumentOrder.Byte, OpcodeWord);
        AddBehavior(Utils.ArgumentOrder.Word, OpcodeWord);
    }

    protected virtual void OpcodeWord()
    {
        if(x is null)
            throw new LineException("Missing argument");
        ushort literal = (ushort)Utils.ParseLiteral(x, true);
        byte[] bytes = BitConverter.GetBytes(literal);
        if(BitConverter.IsLittleEndian)
            representation = [word, bytes[1], bytes[0]];
        else
            representation = [word, bytes[0], bytes[1]];
    }
}

public abstract class TokenWord : TokenBase
{
    protected byte word;

    public TokenWord(string? x, string? y, byte word, byte cost = 1) : base(x, y, cost)
    {
        this.word = word;
        AddBehavior(Utils.ArgumentOrder.Byte, OpcodeWord);
        AddBehavior(Utils.ArgumentOrder.Word, OpcodeWord);
    }

    protected virtual void OpcodeWord()
    {
        if(x is null)
            throw new LineException("Missing argument");
        ushort literal = (ushort)Utils.ParseLiteral(x, true);
        byte[] bytes = BitConverter.GetBytes(literal);
        if(BitConverter.IsLittleEndian)
            representation = [word, bytes[1], bytes[0]];
        else
            representation = [word, bytes[0], bytes[1]];
    }
}

public abstract class TokenByteOrRegister : TokenBase
{
    protected byte byt, reg;

    public TokenByteOrRegister(string? x, string? y, byte byt, byte reg, byte cost = 1) : base(x, y, cost)
    {
        this.byt = byt;
        this.reg = reg;
        AddBehavior(Utils.ArgumentOrder.Byte, OpcodeByte);
        AddBehavior(Utils.ArgumentOrder.Register, OpcodeRegister);
    }

    protected virtual void OpcodeByte()
    {
        if(x is null)
            throw new LineException("Missing argument");
        if(Utils.TryParseLiteral(x, false, out int? data) && data is not null)
            representation = [byt, (byte)data];
        else
            throw new LineException("Invalid byte literal");
    }

    protected virtual void OpcodeRegister()
    {
        if(x is null)
            throw new LineException("Missing argument");
        if(Utils.TryParseRegister(x, out byte? data) && data is not null)
            representation = [reg, (byte)data];
        else
            throw new LineException("Invalid register");
    }
}

public abstract class TokenRegister : TokenBase
{
    protected byte none, reg;

    public TokenRegister(string? x, string? y, byte reg, byte cost = 1) : base(x, y, cost)
    {
        this.reg = reg;
        AddBehavior(Utils.ArgumentOrder.Register, OpcodeRegister);
    }

    protected virtual void OpcodeRegister()
    {
        if(x is null)
            throw new LineException("Missing argument");
        if(Utils.TryParseRegister(x, out byte? data) && data is not null)
            representation = [reg, (byte)data];
        else
            throw new LineException("Invalid register");
    }
}

public abstract class TokenString : TokenBase
{
    protected byte str;

    public TokenString(string? x, string? y, byte str, byte cost = 1) : base(x, y, cost)
    {
        this.str = str;
        AddBehavior(Utils.ArgumentOrder.String, OpcodeString);
    }

    protected virtual void OpcodeString()
    {
        representation = [str];
    }
}

public abstract class TokenLabel : TokenBase
{
    // if(line.StartsWith('J'))
    // {
    //     string[] words = line.Split(' ');
    //     words[1] = $"0x{labels[words[1]]:X4}";
    //     line = string.Join(' ', words);
    // }
    protected byte label;

    public TokenLabel(string? x, string? y, byte label, byte cost = 1) : base(x, y, cost)
    {
        this.label = label;
        AddBehavior(Utils.ArgumentOrder.Label, OpcodeLabel);
    }

    protected virtual void OpcodeLabel()
    {
        if(x is null)
            throw new LineException("Missing jump label");
        if(labels.TryGetValue(x, out ushort addr))
        {
            byte[] bytes = BitConverter.GetBytes(addr);
            if(BitConverter.IsLittleEndian)
                representation = [label, bytes[1], bytes[0]];
            else
                representation = [label, bytes[0], bytes[1]];
        }
        else
            throw new LineException("Invalid jump label");
    }
}