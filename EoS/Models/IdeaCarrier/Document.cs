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

        [Display(Name = "Name")]
        public string DocName { get; set; }

        [Display(Name = "Description")]
        public string DocDescription { get; set; }

        [Display(Name = "Time stamp")]
        public DateTime DocTimestamp { get; set; }

        [Display(Name = "URL")]
        public string DocURL { get; set; }

        public string UserId { get; set; } //<---Remove?
        public virtual ApplicationUser User { get; set; } //<---Remove?

        public string StartupID { get; set; }
        public virtual Startup Startup { get; set; }
    }

    public class DocumentUploadViewModel
    {
        [Display(Name = "Upload a document file")]
        public HttpPostedFileBase DocFile { get; set;}

        [Display(Name = "Description")]
        public string DocDescription { get; set; }

        public string StartupID { get; set; }
    }
}