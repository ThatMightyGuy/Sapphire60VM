using JetFly.Sapphire60.Assembler;
using JetFly.Sapphire60.Assembler.Common;

if(BitConverter.IsLittleEndian)
    Console.WriteLine("Platform is little endian");
else
    Console.WriteLine("Platform is big endian");

Parser parser = new();

string program = File.ReadAllText("memorytest.s6");
Console.WriteLine($"Program read, {program.Length} chars");

Assembly asm;
try
{
    asm = parser.Tokenize(program);
    Console.WriteLine($"Tokenized, {asm.Tokens.Count} tokens");
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

FileStream file = File.OpenWrite("a.s6bin");
foreach(byte b in asm.GetBytes())
    file.WriteByte(b);
Console.WriteLine("Saved to a.s6bin");
