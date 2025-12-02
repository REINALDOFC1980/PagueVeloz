using PagueVeloz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagueVeloz.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionModel> ProcessTransactionAsync(TransactionModel dto);
    }
}
