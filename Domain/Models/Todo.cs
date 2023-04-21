using System.ComponentModel.DataAnnotations;

namespace challengeApi.Models
{
        public class Todo
        {
            [Required]
            public int Id { get; set; }

            [Required,MaxLength(100)]
            [RegularExpression(@"([a - zA - Z0 - 9\s]+)")]
            public string Title { get; set; }

            [MaxLength(500)]
            public string? Description { get; set; }

            [Required]
            public PriorityEnum Priority { get; set; }
        }
}
