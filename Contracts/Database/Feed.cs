using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Database
{
    [Table("tbl_feeds")]
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

        [Required]
        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("link")]
        public string Link { get; set; }

        [Column("author")]
        public string Author { get; set; }

        [Column("language")]
        public string Language { get; set; }

        [Column("copyright")]
        public string Copyright { get; set; }

        [Column("category")]
        public string Category { get; set; }

        [Column("generator")]
        public string Generator { get; set; }

        [Column("docs")]
        public string Docs { get; set; }

        [Column("ttl")]
        public string TTL { get; set; }

        [Column("image")]
        public string Image { get; set; }

        [Column("text_input_title")]
        public string TextInputTitle { get; set; }

        [Column("text_input_description")]
        public string TextInputDescription { get; set; }

        [Column("text_input_name")]
        public string TextInputName { get; set; }

        [Column("text_input_link")]
        public string TextInputLink { get; set; }

        [Column("skip_hours")]
        public string SkipHours { get; set; }

        [Column("skip_days")]
        public string SkipDays { get; set; }

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        public ICollection<Subscription> Subscribers { get; set; }
        public ICollection<Post> Posts { get; set; }
    }
}