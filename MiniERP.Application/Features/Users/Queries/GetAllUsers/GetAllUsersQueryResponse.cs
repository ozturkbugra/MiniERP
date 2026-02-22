using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Users.Queries.GetAllUsers
{
    public sealed record GetAllUsersQueryResponse(
     string Id,
     string FirstName,
     string LastName,
     string Email,
     string UserName,
     bool IsDeleted);
}
