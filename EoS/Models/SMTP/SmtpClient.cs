using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EoS.Models.SMTP
{
    public class SmtpClient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name ="Mail Recipient")]
        public string MailRecipient { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Credential Mail Address")]
        public string CredentialUserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Credential Password")]
        public string CredentialPassword { get; set; }

        [Required]
        [Display(Name = "Host")]
        public string Host { get; set; }

        [Required]
        [Display(Name = "Port")]
        public int Port { get; set; }

        [Required]
        [Display(Name = "Enable SSL")]
        public bool EnableSsl { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool Active { get; set; }
    }
}