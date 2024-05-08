using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NFile = System.IO.File;

namespace LoggingServer.Controllers;

/// <summary>
/// Represents the type of log message.
/// </summary>
public enum LogType
{
    /// <summary>
    /// Represents the debug log type.
    /// </summary>
    Debug,

    /// <summary>
    /// Represents the information log type.
    /// </summary>
    Info,

    /// <summary>
    /// Enum member indicating a warning log type.
    /// </summary>
    Warning,

    /// <summary>
    /// Represents an error message in the log.
    /// </summary>
    Error,

    /// <summary>
    /// Represents a log type indicating a fatal error.
    /// </summary>
    Fatal
}

/// <summary>
/// Represents a log message.
/// </summary>
public record LogMessage(string message);

/// <summary>
/// Represents a logger for logging messages on the server.
/// </summary>
[ApiController]
[Route("/{host}")]
public class Logger : ControllerBase
{
    /// <summary>
    /// Retrieves the list of log files for a given host.
    /// </summary>
    /// <param name="host">The host name.</param>
    /// <param name="date">Optional. The date for which to retrieve log files. Defaults to the current date.</param>
    /// <returns>An <see cref="IActionResult"/> containing a list of log files with their names and sizes.</returns>
    [HttpGet]
    public IActionResult Get([FromRoute] string host, [FromQuery] DateTime? date = null)
    {
        var connection = Request.HttpContext.Connection;
        string user = Request.HttpContext.Connection.RemoteIpAddress is not { } ipAddress ? "system" : ipAddress.ToString();
        string[] files = Directory.GetFiles(GetLoggingDirectory(user, host, date ?? DateTime.Now), "*.log", SearchOption.AllDirectories);

        return Ok(files.Select(file => new { name = Path.GetFileName(file), size = NFile.ReadAllBytes(file).Length }));
    }

    /// <summary>
    /// Retrieves the list of log files for a given host and date.
    /// </summary>
    /// <param name="host">The host name.</param>
    /// <param name="type">The type of log message to retrieve.</param>
    /// <param name="date">Optional. The date for which to retrieve log files. If not specified, uses the current date.</param>
    /// <returns>A list of log files with their names and sizes.</returns>
    /// <remarks>
    /// This method retrieves the log files for the specified host and date based on the type of log message.
    /// If the log file for the specified type does not exist, a "Log file not found" error is returned.
    /// Otherwise, the log file is returned as a physical file with the "text/plain" content type.
    /// </remarks>
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


    /// <summary>
    /// Logs a message to a log file.
    /// </summary>
    /// <param name="host">The host name or identifier.</param>
    /// <param name="type">The type of log message (Debug, Info, Warning, Error, Fatal).</param>
    /// <param name="message">The log message to be logged.</param>
    /// <returns>Returns an IActionResult object representing the result of the log operation.</returns>
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

    /// <summary>
    /// Retrieves the logging directory for a specific user, host, and date.
    /// </summary>
    /// <param name="user">The user for which to retrieve the logging directory.</param>
    /// <param name="host">The host for which to retrieve the logging directory.</param>
    /// <param name="date">The date for which to retrieve the logging directory. If not specified, the current date will be used.</param>
    /// <returns>The full path of the logging directory.</returns>
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