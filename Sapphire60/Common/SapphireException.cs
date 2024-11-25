namespace JetFly.Sapphire60.Common;

public class SapphireException(State state, string message, bool expected = false) : Exception($"0x{state.PRC:X4}: {message} [" + (expected ? "" : "un") + "expected]");