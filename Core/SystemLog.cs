using System.ComponentModel.DataAnnotations;

namespace Core;

public class SystemLog
{
    [Key]
    [Required]
    public int LogId { get; set; }
    [Required]
    [StringLength(10, ErrorMessage = "{0} is too long")]
    public string LogSerial { get; set; } = string.Empty;
    [Required]
    [StringLength(250, ErrorMessage = "{0} is too long")]
    public string Description { get; set; } = string.Empty;
    [Required]
    public DateTime LogDateTime { get; set; } = DateTime.Now;
    [Required]
    public int AppUserId { get; set; }
    [Required]
    public bool Deleted { get; set; } = false;

    #region Navigation Properties

    public AppUser? AppUser { get; set; }

    #endregion
}