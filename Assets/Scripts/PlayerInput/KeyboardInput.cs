using UnityEngine;

public class KeyboardInput : IPlayerInput
{
   public Vector2 GetMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        return new Vector2(x, y).normalized;
    }

    public bool GetJump()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public bool GetHit()
    {
        return Input.GetMouseButtonDown(0);
    }
  
}
