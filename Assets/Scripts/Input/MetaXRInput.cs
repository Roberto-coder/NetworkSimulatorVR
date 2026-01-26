using UnityEngine;
// Aseg�rate de tener el SDK de Meta (Oculus Integration) importado
using OVR;

/// <summary>
/// Implementaci�n de IPlayerInput usando el SDK de Meta (OVRInput).
/// 
//Mapeo por Defecto de Unity (Input Manager)

//Fire1 est� asignado por defecto a joystick button 0 -> Bot�n A

//Fire2 est� asignado por defecto a joystick button 1 -> Bot�n B

//Fire3 est� asignado por defecto a joystick button 2 -> Bot�n X

//Jump est� asignado por defecto a joystick button 3 -> Bot�n Y
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
        // Bot�n 'A' en el control derecho
        return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
    }

    public bool GetSprintInput()
    {
        // Bot�n 'X' en el control izquierdo
        return OVRInput.Get(OVRInput.Button.Three, OVRInput.Controller.LTouch);
    }
}