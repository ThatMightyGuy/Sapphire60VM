using JetFly.Sapphire60.Assembler.Tokens;

namespace JetFly.Sapphire60.Assembler;

public readonly struct MemoryToken(ushort position, ushort size, Token token)
{
    public readonly ushort Position = position;
    public readonly ushort Size = size;
    public readonly Token Token = token;
}