using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces
{
    public interface IAppUserService
    {
        Task<Dictionary<string, string>> GetUserNamesByIdsAsync(List<string> userIds, CancellationToken cancellationToken = default);
    }
}
