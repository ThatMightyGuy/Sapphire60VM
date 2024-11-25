using JetFly.Sapphire60.Assembler.Common;

namespace JetFly.Sapphire60.Assembler.Tokens;

// Idling
public class NopToken(string? x, string? y) : Token(x, y, 0x00, 1);

// Memory
public class LdaToken(string? x, string? y) : TokenNoneOrWord(x, y, 0x10, 0x11, 2);
public class StaToken(string? x, string? y) : TokenNoneOrWord(x, y, 0x14, 0x15, 2);
public class StsToken(string? x, string? y) : TokenString(x, y, 0x18, 8);

// Branching
public class JmpToken(string? x, string? y) : TokenLabel(x, y, 0x20, 2);
public class JlzToken(string? x, string? y) : TokenLabel(x, y, 0x24, 3);
public class JgzToken(string? x, string? y) : TokenLabel(x, y, 0x28, 3);
public class JezToken(string? x, string? y) : TokenLabel(x, y, 0x2B, 3);

// Bitwise
public class NotToken(string? x, string? y) : Token(x, y, 0x30, 1);
public class AndToken(string? x, string? y) : Token(x, y, 0x34, 1);
public class OrToken(string? x, string? y) : Token(x, y, 0x38, 1);
public class XorToken(string? x, string? y) : Token(x, y, 0x3B, 1);

// Data
public class MovToken : TokenBase
{
    const byte byt = 0x40;
    const byte reg = 0x41;

    public MovToken(string? x, string? y) : base(x, y, 1)
    {
        AddBehavior(Utils.ArgumentOrder.RegisterByte, OpcodeRegisterByte);
        AddBehavior(Utils.ArgumentOrder.RegisterRegister, OpcodeRegisterRegister);
    }

    protected virtual void OpcodeRegisterByte()
    {
        representation = [byt, 0, 0];

        if(x is null)
            throw new LineException("Missing target register");
        if(Utils.TryParseRegister(x, out byte? tgt) && tgt is not null)
            representation[1] = (byte)tgt;
        else
            throw new LineException("Invalid target register");

        if(y is null)
            throw new LineException("Missing source byte");
        if(Utils.TryParseLiteral(y, false, out int? src) && src is not null)
            representation[2] = (byte)src;
        else
            throw new LineException("Invalid byte literal");
    }

    protected virtual void OpcodeRegisterRegister()
    {
        representation = [reg, 0, 0];

        if(x is null)
            throw new LineException("Missing target register");
        if(Utils.TryParseRegister(x, out byte? tgt) && tgt is not null)
            representation[1] = (byte)tgt;
        else
            throw new LineException("Invalid target register");

        if(y is null)
            throw new LineException("Missing source register");
        if(Utils.TryParseRegister(y, out byte? src) && src is not null)
            representation[2] = (byte)src;
        else
            throw new LineException("Invalid source register");
    }
}
public class SwpToken(string? x, string? y) : Token(x, y, 0x44, 1);
public class DupToken(string? x, string? y) : Token(x, y, 0x48, 1);

// Math
public class AddToken(string? x, string? y) : TokenByteOrRegister(x, y, 0x50, 0x51, 1);
public class NegToken(string? x, string? y) : TokenRegister(x, y, 0x54, 1);
public class MulToken(string? x, string? y) : TokenByteOrRegister(x, y, 0x58, 0x59, 2);
public class DivToken(string? x, string? y) : TokenByteOrRegister(x, y, 0x5B, 0x5C, 2);

// Interrupts
public class IntToken : Token
{
    const byte word = 0x61;

    public IntToken(string? x, string? y) : base(x, y, 0x60, 2)
    {
        AddBehavior(Utils.ArgumentOrder.Word, OpcodeWord);
    }

    protected virtual void OpcodeWord()
    {
        if(x is null)
            throw new LineException("Missing argument");
        if(Utils.TryParseLiteral(x, false, out int? data) && data is not null)
            representation = [word, (byte)data];
        else
            throw new LineException("Invalid byte literal");
    }
}
public class RfiToken(string? x, string? y) : Token(x, y, 0x64, 2);

// Execution
public class EndToken(string? x, string? y) : Token(x, y, 0xF0, 1);



