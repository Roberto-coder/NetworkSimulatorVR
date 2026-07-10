using System;

namespace Systems.Input
{
    public class InputEvents
    {
        public static event Action MenuPressedEvent;
        public static event Action TriggerPressedEvent;

        public static void MenuPressed()
        {
            MenuPressedEvent?.Invoke();
        }
        
        public static void TriggerPressed()
        {
            TriggerPressedEvent?.Invoke();
        }
    
    }
}