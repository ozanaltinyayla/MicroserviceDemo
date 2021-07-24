using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Web.Models.Catalogs
{
    public class CourseCreateInput
    {
        public string UserId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string Picture { get; set; }

        [Required]
        public string CategoryId { get; set; }

        public FeatureViewModel Feature { get; set; }

        [Display(Name="Course Photo")]
        public IFormFile PhotoFormFile { get; set; }
    }
}
