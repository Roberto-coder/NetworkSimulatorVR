/// <summary>Reglas compartidas por el selector, las transiciones y el perfil.</summary>
public static class ModuleAccessPolicy
{
    public static int ModuleNumber(string id)
    {
        switch (id?.Trim().ToLowerInvariant())
        {
            case "modulo1": case "m1_": case "module01": return 1;
            case "modulo2": case "m2_": case "module02": case "module_02_rack_installation": return 2;
            case "modulo3": case "m3_": case "module03": return 3;
            default: return 0;
        }
    }

    public static bool IsCompleted(PlayerProgress progress, int number) =>
        progress?.completedModuleIds?.Exists(id => ModuleNumber(id) == number) == true ||
        progress?.modules?.Exists(item => item != null && item.completed && ModuleNumber(item.moduleId) == number) == true;

    public static bool IsUnlocked(PlayerProgress progress, int number, bool debug) =>
        number >= 1 && number <= 3 && (debug || number == 1 || IsCompleted(progress, number - 1));

    public static bool CanEnterScene(PlayerProgress progress, string scene, bool debug)
    {
        int number = ModuleNumber(scene);
        return number == 0 || IsUnlocked(progress, number, debug);
    }
}
