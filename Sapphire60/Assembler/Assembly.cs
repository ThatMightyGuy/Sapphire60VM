using System.Text;

namespace JetFly.Sapphire60.Assembler;

public readonly struct Assembly(Dictionary<string, ushort> labels, List<MemoryToken> tokens)
{
    public readonly Dictionary<string, ushort> Labels = labels;
    public readonly List<MemoryToken> Tokens = tokens;

    public byte[] GetBytes()
    {
        byte[] bytes = new byte[ushort.MaxValue];
        foreach(MemoryToken token in Tokens)
            token.Token.GetBytes().CopyTo(bytes, token.Position);
        return bytes;
    }

    public Stream Save()
    {
        byte[] preamble = Encoding.ASCII.GetBytes("S60IMG\xFF\xFF");
        Stream stream = new MemoryStream();
        stream.Write(new ReadOnlySpan<byte>(preamble));
        stream.Write(new ReadOnlySpan<byte>(GetBytes()));
        return stream;
    }
}