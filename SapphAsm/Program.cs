using JetFly.Sapphire60.Assembler;
using JetFly.Sapphire60.Assembler.Common;

#if DEBUG
if(BitConverter.IsLittleEndian)
    Console.WriteLine("Platform is little endian");
else
    Console.WriteLine("Platform is big endian");
#endif

Parser parser = new();

string program = File.ReadAllText(args[0]);
Console.WriteLine(args[0]);

Assembly asm;
try
{
    asm = parser.Tokenize(program);
    Console.WriteLine($"T {asm.Tokens.Count}");
}
catch(PreprocessorException ex)
{
    Console.WriteLine("======= PREPROCESS =======");
    Console.WriteLine(ex.Message);
    return;
}
catch(AssemblyException ex)
{
    Console.WriteLine("======== ASSEMBLY ========");
    Console.WriteLine(ex.Message);
    return;
}

FileStream file = File.OpenWrite(args.Length == 2 ? args[1] : "a.s6bin");
foreach(byte b in asm.GetBytes())
    file.WriteByte(b);
file.Close();