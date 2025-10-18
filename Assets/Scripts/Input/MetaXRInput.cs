using UnityEngine;
// Asegúrate de tener el SDK de Meta (Oculus Integration) importado
using OVR;

/// <summary>
/// Implementación de IPlayerInput usando el SDK de Meta (OVRInput).
/// 
//Mapeo por Defecto de Unity (Input Manager)

//Fire1 está asignado por defecto a joystick button 0 -> Botón A

//Fire2 está asignado por defecto a joystick button 1 -> Botón B

//Fire3 está asignado por defecto a joystick button 2 -> Botón X

//Jump está asignado por defecto a joystick button 3 -> Botón Y
/// 
/// </summary>
public class MetaXRInput : IPlayerInput
{
    public Vector2 GetMoveInput()
    {
        // Usa el Thumbstick del control izquierdo
        return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
    }

    public Vector2 GetRotationInput()
    {
        return OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.RTouch);
    }

    public bool GetJumpInputDown()
    {
        // Botón 'A' en el control derecho
        return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
    }

    public bool GetSprintInput()
    {
        // Botón 'X' en el control izquierdo
        return OVRInput.Get(OVRInput.Button.Three, OVRInput.Controller.LTouch);
    }
}