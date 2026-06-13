namespace ApiAggregator.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(string username);
}