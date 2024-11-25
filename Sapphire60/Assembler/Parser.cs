using System.Text.RegularExpressions;
using System.Reflection;

using JetFly.Sapphire60.Assembler.Tokens;
using JetFly.Sapphire60.Assembler.Common;
using System.Net;

namespace JetFly.Sapphire60.Assembler;

public partial class Parser
{
    [GeneratedRegex("[a-z0-9_]+:", RegexOptions.IgnoreCase)]
    private static partial Regex LabelRegex();

    [GeneratedRegex("^[A-Z][a-z]*")]
    private static partial Regex TokenRegex();
    Dictionary<string, Func<string?, string?, TokenBase>> tokenFactories;

    public Parser()
    {
        tokenFactories = new();

        foreach (Type type in Utils.GetDistantRelatives(typeof(TokenBase)))
        {
            ConstructorInfo? constructor = type.GetConstructor([typeof(string), typeof(string)]);
            if (constructor is not null)
            {
                string name = TokenRegex().Match(type.Name).Value;
                tokenFactories.Add(name.ToUpper(), (x, y) => {
                    TokenBase? tmp = (TokenBase?)Activator.CreateInstance(type, x, y);
                    if(tmp is null)
                        throw new MissingMethodException("Report this to the developer. Error word: bonk");
                    return tmp;
                });
            }
        }

        #if TRACE && SILENCE
        Console.WriteLine($"Picked up {tokenFactories.Count} instructions");
        foreach(string key in tokenFactories.Keys)
            Console.WriteLine(key);
        #endif
    }

    private TokenBase ParseLine(string line)
    {
        string?[] args = [null, null, null, null];
        string[] words = line.Split(' ');
        for(int i = 0; i < Math.Min(args.Length, words.Length); i++)
        {
            if(words[i].StartsWith('$'))
            {
                words[i] = string.Join(' ', words[i..]);
                args[i] = words[i];
                break;
            }
            args[i] = words[i];
        }

        if(args[0] is null)
            throw new LineException("Invalid instruction (no opcode)");
        int wordCount;
        for(wordCount = 0; wordCount < args.Length; wordCount++)
            if(args[wordCount] is null) break;
        if(wordCount > 3)
            throw new LineException($"Invalid instruction (too many arguments)");

        try
        {
            if(tokenFactories.TryGetValue(args[0] ?? "", out var tokenFactory))
                if(args[2] is not null)
                    return tokenFactory(args[1]?[..^1], args[2]);
                else
                    return tokenFactory(args[1], null);
        }
        catch(TargetInvocationException ex)
        {
            if(ex.InnerException is LineException)
                throw new LineException(ex.InnerException.Message);
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
                        bool success = Utils.TryParseLiteral(words[1], true, out int? neworg);
                        if(success && neworg is not null)
                        {
                            org = (ushort)neworg;
                            pc = 0;
                        }
                        else
                            throw new PreprocessorException("Invalid origin address", i + 1);
                        break;
                }
                continue;
            }
            else if(LabelRegex().IsMatch(line))
            {
                ushort labelAddress = (ushort)(org + pc);
                labels.Add(line[..^1], labelAddress);
                Console.WriteLine($"L {line} [0x{labelAddress:X4}]");
                continue;
            }

            string[] instruction = line.Split(' ');
            
            // switch(instruction.Length)
            // {
            //     case 1:
            //         pc++;
            //         break;
            //     case 2:
            //         string arg = instruction[1];
            //         bool sr = Utils.TryParseRegister(arg, out _);
            //         if(sr)
            //             pc += 2;
            //         else
            //         {
            //             bool sl = Utils.TryParseLiteral(arg, false, out _);
            //             if(sl)
            //                 pc += 3;
            //             else if(Utils.TryParseLiteral(arg, true, out _))
            //                 // This is ambiguous because it could be either a byte (+2) or a word (+3).
            //                 // I don't see a reliable way of detecting this.
            //                 // So far it's a word (labels had me do this), so I'm keeping it as is.
            //                 pc += 3;
            //         }
            //         break;
            //     case 3:
            //         pc += 3;
            //         break;
            // }
            
            string? x = instruction.Length >= 2 ? instruction[1] : null;
            string? y = instruction.Length >= 3 ? string.Join(' ', instruction[2..]) : null;

            pc += Utils.GetInstructionSize(x, y);
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
                continue;
            if(line.StartsWith('.'))
            {
                string[] words = line.Split(' ');
                switch(words[0])
                {
                    case ".org":
                        bool success = Utils.TryParseLiteral(words[1], true, out int? neworg);
                        if(success && neworg is not null)
                        {
                            org = (ushort)neworg;
                            pc = 0;
                            Console.WriteLine($"O 0x{org:X4}");
                        }
                        else
                            throw new PreprocessorException("Invalid origin address", i + 1);
                        break;
                }
                continue;
            }

            try
            {
                TokenBase token = ParseLine(line);
                token.SetLabels(labels);
                if(!token.Run())
                    throw new AssemblyException($"Invalid instruction variant: [{line}]", i + 1);

                ushort size = (ushort)token.GetBytes().Length;
                tokens.Add(new((ushort)(org + pc), size, token));
                pc += size;
            }
            catch(LineException ex)
            {
                throw new AssemblyException($"{ex.Message} [{line}]", i + 1);
            }
        }

        return new Assembly(labels, tokens);
    }
}