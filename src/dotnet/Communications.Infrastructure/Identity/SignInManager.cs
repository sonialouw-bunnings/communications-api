using Microsoft.AspNetCore.Identity;
using Communications.Application.Common.Interfaces;
using Communications.Application.Common.Models;

namespace Communications.Infrastructure.Identity
{
    public class SignInManager : ISignInManager
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SignInManager(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<Result> PasswordSignInAsync(string username, string password, bool isPersistent, bool LockoutOnFailiure)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent, LockoutOnFailiure);

            if (result.IsLockedOut)
            {
                return Result.Failure(new string[] { "Account Locked, too many invalid login attempts." });
            }

            return result.MapToResult();
        }
    }
}