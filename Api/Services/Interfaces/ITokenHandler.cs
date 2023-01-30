namespace Api.Services.Interfaces
{
    public interface ITokenHandler
    {
        string GenerateToken(string username);
        bool Validate(string token, out string username);
    }
}