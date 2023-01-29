namespace Domain.Helpers.Interfaces
{
    public interface IPasswordHelper
    {
        public string GetSHA256(string password);
    }
}