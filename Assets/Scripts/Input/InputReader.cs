using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    private Controls controls;

    public event Action<bool> PrimaryFireEvent;
    public event Action<Vector2> MoveEvent;
    public event Action<float> TurretRotateEvent; // NUEVO

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Player.SetCallbacks(this);
        }

        controls.Player.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
            controls.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnPrimaryFire(InputAction.CallbackContext context)
    {
        if (context.performed) PrimaryFireEvent?.Invoke(true);
        else if (context.canceled) PrimaryFireEvent?.Invoke(false);
    }

    // ESTE callback existe si en tu Input Actions asset tienes una action llamada "TurretRotate"
    public void OnTurretRotate(InputAction.CallbackContext context)
    {
        TurretRotateEvent?.Invoke(context.ReadValue<float>());
    }
}