using Dapper;
using Microsoft.Extensions.Logging;

namespace Vantus.Engine.Services;

public class ActionLogService
{
    private readonly DatabaseService _db;
    private readonly ILogger<ActionLogService> _logger;

    public ActionLogService(DatabaseService db, ILogger<ActionLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LogActionAsync(string filePath, string actionType, string description, string status = "Success")
    {
        try
        {
            using var conn = _db.GetConnection();
            await conn.ExecuteAsync(
                "INSERT INTO action_log (file_path, action_type, description, timestamp, status) VALUES (@FilePath, @ActionType, @Description, @Timestamp, @Status)",
                new
                {
                    FilePath = filePath,
                    ActionType = actionType,
                    Description = description,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Status = status
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log action");
        }
    }

    public async Task UndoLastActionAsync()
    {
        try
        {
            using var conn = _db.GetConnection();
            var lastAction = await conn.QueryFirstOrDefaultAsync<dynamic>("SELECT * FROM action_log WHERE status = 'Success' ORDER BY id DESC LIMIT 1");

            if (lastAction == null) return;

            string filePath = lastAction.file_path;
            string actionType = lastAction.action_type;
            string description = lastAction.description; // Description often contains details "-> Dest"

            bool success = false;

            if (actionType.Equals("Move", StringComparison.OrdinalIgnoreCase))
            {
                // Description format: "Applied rule '...' (Move) to Source -> Dest"
                // We need structured data really, but parsing description is the "simple" way for this patch without schema migration
                // Ideally we add 'original_path' and 'new_path' columns.
                // For now, let's assume we can't easily undo without structured data in the patch scope unless we parse.
                // Let's rely on string parsing for this "feature-fill".

                var parts = description.Split("->");
                if (parts.Length == 2)
                {
                    var dest = parts[1].Trim();
                    var source = filePath; // In the log, file_path is usually the source or the object of action.

                    if (File.Exists(dest) && !File.Exists(source))
                    {
                        File.Move(dest, source);
                        success = true;
                        _logger.LogInformation("Undid Move: {Dest} -> {Source}", dest, source);
                    }
                }
            }
            else if (actionType.Equals("Copy", StringComparison.OrdinalIgnoreCase))
            {
                var parts = description.Split("->");
                if (parts.Length == 2)
                {
                    var dest = parts[1].Trim();
                    if (File.Exists(dest))
                    {
                        File.Delete(dest);
                        success = true;
                        _logger.LogInformation("Undid Copy: Deleted {Dest}", dest);
                    }
                }
            }

            if (success)
            {
                await conn.ExecuteAsync("UPDATE action_log SET status = 'Undone' WHERE id = @Id", new { Id = (int)lastAction.id });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to undo last action");
        }
    }
}
