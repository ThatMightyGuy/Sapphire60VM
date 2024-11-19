using JetFly.Sapphire60.Assembler.Common;

namespace JetFly.Sapphire60.Assembler.Tokens;

// Idling
public class NopToken(string? x, string? y) : Token(0x00, x, y, 1);

// Memory
public class LdaToken(string? x, string? y) : TokenShort(0x10, 0x11, x, y, 2);
public class StaToken(string? x, string? y) : TokenShort(0x14, 0x15, x, y, 2);
public class StsToken(string? x, string? y) : TokenShort(0x18, 0x19, x, y, 2);

// Branching
public class JmpToken(string? x, string? y) : TokenShort(0xFF, 0x20, x, y, 2);
public class JlzToken(string? x, string? y) : TokenShort(0xFF, 0x24, x, y, 3);
public class JgzToken(string? x, string? y) : TokenShort(0xFF, 0x28, x, y, 3);
public class JezToken(string? x, string? y) : TokenShort(0xFF, 0x2B, x, y, 3);

// Bitwise
public class NotToken(string? x, string? y) : Token(0x30, x, y, 1);
public class AndToken(string? x, string? y) : Token(0x34, x, y, 1);
public class OrToken(string? x, string? y) : Token(0x38, x, y, 1);
public class XorToken(string? x, string? y) : Token(0x3B, x, y, 1);

// Data
/// TODO: MovToken
public class MovToken : TokenByte
{
    private readonly byte[] representation;
    public MovToken(string? x, string? y) : base(0xFF, 0xFF, "0xFF", "0xFF", 1)
    {
        bool ts = Utils.TryParseRegister(x, out byte? target);

        if(!ts || target is null || target > 1)
            throw new LineException("Invalid target register");
        
        representation = [0x40, (byte)target, 0x00];

        bool rs = Utils.TryParseRegister(y, out byte? sourceR);
        bool ls = Utils.TryParseLiteral(y, out int? sourceL);
        if(ls && sourceL is not null)
        {
            representation[2] = (byte)sourceL;
        }
        else if(rs && sourceR is not null)
        {
            if(sourceR > 1)
                throw new LineException("Invalid source register");
            representation[0] = 0x44;
            representation[2] = (byte)sourceR;
        }
        else
            throw new LineException("Invalid (missing?) source");
    }

    public override byte[] GetBytes() => representation;
}
public class SwpToken(string? x, string? y) : Token(0x48, x, y, 1);
public class DupToken(string? x, string? y) : Token(0x4B, x, y, 1);

// Math

// Interrupts

// Execution
public class EndToken(string? x, string? y) : Token(0xF0, x, y, 1);



