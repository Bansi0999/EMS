using System.ComponentModel.DataAnnotations;

namespace EMS.Models
{
    public class Level
    {
        public int LevelId { get; set; }
        [Required, StringLength(50), Display(Name="Level Name")] public string LevelName { get; set; } = string.Empty;
        [StringLength(250)] public string? Description { get; set; }
    }
}
