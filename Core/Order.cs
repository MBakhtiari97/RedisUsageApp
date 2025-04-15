using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Core;

public class Order
{
    [Key]
    [Required]
    public long OrderId { get; set; }
    [Required]
    public DateTime OrderDateTime { get; set; }
    [Required]
    [StringLength(250)]
    public string BuyPersonName { get; set; } = string.Empty;
    [Required]
    [StringLength(50)]
    public string BuyPersonPhoneNumber { get; set; } = string.Empty;

    #region Navigation Properties

    [JsonIgnore]
    public ICollection<OrderItem>? OrderItems { get; set; }

    #endregion
}