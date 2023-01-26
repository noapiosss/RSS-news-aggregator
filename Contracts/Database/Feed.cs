using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Database
{
    [Table("tbl_feeds", Schema = "public")]
    public class Feed
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("title")]
        public string Title { get; set; }

        [Column("image")]
        public string Image { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("link")]
        public string Link { get; set; }

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        public ICollection<Subscription> Subscribers { get; set; }
        public ICollection<Post> Posts { get; set; }
    }
}