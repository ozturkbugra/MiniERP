using MediatR;

namespace MiniERP.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
{
    // buraya Dependency Injection ile IAuthService (Kimlik Servisimiz) gelecek.

    public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Şimdilik sadece MediatR'ın çalıştığını test etmek için sahte bir mesaj dönüyoruz.
        return $"{request.FirstName} {request.LastName} kullanıcısı için MediatR başarıyla tetiklendi.";
    }
}