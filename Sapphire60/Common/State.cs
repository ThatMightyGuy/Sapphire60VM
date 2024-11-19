namespace JetFly.Sapphire60.Common;

public struct State
{
    // Data
    
    public byte ACC;
    public byte BCC;
    public byte BAK;
    public byte PRH; // Program counter high byte
    public byte PRL; // Program counter low byte
    public byte ITH; // Interrupt return address high byte
    public byte ITL; // Interrupt return address low byte

    // Flags

    public bool CRY; // Carry flag
    public bool NIL; // Is ACC zero?
    public bool LTZ; // Did last arithmetic operation result in a negative number?

    // public byte[] MEMORY;
}