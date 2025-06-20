using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Booking.User.Services
{
    public class UserServiceImpl : UserService.UserServiceBase
    {
        private readonly IUserRepository _repo;
        private readonly ILogger<UserServiceImpl> _logger;

        public UserServiceImpl(ILogger<UserServiceImpl> logger, IUserRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public override async Task<UserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            var user = await _repo.CreateAsync(request.Name, request.Email, request.Pwd) ?? throw new RpcException(new Status(StatusCode.AlreadyExists, "Email já cadastrado"));

            _logger.LogInformation($"Criado usuário {request.Email}");

            return new UserResponse { UserId = user.Id, Name = user.Name, Email = user.Email };
        }

        public override async Task<AuthToken> Authenticate(Credentials request, ServerCallContext context)
        {
            var user = await _repo.GetByEmailAsync(request.Email) ?? throw new RpcException(new Status(StatusCode.NotFound, "Usuário não encontrado"));

            if (!string.Equals(request.Pwd, user.Password))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Credenciais inválidas"));

            _logger.LogInformation($"Autenticado: {request.Email}");

            return new AuthToken { Token = "8ZMh6HAjHyJSjkO9ByGGeZHphxRRtQE57sTSJ7APchdmzbqqrwgTpqITdHWaE9LU", Expires = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()};
        }
    }
}