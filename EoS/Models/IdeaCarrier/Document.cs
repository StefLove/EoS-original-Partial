using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.IdeaCarrier
{
    public class Document
    {
        [Key]
        public int DocId { get; set; }
        public string DocName { get; set; }
        [Display(Name = "Description")]
        public string DocDescription { get; set; }
        public DateTime DocTimestamp { get; set; }
        public string UserId { get; set; }
        public string StartupID { get; set; }
        public string DocURL { get; set; }


        public virtual ApplicationUser User { get; set; }
        public virtual Startup Startup { get; set; }
    }
}