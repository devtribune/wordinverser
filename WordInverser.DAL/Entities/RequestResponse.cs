using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordInverser.DAL.Entities;

[Table("RequestResponse")]
public class RequestResponse
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public Guid RequestId { get; set; }

    [Required]
    public string Request { get; set; } = string.Empty;

    [Required]
    public string Response { get; set; } = string.Empty;

    [Required]
    public string Tags { get; set; } = string.Empty;

    public string? Exception { get; set; }

    [Required]
    public bool IsSuccess { get; set; }

    public DateTime CreatedDate { get; set; }

    public long? ProcessingTimeMs { get; set; }
}
