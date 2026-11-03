using System.Text.Json;
using PotdCounter.Core;

// Sample states only. Export the real presentation model without loading the game.
if (args.Length != 1) throw new ArgumentException("provide the output JSON path");
var states = new (double Start, CounterDisplay State)[]
{
    (0, new(1, 0, KnownState.Off, KnownState.Off)),
    (1.8, new(1, 1, KnownState.Off, KnownState.Off)),
    (3.3, new(1, 2, KnownState.Off, KnownState.Off)),
    (5, new(173, 4, KnownState.Off, KnownState.On)),
    (8, new(174, 0, KnownState.On, KnownState.Off)),
    (9.2, new(174, 1, KnownState.On, KnownState.Off)),
    (10.1, new(174, 2, KnownState.On, KnownState.Off)),
    (11, new(174, 3, KnownState.On, KnownState.Off, Open: true)),
    (12.5, new(180, 0, KnownState.Off, KnownState.Off)),
    (14.5, new(180, 0, KnownState.Off, KnownState.Off, InCombat: true)),
    (16.5, new(180, 0, KnownState.Off, KnownState.Off, Open: true,
        BossDefeated: true, CelebrateVictory: true)),
    (20.5, new(180, 0, KnownState.Off, KnownState.Off, Open: true, BossDefeated: true)),
};
var output = new
{
    duration = 23,
    states = states.Select((item, i) => new
    {
        start = item.Start,
        end = i + 1 < states.Length ? states[i + 1].Start : 23,
        floor = item.State.Floor,
        hidden = item.State.HideForBossCombat,
        view = item.State.Presentation,
    }),
};
File.WriteAllText(args[0], JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine("exported 12 simulated states from the counter presentation model");
