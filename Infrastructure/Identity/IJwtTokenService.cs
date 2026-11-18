namespace Infrastructure.Identity;

internal interface IJwtTokenService
{
    string CreateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    (string Raw, string Hash) CreateRefreshToken();
    string Hash(string raw);
}
