using System.Security.Claims;

namespace BlazorSSO_MIP.Services
{
    public class ClaimsService
    {
        private List<Claim> _claims = new List<Claim>();

        public void SetClaims(IEnumerable<Claim> claims)
        {
            _claims = new List<Claim>(claims);
        }

        public IEnumerable<Claim> GetClaims()
        {
            return _claims;
        }

        public string GetClaimValue(string claimType)
        {
            return _claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
    }
}
