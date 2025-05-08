namespace WalletAPI.Exceptions;

public class UnauthorizeUserAccessException : Exception
{
    public UnauthorizeUserAccessException(string message) : base(message)
    { }
}