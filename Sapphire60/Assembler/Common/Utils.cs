namespace JetFly.Sapphire60.Assembler.Common;

public static class Utils
{
    public enum Registers
    {
        ACC,
        BCC, BAK,
        PRH, PRL,
        CRY, NIL, LTZ,
        ITH, ITL
    }

    /// <summary>
    /// Defines the order of arguments in an instruction.
    /// `None` is no arguments (`NOP` for example),
    /// `Literal` is an explicitly defined value (`ADD 42`, `ADD 0x01`)
    /// `Register` is a register argument (`ADD BCC`)
    /// Then come double arguments which are just `MOV BCC, 42` etc.
    /// 
    /// I don't think there is a single `LiteralLiteral` instruction however.
    /// Could use some cleaning later.
    /// </summary>

    [Flags]
    public enum ArgumentOrder
    {
        None = 0,
        Literal = 1,
        Register = 2,
        LiteralLiteral = 4,
        RegisterLiteral = 8,
        RegisterRegister = 16
    }

    public static int ParseLiteral(string literal)
    {
        if (literal.EndsWith("b", StringComparison.Ordinal))
            return Convert.ToInt32(literal[..^1], 2);
        else if (literal.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return Convert.ToInt32(literal[2..], 16);
        return int.Parse(literal);
    }

    public static byte ParseRegister(string register)
    {
        Enum.TryParse(typeof(Registers), register, out object? parsed);
        if(parsed is not null)
            return (byte)(Registers)parsed;
        return 0xFF;
    }

    public static string ParseRegister(byte register)
    {
        if(Enum.IsDefined(typeof(Registers), register))
            return ((Registers)register).ToString();
        return "NUL";
    }

    public static bool TryParseLiteral(string? literal, out int? data)
    {
        try
        {
            data = ParseLiteral(literal ?? "NIL");
            return true;
        }
        catch
        {
            data = null;
            return false;
        }
    }

    public static bool TryParseRegister(string? register, out byte? data)
    {
        if(register is not null)
        {
            Enum.TryParse(typeof(Registers), register, out object? parsed);
            if(parsed is not null)
            {
                data = (byte)(Registers)parsed;
                return true;
            }
        }
        data = null;
        return false;
    }

    public static bool TryParseRegister(byte? register, out string? data)
    {
        if(register is not null)
        {
            if(Enum.IsDefined(typeof(Registers), register))
            {
                data = ((Registers)register).ToString();
                return true;
            }
        }
        data = null;
        return false;
    }
}