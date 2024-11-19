namespace JetFly.Sapphire60.Assembler.Common;

public class AssemblyException : Exception
{
    public AssemblyException(string message, int line) : base($"L{line}: {message}") {}
    public AssemblyException(string message, int line, LineException innerException) : base($"L{line}: {message}", innerException) {}
}

public class LineException(string message) : Exception(message);

public class PreprocessorException(string message, int line) : AssemblyException($"preprocess: {message}", line);