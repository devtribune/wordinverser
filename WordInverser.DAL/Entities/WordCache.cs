using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordInverser.DAL.Entities;

[Table("WordCache")]
public class WordCache
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Word { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string InversedWord { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
