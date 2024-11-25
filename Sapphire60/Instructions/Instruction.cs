using JetFly.Sapphire60.Common;
using JetFly.Sapphire60.Assembler.Common;

namespace JetFly.Sapphire60.Instructions;

public struct InstructionVariant(byte opcode, Utils.ArgumentOrder order)
{
    public readonly byte Opcode = opcode;
    public readonly Utils.ArgumentOrder Order = order;
}
public abstract class InstructionBase(byte[] bytes, byte cost = 1)
{
    protected byte[] bytes = bytes;
    protected byte cost = cost;
    protected State state = new();

    public virtual bool Process(ref State state)
    {
        this.state = state;
        return true;
    }

    public virtual byte GetCost() => cost;
}

public abstract class Instruction(byte[] bytes, byte cost = 1) : InstructionBase(bytes, cost)
{
    // TODO: Replace ArgumentOrder with opcode byte
    protected readonly Dictionary<byte, Utils.ArgumentOrder> variants = new();
    protected Utils.FlagBehavior<Utils.ArgumentOrder> opcodes = new();

    protected void AddBehavior(byte opcode, Utils.ArgumentOrder order, Action action)
    {
        variants.Add(opcode, order);
        opcodes.Add(order, action);
    }

    protected virtual void NoneOpcode() => state.PRC++;
    protected virtual void RegisterOpcode() => state.PRC += 2;
    protected virtual void RegisterByteOpcode() => state.PRC += 3;
    protected virtual void RegisterRegisterOpcode() => state.PRC += 3;
    protected virtual void ByteOpcode() => state.PRC += 2;
    protected virtual void WordOpcode() => state.PRC += 2;
    protected virtual void StringOpcode() => state.PRC += 2;
    
    // probably unnecessary
    // protected virtual void LabelOpcode() {}

    public override bool Process(ref State state)
    {
        if(!variants.TryGetValue(bytes[0], out Utils.ArgumentOrder order))
            throw new InvalidOperationException("Invalid instruction variant");
        bool success = base.Process(ref state) && opcodes.Run(order);
        if(success)
            state = base.state;
        return success;
    }

    public Utils.ArgumentOrder GetArgumentOrder() => variants[bytes[0]];

    public InstructionVariant[] GetVariants() => variants.Select(x => new InstructionVariant(x.Key, x.Value)).ToArray();

    public bool HasVariant(Utils.ArgumentOrder order) => variants.ContainsValue(order);
}