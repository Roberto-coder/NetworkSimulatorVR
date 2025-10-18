using UnityEngine;

/// <summary>
/// Interfaz para abstraer el origen del input del jugador.
/// Permite intercambiar fácilmente entre controles (Xbox, Meta, Teclado, etc.).

/// </summary>
public interface IPlayerInput
{
    Vector2 GetMoveInput();
    Vector2 GetRotationInput();
    bool GetJumpInputDown();

    bool GetSprintInput();
}