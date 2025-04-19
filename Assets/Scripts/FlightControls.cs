using UnityEngine;
using UnityEngine.InputSystem;

public class FlightControls : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputAction pitchAction, rollAction, yawAction, throttleAction;
    private float pitch, roll, yaw, throttle;

    private float maxSpeed = 1345f;
    private float cruisingSpeed = 577f;
    private float currentSpeed = 0f;

    void Start()
    {
        var flightMap = inputActions.FindActionMap("FlightInputs");

        pitchAction = flightMap.FindAction("Pitch");
        rollAction = flightMap.FindAction("Roll");
        yawAction = flightMap.FindAction("Yaw");
        throttleAction = flightMap.FindAction("Throttle");

        flightMap.Enable();
    }

    private void FixedUpdate()
    {
        Accelerate();
    }

    private void Accelerate()
    {
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += throttle/300f * ((currentSpeed + 100f)/300f);
            Debug.Log(currentSpeed);
        }
    }

    void Update()
    {
        pitch = pitchAction.ReadValue<float>();
        roll = rollAction.ReadValue<float>();
        yaw = yawAction.ReadValue<float>();
        throttle = throttleAction.ReadValue<float>();

        //Debug.Log($"Pitch: {pitch}, Roll: {roll}, Yaw: {yaw}, Throttle: {throttle}");

        // Bunlarý uçuþ kontrolüne aktarabilirsin
    }
}
