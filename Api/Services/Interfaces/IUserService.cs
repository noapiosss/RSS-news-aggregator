namespace Api.Services.Interfaces
{
    public interface IUserService
    {
        bool IsAuthorized(out string username);
    }
}