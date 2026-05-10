using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputType inputType;

    [Header("Joystick")]
    [SerializeField] private FixedJoystick joystick;


    [Header("Trackpad")]
    [SerializeField] private MobileTrackpad trackpad;

    private IPlayerInput currentInput;

    private ICameraInput currentCameraInput;

    public Vector2 Movement => currentInput.GetMovement();
    public bool Jump => currentInput.GetJump();
    public bool Hit => currentInput.GetHit();
    public Vector2 LookDirection => currentCameraInput.GetLookDirection();

    public Button jumpButton;
    public Button hitButton;


    private void Awake()
    {
        InitializeInput();
    }

    private void InitializeInput()
    {
        switch (inputType)
        {
            case InputType.Keyboard:
                currentInput = new KeyboardInput();
                currentCameraInput = new MouseCameraInput();
                break;

            case InputType.Joystick:
                currentInput = new JoystickInput(joystick);
                currentCameraInput = new TrackpadLookInput(trackpad);
                break;
        }
    }

    private void OnEnable()
    {
        if (inputType == InputType.Joystick)
        {
            JoystickInput joystickInput = currentInput as JoystickInput;
            if (joystickInput != null)
            {
                jumpButton.onClick.AddListener(joystickInput.SetJumpPressed);
                hitButton.onClick.AddListener(joystickInput.SetHitPressed);
            }
        }
    }

    private void OnDisable()
    {
        if (inputType == InputType.Joystick)
        {
            JoystickInput joystickInput = currentInput as JoystickInput;
            if (joystickInput != null)
            {
                jumpButton.onClick.RemoveListener(joystickInput.SetJumpPressed);
                hitButton.onClick.RemoveListener(joystickInput.SetHitPressed);
            }
        }
    }
}



public enum InputType
{
    Keyboard,
    Joystick
}


public interface IPlayerInput
{
    Vector2 GetMovement();

    bool GetJump();

    bool GetHit();
}