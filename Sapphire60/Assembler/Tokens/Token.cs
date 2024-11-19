using JetFly.Sapphire60.Assembler.Common;

namespace JetFly.Sapphire60.Assembler.Tokens;

public abstract class Token
{
    private readonly byte opcode, cost;
    public Token(byte opcode, string? x, string? y, byte cost = 1)
    {
        if(x is not null || y is not null)
            throw new LineException("Invalid instruction");
        this.opcode = opcode;
        this.cost = cost;
    }
    public virtual byte[] GetBytes() => [opcode];

    public virtual int GetCost() => cost;
}

public abstract class TokenShort : Token
{
    private readonly byte[] representation;
    public TokenShort(byte opBase, byte opFull, string? x, string? y, byte cost = 1) : base(0x00, null, null, cost)
    {
        if(x is null)
        {
            representation = [opBase];
        }
        else
        {
            ushort addr = (ushort)Utils.ParseLiteral(x);

            byte[] addrBytes = BitConverter.GetBytes(addr);
            if(BitConverter.IsLittleEndian)
                representation = [opFull, addrBytes[1], addrBytes[0]];
            else
                representation = [opFull, addrBytes[0], addrBytes[1]];
        }
    }
    
    public override byte[] GetBytes() => representation;
}
public abstract class TokenByte : Token
{
    private readonly byte[] representation;
    public TokenByte(byte opBase, byte opFull, string? x, string? y, byte cost = 1) : base(0x00, null, null, cost)
    {
        if(x is null || y is null)
        {
            representation = [opBase];
        }
        else
        {
            byte xb = (byte)Utils.ParseLiteral(x);
            byte yb = (byte)Utils.ParseLiteral(y);
            representation = [opFull, xb, yb];
        }
    }

    public override byte[] GetBytes() => representation;

}