namespace Modules.Module02_RackInstallation.Interaction
{
    public sealed class SwitchPowerState
    {
        public bool HasSupply { get; private set; }
        public bool IsOn { get; private set; }
        public void SetSupply(bool available)
        {
            HasSupply = available;
            if (!available) IsOn = false;
        }
        public void Toggle() => IsOn = HasSupply && !IsOn;
    }
}
