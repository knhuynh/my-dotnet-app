namespace MyApp.API.Authorization;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

public class UpdateUserAuthorizationHandler : AuthorizationHandler<UpdateUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateUserRequirement requirement)
    {
        // Admin được cập nhật mọi tài khoản
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Lấy ID người đang đăng nhập từ JWT Claims
        var currentUserId = context.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        // Resource chính là ID tài khoản cần cập nhật
        if (int.TryParse(currentUserId, out var userId)
            && context.Resource is int targetUserId
            && userId == targetUserId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}