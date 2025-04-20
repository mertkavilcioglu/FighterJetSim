using UnityEngine;
using UnityEngine.InputSystem;

public class FlightControls : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputAction pitchAction, rollAction, yawAction, throttleAction;
    private float pitch, roll, yaw, throttle;

    private Rigidbody rb;

    private float maxSpeed = 1345f;
    private float cruisingSpeed = 577f;
    private float currentSpeed = 0f;
    float realSpeed;
    float simulatedSpeed;
    void Start()
    {
        var flightMap = inputActions.FindActionMap("FlightInputs");

        pitchAction = flightMap.FindAction("Pitch");
        rollAction = flightMap.FindAction("Roll");
        yawAction = flightMap.FindAction("Yaw");
        throttleAction = flightMap.FindAction("Throttle");

        flightMap.Enable();

        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Accelerate();
    }

    private void Accelerate()
    {
        realSpeed = rb.linearVelocity.magnitude;
        simulatedSpeed = realSpeed * 5f;
        Debug.Log(simulatedSpeed);

        float currentSpeedMagnitude = rb.linearVelocity.magnitude;
        float speedIncrease; 


        if (simulatedSpeed < 150)
        {
            currentSpeedMagnitude += (throttle / 25f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed >= 150 && simulatedSpeed < 200f)
        {
            currentSpeedMagnitude += (throttle / 40f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed >= 200 && simulatedSpeed < 250f)
        {
            currentSpeedMagnitude += (throttle / 55f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed >= 250 && simulatedSpeed < 300f)
        {
            currentSpeedMagnitude += (throttle / 70f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed >= 300 && simulatedSpeed < 400f)
        {
            currentSpeedMagnitude += (throttle / 85f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed >= 400 && simulatedSpeed < 500f)
        {
            currentSpeedMagnitude += (throttle / 90f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed >= 500 && simulatedSpeed < 700f)
        {
            currentSpeedMagnitude += (throttle / 100f * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeedMagnitude += (throttle / 125f * Time.fixedDeltaTime);
        }



        //Debug.Log(currentSpeed);
        if (currentSpeed < cruisingSpeed / 2)
        {
            currentSpeed += throttle / 100f * ((currentSpeed + 200f) / (cruisingSpeed / 2));
        }
        else if (currentSpeed >= cruisingSpeed / 2 && currentSpeed < cruisingSpeed)
        {
            currentSpeed += (throttle / 400f) * ((cruisingSpeed / 2) / (currentSpeed * 2.5f));
        }
        else if (currentSpeed >= cruisingSpeed && currentSpeed < maxSpeed)
        {
            currentSpeed += (throttle / 400f) * (cruisingSpeed / (currentSpeed * 10f));
        }


        

        speedIncrease = throttle * Time.fixedDeltaTime;
        //currentSpeedMagnitude += speedIncrease;

        currentSpeedMagnitude = Mathf.Min(currentSpeedMagnitude, maxSpeed);

        rb.linearVelocity = -transform.right * currentSpeedMagnitude;
        
        /////////////////////////////////////// OLD Lift Force ///////////////////////////////////////////////
        float liftForce = 0f;

        if (realSpeed < 12f)
        {
            liftForce = Mathf.Lerp(0f, 12f, realSpeed / 12f); 
        }
        else
        {
            liftForce = Mathf.Lerp(12f, 20f, (realSpeed - 12f) / (maxSpeed - 12f));
        }

        rb.AddForce(Vector3.up * liftForce, ForceMode.Force);
        


        // ucagin baktigi yone dogru bu hizi uygula, hiz arttikca gravity - ye dogru kaysin kaldirma kuvveti olarak ama bu sart da degil
        // roll ve pitch icin yine ilgili rotasyonlara += seklinde ekleme yap
    }

    void Update()
    {
        pitch = pitchAction.ReadValue<float>();
        roll = rollAction.ReadValue<float>();
        yaw = yawAction.ReadValue<float>();
        throttle = throttleAction.ReadValue<float>();

        //Debug.Log($"Pitch: {pitch}, Roll: {roll}, Yaw: {yaw}, Throttle: {throttle}");


        if (Input.GetKeyDown(KeyCode.G))
        {
            realSpeed += 100f;
        }
    }
}
