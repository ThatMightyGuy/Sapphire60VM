using JetFly.Sapphire60.Common;

namespace JetFly.VMControl;


public class Program
{
    public static void Main(string[] args)
    {
        if (BitConverter.IsLittleEndian)
            Console.WriteLine("Platform is little endian");
        else
            Console.WriteLine("Platform is big endian");

        Sapphire60.Sapphire60 vm = new(0xFFFF);

        // vm.State.MemoryRead += MemRead;
        // vm.State.MemoryWritten += MemWrite;

        SapphireTelnet telnet = new(vm, 5360);
        _ = telnet.StartAsync();

        byte[] program = File.ReadAllBytes("a.s6bin");
        Console.WriteLine($"Program read, {program.Length} bytes");

        vm.Copy(program, 0);

        const int DELAY = 10;
        int dumpNumber = 0;
        bool cont = true;
        Console.WriteLine("========= INSTR =========");
        bool pass = false;
        bool slowed = false;
        bool tickMode = true;

        void Dump(string? filename = null)
        {
            filename ??= $"dump_{dumpNumber++}.s6img";
            File.WriteAllBytes(filename, vm.Read(0x0000, 0xFFFF));
            Console.WriteLine($"Memory dumped to {filename}");
        }

        // void MemRead(object? sender, MemoryAccessedEventArgs e)
        // {
        //     Console.WriteLine($"Memory read: 0x{e.Address:X4} (value: 0x{e.NewValue})");
        // }
        // void MemWrite(object? sender, MemoryAccessedEventArgs e)
        // {
        //     Console.WriteLine($"Memory written: 0x{e.Address:X4} (value: 0x{e.NewValue})");
        // }

        try
        {
            telnet.CopyFramebuffer();
            while(cont)
            {
                bool choose = true;
                while(!pass && choose)
                {
                    switch(Console.ReadKey().Key)
                    {
                        case ConsoleKey.UpArrow:
                            Console.WriteLine("======= REGISTERS =======");
                            Console.WriteLine(vm.GetRegisters());
                            break;
                        case ConsoleKey.DownArrow:
                            choose = false;
                            tickMode = false;
                            break;
                        case ConsoleKey.RightArrow:
                            choose = false;
                            pass = true;
                            break;
                        case ConsoleKey.LeftArrow:
                            choose = false;
                            pass = true;
                            slowed = true;
                            break;
                        case ConsoleKey.Backspace:
                            Console.WriteLine("Enter address");
                            string text = Console.ReadLine() ?? "0000";
                            ushort addr = Convert.ToUInt16(text, 16);
                            
                            const int LINE_SIZE = 0x10;
                            const int DUMP_SIZE = 0xFF;

                            byte[] dump = vm.Read(addr, DUMP_SIZE);

                            string data = "      ";
                            for(int x = 0; x < LINE_SIZE; x++)
                                data += $"{x:X2} ";
                            
                            Console.WriteLine(data);

                            for(int line = 0; line < DUMP_SIZE / LINE_SIZE; line++)
                            {
                                data = $"{addr + line * LINE_SIZE:X4}: ";
                                for(int x = 0; x < LINE_SIZE; x++)
                                    data += $"{dump[line * LINE_SIZE + x]:X2} ";
                                Console.WriteLine(data);
                            }
                            break;
                        case ConsoleKey.Enter:
                            Dump();
                            break;
                        case ConsoleKey.Escape:
                            cont = false;
                            choose = false;
                            continue;
                    }
                }
                if(tickMode)
                    vm.Tick();
                else
                {
                    vm.Next();
                    tickMode = true;
                }
                telnet.CopyFramebuffer();

                if(slowed)
                    Thread.Sleep(DELAY);
            }
        }
        catch(InvalidOperationException ex)
        {
            Console.WriteLine("========= CRASH =========");
            Console.WriteLine(ex.Message);
            return;
        }
        catch(SapphireException ex)
        {
            Console.WriteLine("======== MACHINE ========");
            Console.WriteLine(ex.Message);
            return;
        }
        finally
        {
            Console.WriteLine("Program exited");
            Dump("dump_halt.s6img");
        }
    }
}