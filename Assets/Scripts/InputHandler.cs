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
    private Button m_action;
    public Button action {
        get {return m_action;}
    }
    private Button m_interact;
    public Button interact {
        get {return m_interact;}
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_action.Reset();
        m_interact.Reset();
    }

    public void Move(InputAction.CallbackContext ctx) {
        dir = ctx.ReadValue<Vector2>();
    }

    public void MouseMove(InputAction.CallbackContext ctx) {
        mousePos = ctx.ReadValue<Vector2>();
    }

    public void Interact(InputAction.CallbackContext ctx) {
        m_interact.Set(ctx);
    }

    public void Action(InputAction.CallbackContext ctx) {
        m_action.Set(ctx);
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
