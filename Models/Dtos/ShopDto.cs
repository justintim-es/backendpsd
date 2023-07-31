
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos;

public class ShopDto {
    [Required(ErrorMessage = "E-Mail is verplicht")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Wachtwoord is verplicht")]
    public string Password { get; set; } = "";
    [Required(ErrorMessage = "KVK-Nummer is verplicht")]
    public string KVKNumber { get; set; } = "";
    [Required(ErrorMessage = "Bedrijfsnaam is verplicht")]
    public string CompanyName { get; set; } = "";
}