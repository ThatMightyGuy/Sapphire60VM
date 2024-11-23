using JetFly.Sapphire60.Assembler;
using JetFly.Sapphire60.Assembler.Tokens;
using JetFly.Sapphire60.Assembler.Common;


void MovTest()
{
    const string dest = "BCC";
    const string src = "42";
    MovToken token = new(dest, src);

    Console.WriteLine($"MOV {dest}, {src}");

    Console.WriteLine($"Order: {token.GetArgumentOrder()}");

    Console.WriteLine($"Run: {token.Run()}");
}

void JmpTest()
{
    const string dest = "TEST";
    JmpToken token = new(dest, null);
    token.SetLabels(new(){{"TEST", 0x00FF}});
    Console.WriteLine($"JMP {dest}");

    Console.WriteLine($"Order: {token.GetArgumentOrder()}");

    Console.WriteLine($"Run: {token.Run()}");
}

void ReflectionTest()
{
    static Type[] GetNonAbstractDerivedTypes(Type baseType)
    {
        return baseType.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))
            .ToArray();
    }

    foreach(Type t in GetNonAbstractDerivedTypes(typeof(TokenBase)))
        Console.WriteLine(t.Name);
}

// JmpTest();
// return;

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
