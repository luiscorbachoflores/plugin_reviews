using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Activity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JellyAsk.Api;

[ApiController]
[Route("JellyAsk")]
[AllowAnonymous]
public class JellyAskController : ControllerBase
{
    private readonly IActivityManager _activityManager;
    private readonly IAuthorizationContext _authorizationContext;

    public JellyAskController(IActivityManager activityManager, IAuthorizationContext authorizationContext)
    {
        _activityManager = activityManager;
        _authorizationContext = authorizationContext;
    }

    [HttpGet("ClientScript")]
    public ActionResult GetClientScript()
    {
        var assembly = typeof(JellyAskController).Assembly;
        using var stream = assembly.GetManifestResourceStream("Jellyfin.Plugin.JellyAsk.wwwroot.jellyask.js");
        if (stream is null)
        {
            return NotFound();
        }

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        Response.Headers.CacheControl = "no-store, must-revalidate";
        Response.Headers.Pragma = "no-cache";
        return Content(content, "application/javascript");
    }

    [HttpPost("Request")]
    public async Task<ActionResult> PostRequest([FromBody] JellyAskRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            return BadRequest("El texto de la petición es obligatorio.");
        }

        var authInfo = await _authorizationContext.GetAuthorizationInfo(Request).ConfigureAwait(false);
        if (!authInfo.IsAuthenticated || authInfo.User is null)
        {
            return Unauthorized("Necesitas iniciar sesión en Jellyfin para pedir un título.");
        }

        var username = authInfo.User.Username;
        var entry = new ActivityLog("Nueva petición de título", "JellyAskRequest", authInfo.UserId)
        {
            ShortOverview = $"Solicitado por {username}",
            Overview = dto.Text.Trim(),
            LogSeverity = LogLevel.Information,
        };

        await _activityManager.CreateAsync(entry).ConfigureAwait(false);
        return Ok();
    }
}

public class JellyAskRequestDto
{
    [Required]
    public string Text { get; set; } = string.Empty;
}
