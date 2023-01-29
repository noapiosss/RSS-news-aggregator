using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Database
{
    [Table("tbl_posts")]
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("publication_date")]
        public DateTime PubDate { get; set; }

        [Column("category")]
        public string Category { get; set; }

        [Column("guid")]
        public string GUID { get; set; }

        [Column("source")]
        public string Source { get; set; }

        [Column("link")]
        public string Link { get; set; }

        [Column("feed_id")]
        public int FeedId { get; set; }
        public Feed Author { get; set; }
        public ICollection<ReadPost> ReadBy { get; set; }
    }
}