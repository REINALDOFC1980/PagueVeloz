// PagueVeloz.Application/DTOs/AccountUpdateDto.cs
using PagueVeloz.Domain.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PagueVeloz.Application.DTOs
{
    public class AccountUpdateDto
    {
        [Required]
        public long Balance { get; set; }

        [Required]
        public long ReservedBalance { get; set; }

        [Required]
        public long CreditLimit { get; set; }

        [Required]
        [RegularExpression("Active|Inactive|Blocked",
            ErrorMessage = "Status deve ser Active, Inactive ou Blocked")]

        public string Status { get; set; } = "Active";
    }
}
