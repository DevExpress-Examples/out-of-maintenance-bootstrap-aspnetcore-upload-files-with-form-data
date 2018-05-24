using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace UploadControlCore.Models {
    public class Portfolio {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [Display(Name="Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Files")]
        public string[] FileNames { get; set; }
    }
}