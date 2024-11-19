using JetFly.Sapphire60.Common;

namespace JetFly.Sapphire60;

public class Sapphire60(uint memorySize)
{
    public readonly State State = new();

    private readonly byte[] memory = new byte[memorySize];

    public void Tick()
    {

    }

    public void Copy(byte[] bytes, uint location)
    {
        if(bytes.Length + location > memory.Length)
            throw new ArgumentOutOfRangeException(nameof(bytes), "Array too big for leftover memory");
        bytes.CopyTo(memory, location);
    }
    
    public void Write(uint location, byte value) => memory[location] = value;

    public byte[] Read(uint from, int count)
    {
        return new ArraySegment<byte>(memory, (int)from, count).ToArray();
    }

    public byte Read(uint from) => memory[from];

}
