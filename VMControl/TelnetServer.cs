using System.Net;
using System.Net.Sockets;

namespace JetFly.VMControl;
public abstract class TelnetServer(int port, int width, int height)
{
    protected readonly TcpListener listener = new(IPAddress.Loopback, port);
    protected readonly int width = width;
    protected readonly int height = height;
    protected byte[] framebuffer = new byte[width * height];

    private NetworkStream? stream;
    protected StreamWriter? writer;
    protected StreamReader? reader;

    public async virtual Task StartAsync()
    {
        listener.Start();
        Console.WriteLine("Telnet server listening on port " + listener.LocalEndpoint);

        while (true)
        {
            await Task.Yield();
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Terminal attached");

            stream = client.GetStream();
            writer = new(stream);
            reader = new(stream);

            _ = HandleClientAsync(client);
        }
    }

    protected async virtual Task HandleClientAsync(TcpClient client)
    {
        // clear terminal
        await writer.WriteAsync("\x1B[2J");
        // move cursor to top left
        await writer.WriteAsync("\x1B[H");
        await writer.FlushAsync();

        while (true)
        {   
            async Task<bool> Input()
            {   
                string? input = await reader.ReadLineAsync();
                if (input is null)
                    return false;
                await OnInput(input);
                return true;
            }

            await Task.Yield();
            if(!await Input())
                break;
        }

        client.Close();
    }

    protected async virtual Task OnRender() {}
    protected async virtual Task OnInput(string input) {}


    protected async virtual Task Render()
    {
        if(writer is null)
            return;
        await OnRender();
        await writer.WriteAsync("\x1B[H");
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char c = (char)framebuffer[y * width + x];
                await writer.WriteAsync(c == '\0' ? ' ' : c);
            }
            await writer.WriteAsync("\n");
        }
        await writer.FlushAsync();
    }
}
