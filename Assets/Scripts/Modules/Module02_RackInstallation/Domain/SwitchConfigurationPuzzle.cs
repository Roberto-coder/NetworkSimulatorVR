namespace Modules.Module02_RackInstallation.Domain
{
    /// <summary>Reglas puras del puzzle: no dependen de Unity, UI, energía ni física.</summary>
    public sealed class SwitchConfigurationPuzzle
    {
        private readonly int[] slots = { -1, -1, -1, -1 };
        public bool IsConfigured { get; private set; }
        public int GetBlock(int slot) => slot >= 0 && slot < 4 ? slots[slot] : -1;

        public bool Place(int block, int slot)
        {
            if (IsConfigured || block < 0 || block >= 4 || slot < 0 || slot >= 4) return false;
            // Un bloque solo ocupa una posición; reemplazar libera el contenido anterior.
            for (int i = 0; i < 4; i++) if (slots[i] == block) slots[i] = -1;
            slots[slot] = block;
            return true;
        }
        public int FirstIncorrectSlot()
        {
            for (int i = 0; i < 4; i++) if (slots[i] != i) return i;
            return -1;
        }
        public bool Apply()
        {
            IsConfigured = FirstIncorrectSlot() == -1;
            return IsConfigured;
        }
        public void Reset()
        {
            IsConfigured = false;
            for (int i = 0; i < 4; i++) slots[i] = -1;
        }
    }
}
