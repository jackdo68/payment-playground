namespace PaymentApp.Domain.Exceptions;

public class DuplicateEmailException : DomainException
{
    public string Email { get; }

    public DuplicateEmailException(string email)
        : base("DUPLICATE_EMAIL", $"A user with email '{email}' already exists")
    {
        Email = email;
    }
}