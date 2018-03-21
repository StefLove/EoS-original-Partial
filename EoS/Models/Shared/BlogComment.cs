using System;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Shared
{
    public class BlogComment
    {
        [Key]
        public int BlogCommentId { get; set; }
        [Required]
        [Display(Name = "Comment")]
        public string BlogCommentText { get; set; }

        [Editable(false)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Date")]
        public DateTime BlogCommentDate { get; set; }

        public string CreatorId { get; set; }
        public virtual ApplicationUser Creator { get; set; }

        public int BlogId { get; set; }
        public virtual Admin.Blog Blog { get; set; }
    }
}