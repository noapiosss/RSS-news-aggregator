using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Database
{
    [Table("tbl_read_posts")]
    public class ReadPost
    {
        [Column("user_id")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Column("post_id")]
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}