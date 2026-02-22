using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<GetUserByIdQueryResponse>>
{
    private readonly IAuthService _authService;

    public GetUserByIdQueryHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<GetUserByIdQueryResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await _authService.GetUserByIdAsync(request.Id, cancellationToken);
    }
}
