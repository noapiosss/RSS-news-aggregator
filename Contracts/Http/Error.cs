namespace Contracts.Http
{
    public enum ErrorCode
    {
        BadRequest = 40000,
        Unauthorized = 40100,
        WrongPassword = 40301,
        UserNotFound = 40401,
        FeedNotFound = 40402,
        PostNotFound = 40403,
        SubscriptionNotFound = 40404,
        UsernameIsAlreadyInUse = 40901,
        EmailIsAlreadyInUse = 40902,
        InternalServerError = 50000
    }

    public class ErrorResponse
    {
        public ErrorCode Code { get; init; }
        public string Message { get; init; }
    }
}