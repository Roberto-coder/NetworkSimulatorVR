using System;

namespace Presentacion.GlobalUI.RadialSelectorTool
{
    /// <summary>
    /// Compatibility bridge for existing scenes. New objects should use RadialMenuController.
    /// </summary>
    [Obsolete("Use RadialMenuController instead.")]

    // [RadialMenuController.cs] apertura, cierre y equipamiento.
    // [RadialMenuBuilder.cs] construcción y geometría.
    // [RadialMenuSelector.cs] estado de selección y resaltado.
    // [RadialPart.cs] eventos RayCanvas y filtro radial.
    // [RadialSelectionComponent_OLD.cs] puente de compatibilidad.
    // [RadialSelection.prefab] configuración visual actualizada.
    public sealed class RadialSelectionComponent_OLD : RadialMenuController
    {
    }
}
