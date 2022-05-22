using Microsoft.AspNetCore.Identity;
using Communications.Application.Common.Models;

namespace Communications.Infrastructure.Identity
{
    public static class IdentityResultExtensions
    {
        public static Result MapToResult(this IdentityResult result)
        {
            return result.Succeeded
                ? Result.Success()
                : Result.Failure(result.Errors.Select(e => e.Description));
        }

        public static Result MapToResult(this SignInResult result)
        {
            return result.Succeeded
                ? Result.Success()
                : Result.Failure(new string[] { "Invalid Login Attempt." });
        }
    }
}
