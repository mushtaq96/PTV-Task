using Newtonsoft.Json;

namespace StreetService.Models
{
    public class Street
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Geometry { get; set; } // wkt format
        public int Capacity { get; set; }
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}