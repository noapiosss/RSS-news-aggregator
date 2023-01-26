using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Database
{
    [Table("tbl_subscriptions", Schema = "public")]
    public class Subscription
    {
        [Column("user_id")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Column("feed_id")]
        public int FeedId { get; set; }
        public Feed Feed { get; set; }
    }
}