namespace HeroEngine.Core.Combat;

/// <summary>
/// Single-responsibility logger that writes combat events to a structured text file.
/// </summary>
public sealed class CombatLogger
{
    private readonly string       _path;
    private readonly List<string> _buffer = new();

    /// <summary>
    /// Creates a logger targeting the given file path.
    /// The parent directory is created if it does not exist.
    /// </summary>
    public CombatLogger(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Log path cannot be empty.", nameof(path));

        _path = path;

        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    /// <summary>Buffers a line to be written on <see cref="Flush"/>.</summary>
    public void Log(string line)
    {
        string stamped = $"[{DateTime.Now:HH:mm:ss}] {line}";
        _buffer.Add(stamped);
    }

    /// <summary>Writes all buffered lines to the log file.</summary>
    public void Flush()
    {
        try
        {
            File.WriteAllLines(_path, _buffer);
            Console.WriteLine($"\n  📋  Battle log saved to: {Path.GetFullPath(_path)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠  Could not write log file: {ex.Message}");
        }
    }
}
