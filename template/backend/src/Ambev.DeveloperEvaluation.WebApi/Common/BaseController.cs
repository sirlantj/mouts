using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Base controller for the API.
/// Only exposes claim helpers; response wrapping is done explicitly in each action so
/// the generic <see cref="ControllerBase.Ok(object?)"/> overload resolution is not
/// shadowed (which previously caused the ApiResponseWithData double-envelope bug).
/// </summary>
[Route("api/[controller]")]
[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid GetCurrentUserId()
    {
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("User id claim is missing.");

        return Guid.Parse(raw);
    }

    protected string GetCurrentUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value
        ?? throw new InvalidOperationException("User email claim is missing.");
}
