using System.ComponentModel.DataAnnotations;

namespace StreetService.Models

{   
    public class CreateStreetDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Geometry { get; set; }
        private const int MaxStreetCapacity = 5000;
        [Range(0, MaxStreetCapacity)]
        public int Capacity { get; set; }
    }
}
