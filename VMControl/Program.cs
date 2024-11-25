using JetFly.Sapphire60;
using JetFly.Sapphire60.Common;

if (BitConverter.IsLittleEndian)
    Console.WriteLine("Platform is little endian");
else
    Console.WriteLine("Platform is big endian");

Sapphire60 vm = new(0xFFFF);

byte[] program = File.ReadAllBytes("a.s6bin");
Console.WriteLine($"Program read, {program.Length} bytes");

vm.Copy(program, 0);

const int DELAY = 1000;
int dumpNumber = 0;
bool cont = true;
Console.WriteLine("========= INSTR =========");
bool pass = false;
bool slowed = false;

void Dump(string? filename = null)
{
    filename ??= $"dump_{dumpNumber++}.s6img";
    File.WriteAllBytes(filename, vm.Read(0x0000, 0xFFFF));
    Console.WriteLine($"Memory dumped to {filename}");
}

try
{
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
        vm.Next();
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
