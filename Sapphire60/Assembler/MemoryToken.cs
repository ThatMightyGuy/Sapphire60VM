using JetFly.Sapphire60.Assembler.Tokens;

namespace JetFly.Sapphire60.Assembler;

public readonly struct MemoryToken(ushort position, ushort size, TokenBase token)
{
    public readonly ushort Position = position;
    public readonly ushort Size = size;
    public readonly TokenBase Token = token;
}