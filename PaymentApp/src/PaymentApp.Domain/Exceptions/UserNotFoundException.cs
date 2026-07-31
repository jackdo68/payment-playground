namespace PaymentApp.Domain.Exceptions;

public class UserNotFoundException : DomainException
{
    public int UserId { get; }

    public UserNotFoundException(int userId)
        : base("USER_NOT_FOUND", $"User with ID {userId} was not found")
    {
        UserId = userId;
    }

    public UserNotFoundException(string email)
        : base("USER_NOT_FOUND", $"User with email '{email}' was not found")
    {
    }
}