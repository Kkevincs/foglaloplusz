namespace Foglaloplusz.BLL.DTOs.UserDTOs;

/// <summary>Profil frissítés (vendég / tulajdonos / szakember): név, e-mail, opcionális jelszócsere.</summary>
public class UserProfileUpdateDTO
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Csak szakember: rövid bemutatkozás.</summary>
    public string? Description { get; set; }

    /// <summary>Jelenlegi jelszó — kötelező, ha az <see cref="NewPassword"/> ki van töltve.</summary>
    public string? CurrentPassword { get; set; }

    /// <summary>Új jelszó; üres = változatlan.</summary>
    public string? NewPassword { get; set; }
}
