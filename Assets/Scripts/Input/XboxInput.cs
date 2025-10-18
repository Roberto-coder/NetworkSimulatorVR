using UnityEngine;

/// <summary>
/// Implementación de IPlayerInput usando el Input Manager clásico de Unity
/// (Configurado por defecto para controles de Xbox).
/// Mapeo de Botones (Xbox -> Unity)
//A -> joystick button 0

//B -> joystick button 1

//X -> joystick button 2

//Y -> joystick button 3

//LB (Bumper Izquierdo) -> joystick button 4

//RB (Bumper Derecho) -> joystick button 5

//View (Botón "Atrás" o "Select") -> joystick button 6

//Menu (Botón "Start") -> joystick button 7

//Click Stick Izquierdo (LSB) -> joystick button 8

//Click Stick Derecho (RSB) -> joystick button 9

/// </summary>
public class XboxInput : IPlayerInput
{
    public Vector2 GetMoveInput()
    {
        // Usa los ejes "Horizontal" y "Vertical" del Input Manager
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        return new Vector2(horizontal, vertical);
    }

    public Vector2 GetRotationInput()
    {
        float rotH = Input.GetAxis("RightStickHorizontal");
        float rotV = Input.GetAxis("RightStickVertical");
        return new Vector2(rotH, rotV);
    }


    public bool GetJumpInputDown()
    {
        // "Jump" por defecto es 'Y' (joystick button 3)
        return Input.GetButtonDown("Jump");
    }

    public bool GetSprintInput()
    {
        // Botón 'X' en el control derecho
        return Input.GetButton("Fire3");
    }
}