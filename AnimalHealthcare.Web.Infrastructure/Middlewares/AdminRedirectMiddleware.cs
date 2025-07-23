using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AnimalHealthcare.Web.Infrastructure.Middlewares
{
    using static AnimalHealthcare.GCommon.ValidationConstants.UserProfile;
    public class AdminRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminRedirectMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, UserManager<IdentityUser> userManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null && await userManager.IsInRoleAsync(user, AdminRole))
                {
                    // Only redirect if the current path is exactly "/"
                    if (context.Request.Path == "/")
                    {
                        context.Response.Redirect("/Admin/UserManagement/Index");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
