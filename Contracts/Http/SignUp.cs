using System.ComponentModel.DataAnnotations;

namespace Contracts.Http
{
    public class SignUpRequest
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; init; }

        [Required]
        public string Email { get; init; }

        [Required]
        public string Password { get; init; }
    }

    public class SignUpResponse
    {
        public bool RegistrationIsSuccessful { get; set; }
    }
}