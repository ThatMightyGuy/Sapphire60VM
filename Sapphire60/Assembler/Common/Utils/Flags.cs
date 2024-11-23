namespace JetFly.Sapphire60.Assembler.Common;

public static partial class Utils
{
    public enum Registers
    {
        ACC,
        BCC, BAK,
        PRH, PRL,
        CRY, NIL, LTZ,
        ITH, ITL
    }

    /// <summary>
    /// Defines the order of arguments in an instruction.
    /// `None` is no arguments (`NOP` for example),
    /// Then comes `RegisterByte` which is just `MOV BCC, 42` etc.
    /// </summary>

    [Flags]
    public enum ArgumentOrder
    {
        None = 0,
        Register = 1,
        RegisterByte = 2,
        RegisterRegister = 4,
        Byte = 8,
        Word = 16,
        String = 32,
        Label = 64,
        Invalid = 128
    }

    public class FlagBehavior<T> where T: Enum
    {
        // holy cursed
        private static T Or(T value1, T value2)
        {
            return (T)(object)((int)(object)value1 | (int)(object)value2);
        }

        private readonly Dictionary<T, Action> behaviors = new();

        public bool Add(T flag, Action action) => behaviors.TryAdd(flag, action);

        public bool Remove(T flag) => behaviors.Remove(flag);

        public bool IsDefined(T flag) => behaviors.ContainsKey(flag);

        public void RunAll(T flags)
        {
            foreach (var behavior in behaviors)
                if (flags.HasFlag(behavior.Key))
                    behavior.Value();
        }

        public bool Run(T flag)
        {
            if(!IsDefined(flag)) return false;
            behaviors[flag]();
            return true;
        }

        public T? GetFlags()
        {
            T? flags = default;
            foreach(T flag in behaviors.Keys)
            {
                if(flags is null)
                    flags = flag;
                else
                    flags = Or(flags, flag);
            }
            return flags;
        }
    }
}