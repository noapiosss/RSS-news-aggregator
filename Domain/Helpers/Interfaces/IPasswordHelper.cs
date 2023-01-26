namespace Domain.Helpers.Interfaces
{
    public interface IPasswordHelper
    {
        string GetSHA256(string password);
    }
}