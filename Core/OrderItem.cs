using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Core;

public class OrderItem
{
    [Key]
    [Required]
    public long OrderItemId { get; set; }
    [Required]
    public long OrderId { get; set; }
    [Required]
    [StringLength(250)]
    public string ItemName { get; set; } = string.Empty;

    #region Navigation Properties

    [JsonIgnore]
    public Order? Order { get; set; }

    #endregion
}