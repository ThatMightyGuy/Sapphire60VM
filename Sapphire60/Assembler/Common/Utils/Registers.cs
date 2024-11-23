namespace JetFly.Sapphire60.Assembler.Common;

public static partial class Utils
{
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

    public static bool TryParseRegister(string? register, out byte? data)
    {
        if(register is null || !Enum.IsDefined(typeof(Registers), register))
        {
            data = null;
            return false;
        }
        Registers reg = Enum.Parse<Registers>(register);
        if(reg is Registers.ACC || reg is Registers.BCC)
        {
            data = (byte)reg;
            return true;
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