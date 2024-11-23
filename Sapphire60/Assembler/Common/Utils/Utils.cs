namespace JetFly.Sapphire60.Assembler.Common;

public static partial class Utils
{
    public static Type[] GetDistantRelatives(Type type)
    {
        return type.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && type.IsAssignableFrom(t))
            .ToArray();
    }
}