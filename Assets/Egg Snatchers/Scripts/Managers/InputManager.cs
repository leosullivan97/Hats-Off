using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [Header("Elements")]
    [SerializeField] private MobileJoystick joystick;

    private void Awake()
    {
        // Ensure only one instance of InputManager exists
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Returns the movement input from the joystick
    public Vector2 GetMoveVector() => joystick.GetMoveVector();
}
