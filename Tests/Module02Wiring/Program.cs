using Modules.Module02_RackInstallation.Flow.Validation;
using Modules.Module02_RackInstallation.Domain;
using System;
using Shared.Cabling;
using Modules.Module02_RackInstallation.Flow;

static class Program
{
    static int checks;
    static void Equal(int expected, int actual, string reason)
    {
        checks++;
        if (expected != actual) throw new Exception($"{reason}: expected {expected}, got {actual}");
    }
    static void Main()
    {
        var origins = new[] { "PDU-A/AC01", "PP-A/01", "PP-A/02", "PP-A/03", "FW1/eth01" };
        var destinations = new[] { "SW1/Power", "SW1/Gi01", "SW1/Gi02", "SW1/Gi03", "SW1/Gi04" };
        Equal(5, Module02ConnectionPlan.Links.Count, "Exactly five requirements");
        for (int i = 0; i < 5; i++)
        {
            var requiredKind = i == 0 ? NetworkPortKind.Power : NetworkPortKind.EthernetRj45;
            int Match(string start, string end) => Module02ConnectionPlan.FindMatch(start, end, requiredKind, requiredKind, requiredKind);
            Equal(i, Match(origins[i], destinations[i]), "Valid forward connection");
            Equal(i, Match(destinations[i], origins[i]), "Swapped cable ends");
            Equal(i, Match(origins[i].ToLowerInvariant(), destinations[i].ToLowerInvariant()), "Case-insensitive IDs");
            Equal(-1, Match(origins[i], null), "Disconnect destination");
            Equal(-1, Match(null, destinations[i]), "Disconnect origin");
            Equal(i, Match(origins[i], destinations[i]), "Reconnect restores match");
            Equal(-1, Match(origins[i], origins[i]), "Same endpoint");
            Equal(-1, Match(origins[i], "SW1/Console"), "Console excluded from five connections");
            for (int j = 0; j < 5; j++)
                Equal(i == j ? i : -1, Match(origins[i], destinations[j]), "Fixed destination assignment");
            foreach (NetworkPortKind cable in Enum.GetValues<NetworkPortKind>())
                foreach (NetworkPortKind start in Enum.GetValues<NetworkPortKind>())
                    foreach (NetworkPortKind end in Enum.GetValues<NetworkPortKind>())
                        Equal(cable == requiredKind && start == requiredKind && end == requiredKind ? i : -1,
                            Module02ConnectionPlan.FindMatch(origins[i], destinations[i], cable, start, end), "Cable and BOTH ports must match family");
        }
        var power = new SwitchPowerState();
        power.Toggle(); Equal(0, power.IsOn ? 1 : 0, "Cannot power on without supply");
        power.SetSupply(true); Equal(0, power.IsOn ? 1 : 0, "Connecting supply does not automatically turn on");
        power.Toggle(); Equal(1, power.IsOn ? 1 : 0, "Button turns on with supply");
        power.Toggle(); Equal(0, power.IsOn ? 1 : 0, "Button turns off");
        power.Toggle(); power.SetSupply(false); Equal(0, power.IsOn ? 1 : 0, "Removing supply turns off");
        power.SetSupply(true); Equal(0, power.IsOn ? 1 : 0, "Supply restoration requires button again");
        power.Toggle(); Equal(1, power.IsOn ? 1 : 0, "Can turn on again");
        // Las 24 permutaciones deben aceptar únicamente el orden didáctico 0,1,2,3.
        for (int a = 0; a < 4; a++) for (int b = 0; b < 4; b++)
        for (int c = 0; c < 4; c++) for (int d = 0; d < 4; d++)
        {
            if (a == b || a == c || a == d || b == c || b == d || c == d) continue;
            var puzzle = new SwitchConfigurationPuzzle();
            puzzle.Place(a, 0); puzzle.Place(b, 1); puzzle.Place(c, 2); puzzle.Place(d, 3);
            Equal(a == 0 && b == 1 && c == 2 && d == 3 ? 1 : 0, puzzle.Apply() ? 1 : 0, "Puzzle permutation");
        }
        var edit = new SwitchConfigurationPuzzle();
        Equal(0, edit.Apply() ? 1 : 0, "Empty puzzle rejected");
        edit.Place(0, 0); edit.Place(0, 1); Equal(-1, edit.GetBlock(0), "Moving a block clears its previous slot");
        edit.Place(1, 1); Equal(1, edit.GetBlock(1), "Slot replacement");
        Equal(0, edit.Place(4, 0) ? 1 : 0, "Unknown block rejected");
        Equal(0, edit.Place(0, -1) ? 1 : 0, "Unknown slot rejected");
        for (int i = 0; i < 4; i++) edit.Place(i, i);
        Equal(1, edit.Apply() ? 1 : 0, "Correction accepted");
        Equal(0, edit.Place(1, 0) ? 1 : 0, "Configured puzzle locked");
        edit.Reset(); Equal(0, edit.IsConfigured ? 1 : 0, "Power-off reset clears configuration");
        for (int i = 0; i < 4; i++) Equal(-1, edit.GetBlock(i), "Reset clears slot");
        // Matriz esperada por objetivo: equipar herramientas no figura como acción restringida.
        int[] masks = { 0, 0, 1, 2, 4, 4 | 8, 4 | 8 | 16, 4 | 8 | 16 | 32, 16 | 32 | 64, 16 };
        for (int stage = -1; stage <= 10; stage++)
            foreach (Module02Action action in Enum.GetValues<Module02Action>())
                foreach (bool locked in new[] { false, true })
                {
                    int mask = stage >= 0 && stage < masks.Length ? masks[stage] : 0;
                    if (locked) mask &= ~(4 | 8);
                    Equal((mask & (1 << (int)action)) != 0 ? 1 : 0,
                        Module02SequenceRules.Allows(stage, action, locked) ? 1 : 0, "Sequential action permission");
                }
        Equal(9, Modules.Module02_RackInstallation.Objectives.Module02ObjectiveCatalog.OrderedIds.Count, "Nine practical objectives");
        checks += TutorialRegressionChecks.Run();
        checks += FinaleRegressionChecks.Run();
        Console.WriteLine($"PASS: {checks} wiring, power, puzzle, sequence and tutorial assertions.");
    }
}
