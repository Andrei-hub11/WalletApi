namespace WalletAPI.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static NotFoundException Create<T>(object id) =>
        new($"Entity of type {typeof(T).Name} with id {id} was not found.");
}