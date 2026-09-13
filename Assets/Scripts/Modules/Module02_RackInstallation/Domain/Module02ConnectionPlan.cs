using System;
using System.Collections.Generic;
using Shared.Cabling;

namespace Modules.Module02_RackInstallation.Domain
{
    public readonly struct PlannedConnection
    {
        public readonly string Origin, Destination;
        public readonly NetworkPortKind Kind;
        public PlannedConnection(string origin, string destination, NetworkPortKind kind)
        { Origin = origin; Destination = destination; Kind = kind; }

        public bool Matches(string start, string end, NetworkPortKind cableKind,
            NetworkPortKind startKind, NetworkPortKind endKind) =>
            cableKind == Kind && startKind == Kind && endKind == Kind &&
            ((Same(start, Origin) && Same(end, Destination)) || (Same(end, Origin) && Same(start, Destination)));

        private static bool Same(string first, string second) =>
            string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Single fixed wiring table, also reusable by labels and tutorial.</summary>
    public static class Module02ConnectionPlan
    {
        public static IReadOnlyList<PlannedConnection> Links { get; } = Array.AsReadOnly(new[]
        {
            new PlannedConnection("PDU-A/AC01", "SW1/Power", NetworkPortKind.Power),
            new PlannedConnection("PP-A/01", "SW1/Gi01", NetworkPortKind.EthernetRj45),
            new PlannedConnection("PP-A/02", "SW1/Gi02", NetworkPortKind.EthernetRj45),
            new PlannedConnection("PP-A/03", "SW1/Gi03", NetworkPortKind.EthernetRj45),
            new PlannedConnection("FW1/eth01", "SW1/Gi04", NetworkPortKind.EthernetRj45)
        });

        public static int FindMatch(string start, string end, NetworkPortKind cableKind,
            NetworkPortKind startKind, NetworkPortKind endKind)
        {
            for (int i = 0; i < Links.Count; i++)
                if (Links[i].Matches(start, end, cableKind, startKind, endKind)) return i;
            return -1;
        }
    }
}
