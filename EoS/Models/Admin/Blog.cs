using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EoS.Models.Admin
{
    public class Blog
    {
        [Key]
        public int BlogId { get; set; }
        [Required]
        [Display(Name ="Text")]
        public string BlogText { get; set; }

        [Editable(false)]
        [DataType(DataType.DateTime)]
        [Display(Name = "Date")]
        public DateTime BlogDate { get; set; }

        public string CreatorId { get; set; }
        public virtual ApplicationUser Creator { get; set; }

        public virtual ICollection<Shared.BlogComment> BlogComments { get; set; }
    }
}