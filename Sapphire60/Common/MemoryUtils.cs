namespace JetFly.Sapphire60.Common;

public static class MemoryUtils
{
    public static ushort GetAddress(byte high, byte low)
    {
        return (ushort)(high << 8 | low);
    }
}