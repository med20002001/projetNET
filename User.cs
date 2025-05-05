using System.ComponentModel.DataAnnotations;

public class User
{
    [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
    [StringLength(50, ErrorMessage = "Le nom d'utilisateur ne peut pas dépasser 50 caractères.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "L'usage est requis.")]
    [StringLength(100, ErrorMessage = "L'usage ne peut pas dépasser 100 caractères.")]
    public string Usage { get; set; }
}
