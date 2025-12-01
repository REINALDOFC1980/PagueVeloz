using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagueVeloz.Application.DTOs
{
    public class AccountRequestDto
    {
        public long Balance { get; set; }
        public long ReservedBalance { get; set; }
        public long CreditLimit { get; set; }
    }
}
