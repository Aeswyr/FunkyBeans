using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public Vector2 dir {
        get;
        private set;
    }

    public Vector2 mousePos {
        get;
        private set;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public void Move(InputAction.CallbackContext ctx) {
        dir = ctx.ReadValue<Vector2>();
    }

    public void MouseMove(InputAction.CallbackContext ctx) {
        mousePos = ctx.ReadValue<Vector2>();
    }
}

public struct Button {
    public bool down, released, pressed;

    public void Reset() {
        released = false;
        pressed = false;
    }

    public void Set(InputAction.CallbackContext ctx) {
        down = ctx.canceled == false;

        released = !down;
        pressed = down;
    }
}
