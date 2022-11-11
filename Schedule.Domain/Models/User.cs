using System.ComponentModel.DataAnnotations;
using Schedule.Domain.Attributes;

namespace Schedule.Domain.Models;

public class User
{
    [Required(ErrorMessage = "Логин не может быть пустым")]
    [MinLength(5, ErrorMessage = "Логин должен иметь длину больше 5 символов")]
    [MaxLength(32, ErrorMessage = "Логин должен иметь длину меньше 32 символов")]
    [ValidationAttributes.IsLoginCorrectAttribute(ErrorMessage = "Логин должен состоять только из латинских букв")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Пароль не может быть пустым")]
    public string Password { get; set; } = string.Empty;
}