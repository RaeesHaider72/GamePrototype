using UnityEngine;

public class CameraInput : MonoBehaviour
{
    
}



public interface ICameraInput
{
    Vector2 GetLookDirection();
}


public class MouseCameraInput : ICameraInput
{
    public Vector2 GetLookDirection()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }
}



public class TrackpadLookInput : ICameraInput
{
    private MobileTrackpad trackpad;

    public TrackpadLookInput(MobileTrackpad trackpad)
    {
        this.trackpad = trackpad;
    }

    public Vector2 GetLookDirection()
    {
        return trackpad.LookDelta;
    }
}