using System.Reflection;
using System.Text;

using JetFly.Sapphire60.Common;
using JetFly.Sapphire60.Instructions;
using JetFly.Sapphire60.Assembler.Common;

namespace JetFly.Sapphire60;

public partial class Sapphire60
{
    public State State;

    private readonly Dictionary<byte, Func<byte[], Instruction>> instructionFactories;
    private byte cyclesLeft;

    public const int MAX_STACK = byte.MaxValue;

    public Sapphire60(uint memorySize)
    {
        State = new State{NIL = true, MEMORY = new byte[memorySize], STACK = new()};
        cyclesLeft = 0;

        instructionFactories = new();

        foreach (Type type in Utils.GetDistantRelatives(typeof(Instruction)))
        {
            ConstructorInfo? constructor = type.GetConstructor([typeof(byte[])]);
            if (constructor is not null)
            {
                Instruction instruction(byte[] x)
                {
                    if (Activator.CreateInstance(type, x) is not Instruction tmp)
                        throw new MissingMethodException("Report this to the developer. Error word: squeak");
                    return tmp;
                }

                Instruction instr = instruction([]);
                foreach(InstructionVariant variant in instr.GetVariants())
                    instructionFactories.Add(variant.Opcode, instruction);
            }
        }

        #if TRACE && SILENCE
        Console.WriteLine($"Picked up {instructionFactories.Count} instructions");
        #endif
    }

    public bool Tick()
    {
        if(cyclesLeft > 0)
        {
            cyclesLeft--;
            return false;
        }

        ushort addr = State.PRC;
        byte[] bytes = State.MEMORY[addr..Math.Min(addr + 258, State.MEMORY.Length)];
        Instruction instr;
        try
        {
            instr = instructionFactories[State.MEMORY[addr]](bytes);
        }
        catch(KeyNotFoundException)
        {
            throw new SapphireException(State, "invalid opcode");
        }
        Console.WriteLine($"0x{State.PRC:X4}: [{instr.GetType().Name}, {instr.GetArgumentOrder()}] 0x{bytes[1]:X2}, 0x{bytes[2]:X2}");
        cyclesLeft = instr.GetCost();
        instr.Process(ref State);
        return true;
    }

    public void Next()
    {
        while(!Tick()) {}
    }

    public void Copy(byte[] bytes, uint location)
    {
        if(bytes.Length + location > State.MEMORY.Length)
            throw new ArgumentOutOfRangeException(nameof(bytes), "Array too big for leftover memory");
        bytes.CopyTo(State.MEMORY, location);
    }

    public void Write(uint location, byte value) => State.MEMORY[location] = value;

    public byte[] Read(uint from, int count)
    {
        // return new ArraySegment<byte>(State.MEMORY, (int)from, count).ToArray();
        return State.MEMORY[(int)from..((int)from+count)];
    }

    public byte Read(uint from) => State.MEMORY[from];

    public string GetRegisters()
    {
        static string GetByte(byte register, string name) => $"{name.ToUpper()}:\t[{register:X2}]\tUINT({register}) SIGN({unchecked((sbyte)register)})";
        static string GetWord(ushort register, string name) => $"{name.ToUpper()}:\t[{register:X4}]\tUINT({register}) SIGN({unchecked((short)register)})";
        static string GetFlag(bool flag, string name) => $"{name.ToUpper()}:\t[{flag}]";

        const string SEPARATOR = "-------------------------"; 

        StringBuilder str = new();

        str.AppendLine(GetByte(State.ACC, nameof(State.ACC)));
        str.AppendLine(GetByte(State.BCC, nameof(State.BCC)));
        str.AppendLine(GetByte(State.BAK, nameof(State.BAK)));
        str.AppendLine(SEPARATOR);
        str.AppendLine(GetWord(State.PRC, nameof(State.PRC)));
        str.AppendLine(GetWord(State.ITA, nameof(State.ITA)));
        str.AppendLine(SEPARATOR);
        str.AppendLine(GetFlag(State.CRY, nameof(State.CRY)));
        str.AppendLine(GetFlag(State.NIL, nameof(State.NIL)));
        str.AppendLine(GetFlag(State.LTZ, nameof(State.LTZ)));
        str.AppendLine(GetFlag(State.INT, nameof(State.INT)));
        str.AppendLine(SEPARATOR);
        byte[] stack = State.STACK.ToArray();
        str.AppendLine($"Stack [{stack.Length}]:");
        for(int i = stack.Length - 1; i >= 0; i--)
            str.AppendLine($"{stack.Length - i}:\t[{stack[i]:X2}]\tUINT({stack[i]}) SIGN({unchecked((sbyte)stack[i])})");

        return str.ToString();
    }

    
}
