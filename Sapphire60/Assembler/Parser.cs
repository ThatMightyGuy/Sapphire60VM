using System.Text.RegularExpressions;
using System.Reflection;

using JetFly.Sapphire60.Assembler.Tokens;
using JetFly.Sapphire60.Assembler.Common;

namespace JetFly.Sapphire60.Assembler;

public partial class Parser
{
    [GeneratedRegex("[a-z0-9_]+:", RegexOptions.IgnoreCase)]
    private static partial Regex LabelRegex();
    Dictionary<string, Func<string?, string?, Token>> tokenFactories;

    public Parser()
    {
        tokenFactories = new();
        // tokenFactories.Add("NOP", (x, y) => new NopToken(x, y));

        foreach (Type? type in typeof(Token).Assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(Token)) && !type.IsAbstract)
            {
                // Console.WriteLine(type.Name);
                ConstructorInfo? constructor = type.GetConstructor([typeof(string), typeof(string)]);
                if (constructor is not null)
                    tokenFactories.Add(type.Name[..3].ToUpper(), (x, y) => (Token)Activator.CreateInstance(type, x, y));
            }
        }

        Console.WriteLine($"Picked up {tokenFactories.Count} tokens of class {nameof(Token)}");
        #if DEBUG
        foreach(string key in tokenFactories.Keys)
            Console.WriteLine(key);
        #endif
    }

    private Token ParseLine(string line)
    {
        string?[] words = [null, null, null, null];
        line.Split(' ').CopyTo(words, 0);

        if(words[0] is null)
            throw new LineException("Invalid instruction (no opcode)");
        int wordCount;
        for(wordCount = 0; wordCount < words.Length; wordCount++)
            if(words[wordCount] is null) break;
        if(wordCount > 3)
            throw new LineException($"Invalid instruction (too many arguments)");

        try
        {
            if(tokenFactories.TryGetValue(words[0], out var tokenFactory))
                if(words[2] is not null)
                    return tokenFactory(words[1]?[..^1], words[2]);
                else
                    return tokenFactory(words[1], null);
        }
        catch(TargetInvocationException ex)
        {
            if(ex.InnerException is LineException)
                throw new LineException($"({words[0]}) {ex.InnerException.Message}");
            throw;
        }
        throw new LineException("Unknown opcode");
    }

    private static List<string> Trim(string code)
    {
        return code.Split('\n').Select(line => line.Trim()).ToList();
    }

    public Assembly Tokenize(string code)
    {
        ushort org = 0;
        ushort pc = 0;
        List<string> lines = Trim(code);
        Dictionary<string, ushort> labels = new();
        List<MemoryToken> tokens = new();

        for(int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if(line.Length == 0)
                continue;
            if(line.StartsWith(';'))
                continue;
            if(line.StartsWith('.'))
            {
                string[] words = line.Split(' ');
                switch(words[0])
                {
                    case ".org":
                        bool success = Utils.TryParseLiteral(words[1], out int? neworg);
                        if(success && neworg is not null)
                        {
                            org = (ushort)neworg;
                            pc = 0;
                        }
                        else
                            throw new PreprocessorException("Invalid origin address", i);
                        break;
                }
                continue;
            }
            else if(LabelRegex().IsMatch(line))
            {
                labels.Add(line[..^1], (ushort)(org + pc));
                Console.WriteLine($"L {line} [0x{(org + pc):X4}]");
                continue;
            }

            string[] instruction = line.Split(' ');
            switch(instruction.Length)
            {
                case 1:
                    pc++;
                    break;
                case 2:
                    string arg = instruction[1];
                    bool sr = Utils.TryParseRegister(arg, out _);
                    if(sr)
                        pc += 2;
                    else
                    {
                        bool sl = Utils.TryParseLiteral(arg, out int? val);
                        if(sl && val > 255)
                            pc += 3;
                        else
                            // This is ambiguous.
                            pc += 2;
                    }
                    break;
                case 3:
                    pc += 3;
                    break;
            }
        }

        org = 0;
        pc = 0;
        for(int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if(line.Length == 0)
                continue;
            if(line.StartsWith(';'))
                continue;
            if(LabelRegex().IsMatch(line))
            {
                continue;
            }
            if(line.StartsWith('.'))
            {
                string[] words = line.Split(' ');
                switch(words[0])
                {
                    case ".org":
                        bool success = Utils.TryParseLiteral(words[1], out int? neworg);
                        if(success && neworg is not null)
                        {
                            org = (ushort)neworg;
                            pc = 0;
                            Console.WriteLine($"O 0x{org:X4}");
                        }
                        else
                            throw new PreprocessorException("Invalid origin address", i);
                        break;
                }
                continue;
            }

            if(line.StartsWith('J'))
            {
                string[] words = line.Split(' ');
                words[1] = $"0x{labels[words[1]]:X4}";
                line = string.Join(' ', words);
            }
            
            try
            {
                Token token = ParseLine(line);
                ushort size = (ushort)token.GetBytes().Length;
                tokens.Add(new((ushort)(org + pc), size, token));
                pc += size;
            }
            catch(LineException ex)
            {
                throw new AssemblyException($"{ex.Message} [{line}]", i);
            }
        }

        return new Assembly(labels, tokens);
    }
}