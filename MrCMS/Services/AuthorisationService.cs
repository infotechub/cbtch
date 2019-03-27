using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using MrCMS.Entities.People;
using System.Linq;

namespace MrCMS.Services
{
    public class AuthorisationService : IAuthorisationService
    {
        private readonly IAuthenticationManager _authenticationManager;
        private readonly UserManager<User, int> _userManager;

        public AuthorisationService(IAuthenticationManager authenticationManager, UserManager<User, int> userManager)
        {
            _authenticationManager = authenticationManager;
            _userManager = userManager;
        }

        public async Task SetAuthCookie(User user, bool rememberMe)
        {
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            ClaimsIdentity identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            _authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = rememberMe }, identity);
        }

        public void Logout()
        {
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

        public async Task UpdateClaimsAsync(User user, IEnumerable<Claim> claims)
        {
            List<Claim> existingClaims = (await _userManager.GetClaimsAsync(user.Id)).ToList();
            IList<Claim> list = claims as IList<Claim> ?? claims.ToList();
            IEnumerable<Claim> newClaims =
                list.Where(claim => existingClaims.All(c => c.Type != claim.Type));
            IEnumerable<Claim> updatedClaims =
                list.Where(claim => existingClaims.Any(c => c.Type == claim.Type && c.Value != claim.Value));
            foreach (Claim newClaim in newClaims)
                await _userManager.AddClaimAsync(user.Id, newClaim);
            foreach (Claim updatedClaim in updatedClaims)
            {
                Claim existing = existingClaims.First(c => c.Type == updatedClaim.Type);
                await _userManager.RemoveClaimAsync(user.Id, existing);
                await _userManager.AddClaimAsync(user.Id, updatedClaim);
            }
        }
    }

    public struct ClaimsComparison : IEquatable<ClaimsComparison>
    {
        public ClaimsComparison(Claim claim)
            : this()
        {
            Issuer = claim.Issuer;
            Type = claim.Type;
            Value = claim.Value;
        }

        public string Issuer { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public bool Equals(ClaimsComparison other)
        {
            return base.Equals(other);
        }
    }
}