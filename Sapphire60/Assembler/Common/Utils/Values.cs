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
}