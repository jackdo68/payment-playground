namespace PaymentApp.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string File { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public decimal Balance { get; set; }

    public DateTime CreatedAt { get; set; }

}