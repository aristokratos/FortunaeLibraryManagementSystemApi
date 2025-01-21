using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortunaeLibraryManagementSystem.Service.DTOs
{
    public class UpdateBookDTO
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Genre { get; set; }
        public string? ISBN { get; set; }
        public bool? IsAvailable { get; set; }
        public IFormFile? Image { get; set; }
    }
}
