namespace NetRecord.Utils.Exceptions;

public class NetRecordException : Exception
{
    internal NetRecordException(string? message = null)
        : base(message) { }

    internal NetRecordException(string? message, Exception e)
        : base(message, e) { }

    internal NetRecordException() { }
}
