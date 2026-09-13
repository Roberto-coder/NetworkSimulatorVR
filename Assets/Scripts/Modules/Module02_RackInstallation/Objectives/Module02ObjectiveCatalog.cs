namespace Modules.Module02_RackInstallation.Objectives
{
    /// <summary>
    /// IDs estables de la práctica en orden. Los títulos y descripciones editables
    /// pertenecen a los ObjectiveData de Assets/GameData, no al coordinador.
    /// </summary>
    public static class Module02ObjectiveCatalog
    {
        public const string InspectRack = "inspect_rack";
        public const string InspectSwitch = "inspect_switch";
        public const string MountSwitch = "mount_switch";
        public const string FastenSwitch = "fasten_switch";
        public const string ConnectLinks = "connect_links";
        public const string LabelLinks = "label_links";
        public const string ConnectConsole = "connect_console";
        public const string PowerOn = "power_on";
        public const string ConfigureSwitch = "configure_switch";

        public static readonly System.Collections.Generic.IReadOnlyList<string> OrderedIds =
            System.Array.AsReadOnly(new[] { InspectRack, InspectSwitch, MountSwitch, FastenSwitch,
                ConnectLinks, LabelLinks, ConnectConsole, PowerOn, ConfigureSwitch });
    }
}
