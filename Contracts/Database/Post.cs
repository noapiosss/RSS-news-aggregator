using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Database
{
    [Table("tbl_posts", Schema = "public")]
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("link")]
        public string Link { get; set; }

        [Column("publication_date")]
        public DateTime PubDate { get; set; }
        public Feed Author { get; set; }
        public ICollection<ReadPost> ReadBy { get; set; }
    }
}