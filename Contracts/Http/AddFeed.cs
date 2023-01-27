using System.ComponentModel.DataAnnotations;
using Contracts.Database;

namespace Contracts.Http
{
    public class AddFeedRequest
    {
        [Required]
        public string Url { get; init; }
    }

    public class AddFeedResponse
    {
        public Feed Feed { get; set; }
    }
}