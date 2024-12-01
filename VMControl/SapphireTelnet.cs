using JetFly.Sapphire60.Common;

namespace JetFly.VMControl;
public class SapphireTelnet : TelnetServer
{
    const int WIDTH = 80;
    const int HEIGHT = 25;
    const ushort VRAM = 0x0880;

    const ushort KBD_QUEUE = 0x1850;
    const ushort KBD_CHAR = 0x1851;
    const ushort KBD_DIR = 0x1852;

    private readonly Sapphire60.Sapphire60 vm;
    private readonly Queue<byte> queue;

    public SapphireTelnet(Sapphire60.Sapphire60 vm, int port) : base(port, WIDTH, HEIGHT)
    {
        this.vm = vm;
        this.vm.State.MemoryWritten += OnMemoryWritten;
        queue = new();
    }

    public async void CopyFramebuffer()
    {
        await Render();
    }

    protected override async Task OnRender()
    {
        framebuffer = vm.Read(VRAM, WIDTH * HEIGHT);
    }

    protected override async Task OnInput(string input)
    {
        if(writer is null)
            return;
        for(int i = 0; i < input.Length; i++)
        {
            if(!QueueInput(input[i]))
                await writer.WriteAsync('\a');
        }
        await base.OnInput(input);
    }

    public bool QueueInput(char c)
    {
        if(queue.Count >= 255)
            return false;
        queue.Enqueue((byte)c);
        return true;
    }

    private void OnMemoryWritten(object? sender, MemoryAccessedEventArgs e)
    {
        if(e.Address == KBD_DIR)
        {
            if(e.NewValue == 0xFF)
            {
                vm.State.MEMORY[KBD_QUEUE] = (byte)queue.Count;
                vm.State.MEMORY[KBD_CHAR] = (queue.Count > 0) ? queue.Dequeue() : (byte)0x00;
                vm.State.MEMORY[KBD_DIR] = 0x00;
            }
        }
    }
}