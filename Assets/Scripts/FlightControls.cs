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
            accelerationFactor = 0.2f;
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


        /*if (currentSpeed < cruisingSpeed / 2)
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
        }*/





        float currentSpeedMagnitude = rb.linearVelocity.magnitude;

        float baseAcceleration = 0.45f; 
        float speedIncrease = throttle * accelerationFactor * baseAcceleration * Time.fixedDeltaTime;
        currentSpeedMagnitude += speedIncrease;

        currentSpeedMagnitude = Mathf.Min(currentSpeedMagnitude, maxSpeed);

        rb.linearVelocity = -transform.right * currentSpeedMagnitude;





        /////////////////////////////////////// Yerçekimi ve Lift Force ///////////////////////////////////////////////
        // Y doğrultusunda gravity ve lift etkisini uygulama
        float gravityEffect = 0f;
        float liftEffect = 0f;

        // Yerçekimi etkisi: Yavaşça artan bir negatif hız (aşağı doğru)
        gravityEffect = Mathf.Lerp(0f, -2f, currentSpeedMagnitude / maxSpeed);

        // Kaldırma kuvveti: Yavaşça artan bir pozitif hız (yukarı doğru)
        liftEffect = 0;// Mathf.Lerp(0f, 10f, currentSpeedMagnitude / maxSpeed);

        // Y doğrultusunda hız üzerinde değişiklik yapma
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y + gravityEffect + liftEffect, rb.linearVelocity.z);



        
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

        // Bunları uçuş kontrolüne aktarabilirsin

        if (Input.GetKeyDown(KeyCode.G))
        {
            currentSpeed += 100f;
        }
    }
}
