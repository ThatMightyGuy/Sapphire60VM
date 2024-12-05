using System.Text.RegularExpressions;
using System.Reflection;

using JetFly.Sapphire60.Assembler.Tokens;
using JetFly.Sapphire60.Assembler.Common;

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
                TokenBase token(string? x, string? y)
                {
                    if (Activator.CreateInstance(type, x, y) is not TokenBase tmp)
                        throw new MissingMethodException("Report this to the developer. Error word: bonk");
                    return tmp;
                }
                string name = TokenRegex().Match(type.Name).Value;
                tokenFactories.Add(name.ToUpper(), token);
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

    private bool IsInRange(List<(int start, int end)> ranges, int x)
    {
        foreach (var (start, end) in ranges)
            if (start <= x && x <= end)
                return true;
        return false;
    }

    public Assembly Tokenize(string code)
    {
        ushort org = 0;
        ushort pc = 0;
        List<string> lines = Trim(code);
        Dictionary<string, ushort> labels = new();
        List<MemoryToken> tokens = new();
        List<string> defines = new();
        bool ifdef = false;
        bool ignoreLines = false;
        List<(int start, int end)> ignored = new();
        (int start, int end) range = (0, lines.Count);

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
                    case ".define":
                        if(ignoreLines) continue;
                        if(words.Length < 2)
                            throw new PreprocessorException("Invalid syntax", i + 1);
                        else if(words.Length >= 3)
                        {
                            for(int l = i + 1; l < lines.Count; l++)
                            {
                                if(lines[l].StartsWith(';'))
                                    continue;
                                string replace = string.Join(' ', words[2..]);
                                string[] wds = lines[l].Split(' ');
                                bool isString = wds.Length > 1 && wds[1].StartsWith('$');
                                if(isString)
                                    lines[l] = lines[l].Replace("#" + words[1], replace);
                                else
                                    lines[l] = lines[l].Replace(words[1], replace);
                            }
                        }
                        defines.Add(words[1]);
                        continue;
                    case ".ifdef":
                        if(ifdef)
                            throw new PreprocessorException("Invalid usage of ifdef; no multi-level statements allowed", i + 1);
                        ifdef = true;
                        ignoreLines = defines.Contains(words[1]);
                        range.start = i;
                        continue;
                    case ".ifndef":
                        if(ifdef)
                            throw new PreprocessorException("Invalid usage of ifdef; no multi-level statements allowed", i + 1);
                        ifdef = true;
                        ignoreLines = !defines.Contains(words[1]);
                        range.start = i;
                        continue;
                    case ".endif":
                        if(!ifdef)
                            throw new PreprocessorException("No matching if(n)def", i + 1);
                        if(ignoreLines)
                        {
                            range.end = i;
                            ignored.Add(range);
                        }
                        range.start = 0;
                        range.end = lines.Count;
                        ifdef = false;
                        ignoreLines = false;
                        continue;
                    case ".org":
                        if(ignoreLines) continue;
                        bool success = Utils.TryParseLiteral(words[1], true, out int? neworg);
                        if(success && neworg is not null)
                        {
                            org = (ushort)neworg;
                            pc = 0;
                        }
                        else
                            throw new PreprocessorException("Invalid origin address", i + 1);
                        continue;
                    case ".attach":
                        if(ignoreLines) continue;
                        if (words.Length < 2)
                            throw new PreprocessorException("Invalid syntax", i + 1);
                        List<string> before = lines[..i];
                        List<string> attached = Trim(File.ReadAllText(string.Join(' ', words[1..])));
                        List<string> after = lines[(i + 1)..];
                        before.AddRange(attached);
                        before.AddRange(after);
                        lines = before;
                        // Restart preprocessor, probably not great but that's the only way I can think of.
                        return Tokenize(string.Join('\n', lines));
                }
            }
            else if(LabelRegex().IsMatch(line))
            {
                ushort labelAddress = (ushort)(org + pc);
                labels.Add(line[..^1], labelAddress);
                Console.WriteLine($"L {line} [0x{labelAddress:X4}]");
                continue;
            }

            string[] instruction = line.Split(' ');
            
            string? x = instruction.Length >= 2 ? instruction[1] : null;
            string? y = instruction.Length >= 3 ? string.Join(' ', instruction[2..]) : null;

            if(x is not null && x.StartsWith('$'))
                x = string.Join(' ', instruction[1..]);
            pc += Utils.GetInstructionSize(x, y);
        }

        if(ifdef)
            throw new PreprocessorException("No matching endif", lines.Count);

        org = 0;
        pc = 0;

        for(int i = 0; i < lines.Count; i++)
        {
            if(IsInRange(ignored, i))
                continue;
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