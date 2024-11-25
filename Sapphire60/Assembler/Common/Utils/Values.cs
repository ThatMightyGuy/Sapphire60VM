namespace JetFly.Sapphire60.Assembler.Common;

public static partial class Utils
{
    public static int ParseLiteral(string? literal, bool size = true)
    {
        if(literal is null)
            throw new ArgumentNullException(nameof(literal), "Argument should not be null");
        if (literal.EndsWith("b", StringComparison.Ordinal))
            return Convert.ToInt32(literal[..^1], 2);
        else if (literal.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            int temp = Convert.ToInt32(literal[2..], 16);
            if(temp < 0 || temp > (size ? 0xFFFF : 0xFF))
            {
                string type = size ? "ushort" : "byte";
                throw new ArgumentException($"Value is not a {type}");
            }
            return temp;
        }
        return int.Parse(literal);
    }

    public static bool TryParseLiteral(string? literal, bool size, out int? data)
    {
        try
        {
            data = ParseLiteral(literal ?? "NIL", size);
            return true;
        }
        catch
        {
            data = null;
            return false;
        }
    }

    public static ArgumentOrder FindArgumentOrder(string? x, string? y)
    {
        ArgumentOrder order = ArgumentOrder.None;
        
        if(x is null && y is null)
            return ArgumentOrder.None;

        if(TryParseRegister(x, out _))
            order = ArgumentOrder.Register;
        else if(TryParseLiteral(x, false, out _))
            return ArgumentOrder.Byte;
        else if(TryParseLiteral(x, true, out _))
            return ArgumentOrder.Word;

        if(order is ArgumentOrder.Register)
        {
            if(TryParseRegister(y, out _))
                return ArgumentOrder.RegisterRegister;
            else if(TryParseLiteral(y, false, out _))
                return ArgumentOrder.RegisterByte;
        }        

        if(x is not null)
        {
            if(x.StartsWith('$'))
                return ArgumentOrder.String;
            else
                return ArgumentOrder.Label;
        }
        return ArgumentOrder.Invalid;
    }

    public static ushort GetInstructionSize(string? x, string? y)
    {
        return FindArgumentOrder(x, y) switch
        {
            ArgumentOrder.None => 1,
            ArgumentOrder.Register => 2,
            ArgumentOrder.RegisterByte => 3,
            ArgumentOrder.RegisterRegister => 3,
            ArgumentOrder.Byte => 2,
            ArgumentOrder.Word => 3,
            ArgumentOrder.String => (ushort)(2 + (x ?? "").Length),
            ArgumentOrder.Label => 3,
            _ => 0
        };
    }
}