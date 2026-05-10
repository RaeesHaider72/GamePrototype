using UnityEngine;

public class JoystickInput : IPlayerInput
{
    private FixedJoystick joystick;
    private bool jumpPressed;
    private bool hitPressed;

    public JoystickInput(FixedJoystick joystick)
    {
        this.joystick = joystick;
    }

    public Vector2 GetMovement()
    {
        return new Vector2(joystick.Horizontal, joystick.Vertical);
    }

    public bool GetJump()
    {
        if (jumpPressed)
        {
            jumpPressed = false;
            return true;
        }
        return false;
    }

    public bool GetHit()
    {
        if (hitPressed)
        {
            hitPressed = false;
            return true;
        }
        return false;
    }

    public void SetJumpPressed() => jumpPressed = true;
    public void SetHitPressed() => hitPressed = true;
}