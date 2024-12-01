namespace JetFly.Sapphire60.Common;

public class MemoryAccessedEventArgs(ushort address, byte newValue) : EventArgs
{
    public readonly ushort Address = address;
    public readonly byte NewValue = newValue;
}

// public delegate void MemoryAccessedEventHandler(object sender, MemoryAccessedEventArgs e);
public delegate void MemoryReadEventHandler(object sender, MemoryAccessedEventArgs e);
public delegate void MemoryWrittenEventHandler(object sender, MemoryAccessedEventArgs e);
