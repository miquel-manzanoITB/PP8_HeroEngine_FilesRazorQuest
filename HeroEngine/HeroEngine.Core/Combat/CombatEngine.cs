namespace HeroEngine.Core.Combat;

/// <summary>
/// The polymorphic combat engine. Manages turn-based rounds between any
/// collection of heroes and enemies, using <see cref="ICombatant"/> throughout
/// so no if/switch on concrete types is ever needed.
/// </summary>
public sealed class CombatEngine
{
    private readonly List<ICombatant>  _heroes;
    private readonly List<ICombatant>  _enemies;
    private readonly CombatLogger      _logger;
    private readonly CombatHelper      _helper;
    private          int               _round;

    /// <summary>
    /// Creates a new combat engine.
    /// </summary>
    /// <param name="heroes">The hero-side combatants.</param>
    /// <param name="enemies">The enemy-side combatants.</param>
    /// <param name="logPath">Optional file path for the battle log.</param>
    public CombatEngine(
        IEnumerable<ICombatant> heroes,
        IEnumerable<ICombatant> enemies,
        string logPath = "logs/battle.log")
    {
        _heroes  = heroes?.ToList()  ?? throw new ArgumentNullException(nameof(heroes));
        _enemies = enemies?.ToList() ?? throw new ArgumentNullException(nameof(enemies));

        if (_heroes.Count == 0)  throw new ArgumentException("At least one hero required.",  nameof(heroes));
        if (_enemies.Count == 0) throw new ArgumentException("At least one enemy required.", nameof(enemies));

        _logger = new CombatLogger(logPath);
        _helper = new CombatHelper();
        _round  = 0;
    }

    // ─── Public entry point ───────────────────────────────────────────────────

    /// <summary>
    /// Runs the full battle until all heroes or all enemies are defeated.
    /// </summary>
    public void Run()
    {
        _logger.Log("=== BATTLE START ===");
        Console.WriteLine("\n╔══════════════════════════════════════════╗");
        Console.WriteLine("║          BATTLE COMMENCES!               ║");
        Console.WriteLine("╚══════════════════════════════════════════╝\n");

        while (!BattleOver())
        {
            _round++;
            ExecuteRound();
        }

        AnnounceResult();
        _logger.Log("=== BATTLE END ===");
        _logger.Flush();

        // Print stats summary
        _helper.PrintStats();
    }

    // ─── Round execution ──────────────────────────────────────────────────────

    private void ExecuteRound()
    {
        var header = $"BATTLE LOG - Round {_round}";
        Console.WriteLine($"\n{'=',1}{new string('=', 48)}");
        Console.WriteLine($"  {header}");
        Console.WriteLine($"{'=',1}{new string('=', 48)}");

        _logger.Log(new string('=', 50));
        _logger.Log($" {header}");
        _logger.Log(new string('=', 50));

        // Build turn order: all alive combatants sorted by initiative (descending)
        var turnOrder = ActiveHeroes()
            .Concat(ActiveEnemies())
            .OrderByDescending(c => c.Initiative)
            .ToList();

        foreach (var attacker in turnOrder)
        {
            if (attacker.IsDefeated) continue;

            // Determine target pool (opposite side)
            var targets = IsHeroSide(attacker) ? ActiveEnemies() : ActiveHeroes();

            if (targets.Count == 0) break;

            // Pick target with lowest HP (greedy AI)
            var target = targets.MinBy(t => GetHp(t))!;

            int damage = attacker.Attack();
            int net    = target.ReceiveDamage(damage);

            string side     = IsHeroSide(attacker) ? "[HERO] " : "[ENEMY]";
            string defeated = target.IsDefeated ? " | DEFEATED!" : "";
            string logLine  = $"  {side} {attacker.Name,-12} -> {target.Name,-14} -> {net} dmg{defeated}";

            Console.WriteLine(logLine);
            _logger.Log(logLine);

            _helper.RecordAction(attacker.Name, net, _round, target.IsDefeated);
        }

        PrintRoundSummary();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private List<ICombatant> ActiveHeroes()  => _heroes.Where(h  => !h.IsDefeated).ToList();
    private List<ICombatant> ActiveEnemies() => _enemies.Where(e => !e.IsDefeated).ToList();

    private bool BattleOver() => ActiveHeroes().Count == 0 || ActiveEnemies().Count == 0;

    private bool IsHeroSide(ICombatant c) => _heroes.Contains(c);

    private static int GetHp(ICombatant c)
    {
        // Inspect via pattern matching — no if/switch needed
        return c switch
        {
            HeroCombatant hc => hc.UnderlyingHero.CurrentHp,
            Enemy e          => e.CurrentHp,
            _                => int.MaxValue
        };
    }

    private void PrintRoundSummary()
    {
        string summary = $"\n  Remaining enemies: {ActiveEnemies().Count} | " +
                         $"Heroes standing: {ActiveHeroes().Count}";
        Console.WriteLine(summary);
        Console.WriteLine(new string('=', 50));
        _logger.Log(summary);
    }

    private void AnnounceResult()
    {
        Console.WriteLine();
        if (ActiveHeroes().Count > 0)
        {
            Console.WriteLine("  🏆  HEROES TRIUMPH! The Bug Primordial is defeated!");
        }
        else
        {
            Console.WriteLine("  💀  HEROES FALL! The Bug Primordial consumes Bytecroft...");
        }
        Console.WriteLine();
    }
}
