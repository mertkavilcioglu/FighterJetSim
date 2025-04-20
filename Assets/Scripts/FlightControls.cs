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
    private float accelerationMultiplier = 0.1f;
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
        float realSpeed = rb.linearVelocity.magnitude;
        Debug.Log(realSpeed);

        float accelerationFactor = 1f;

        if (realSpeed < cruisingSpeed / 2f)
        {
            accelerationFactor = Mathf.Lerp(0.3f, 1.2f, realSpeed / (cruisingSpeed / 2f));
        }
        else if (realSpeed >= cruisingSpeed / 2f && realSpeed < cruisingSpeed)
        {
            accelerationFactor = Mathf.Lerp(1.2f, 0.6f, (realSpeed - cruisingSpeed / 2f) / (cruisingSpeed / 2f));
        }
        else if (realSpeed >= cruisingSpeed && realSpeed < maxSpeed)
        {
            accelerationFactor = Mathf.Lerp(0.6f, 0.1f, (realSpeed - cruisingSpeed) / (maxSpeed - cruisingSpeed));
        }
        else
        {
            accelerationFactor = 0f; 
        }

        float currentSpeedMagnitude = rb.linearVelocity.magnitude;

        float baseAcceleration = 0.5f; 
        float speedIncrease = throttle * accelerationFactor * baseAcceleration * Time.fixedDeltaTime;
        currentSpeedMagnitude += speedIncrease;

        currentSpeedMagnitude = Mathf.Min(currentSpeedMagnitude, maxSpeed);

        rb.linearVelocity = -transform.right * currentSpeedMagnitude;





        /*Vector3 forceDirection = -transform.right; 
        rb.AddForce(forceDirection * throttle * accelerationFactor / 2.25f, ForceMode.Force);*/



        /*Vector3 targetVelocity = -transform.right * throttle * accelerationFactor;
        // Hızın maxSpeed'i aşmaması için hız sınırını kontrol ediyoruz
        if (targetVelocity.magnitude > maxSpeed)
        {
            targetVelocity = targetVelocity.normalized * maxSpeed;
        }


        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity / 1.3f, Time.fixedDeltaTime * 2f);*/







        /////////////////////////////////////// Lift Force ///////////////////////////////////////////////
        float liftForce = 0f;

        if (realSpeed < 12f)
        {
            liftForce = Mathf.Lerp(0f, 10f, realSpeed / 12f); 
        }
        else
        {
            liftForce = Mathf.Lerp(10f, 30f, (realSpeed - 12f) / (maxSpeed - 12f));
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

        // Bunları uçuş kontrolüne aktarabilirsin

        if (Input.GetKeyDown(KeyCode.G))
        {
            currentSpeed += 100f;
        }
    }
}
