// Define un contrato para cualquier sistema que pueda sostener un objeto.
// Expone la referencia al objeto seleccionado actualmente.

namespace Framework.Interaction
{
    public interface IObjectHolder
    {
        Interactable SelectedObject { get; }
    }
}
