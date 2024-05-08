using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NFile = System.IO.File;

namespace LoggingServer.Controllers;

public enum LogType
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public record LogMessage(string message);

[ApiController]
[Route("/{host}")]
public class Logger : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromRoute] string host, [FromQuery] DateTime? date = null)
    {
        var connection = Request.HttpContext.Connection;
        string user = Request.HttpContext.Connection.RemoteIpAddress is not { } ipAddress ? "system" : ipAddress.ToString();
        string[] files = Directory.GetFiles(GetLoggingDirectory(user, host, date ?? DateTime.Now), "*.log", SearchOption.AllDirectories);

        return Ok(files.Select(file => new { name = Path.GetFileName(file), size = NFile.ReadAllBytes(file).Length }));
    }

    [HttpGet("{type}")]
    public IActionResult Get([FromRoute] string host, [FromRoute] LogType type, [FromQuery] DateTime? date = null)
    {
        string user = Request.HttpContext.Connection.RemoteIpAddress is not { } ipAddress ? "system" : ipAddress.ToString();
        string logFile = Path.Combine(GetLoggingDirectory(user, host, date ?? DateTime.Now), $"{type:G}.log");
        if (!NFile.Exists(logFile))
        {
            return NotFound(new { error = "Log file not found" });
        }

        return PhysicalFile(logFile, "text/plain");
    }


    [HttpPost("{type}")]
    public IActionResult Log([FromRoute] string host, [FromRoute] LogType type, [FromBody] LogMessage message)
    {
        string user = Request.HttpContext.Connection.RemoteIpAddress is not { } ipAddress ? "system" : ipAddress.ToString();
        string logFile = Path.Combine(GetLoggingDirectory(user, host, DateTime.Now), $"{type:G}.log");
        try
        {
            if (NFile.Exists(logFile))
            {
                NFile.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss}]: {message.message}\n");
                return Ok(new { message = "File Appended" });
            }

            NFile.WriteAllText(logFile, $"[{DateTime.Now:HH:mm:ss}]: {message.message}\n");
            return Ok(new { message = "File Created" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static string GetLoggingDirectory(string user, string host, DateTime date)
    {
        if (user.Equals("::1"))
        {
            user = "localhost";
        }

        string path = Path.Combine(AppContext.BaseDirectory, "logs", host, user, date.ToString("MM-dd-yyyy"));
        path = string.Join("_", path.Split(Path.GetInvalidPathChars()));
        return Directory.CreateDirectory(path).FullName;
    }
}