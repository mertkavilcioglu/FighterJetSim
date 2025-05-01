using UnityEngine;
using UnityEngine.InputSystem;

public class FlightControls : MonoBehaviour
{
    [Header("Input Settings")]
    public InputActionAsset inputActions;
    private InputAction pitchAction, rollAction, yawAction, throttleAction, airBrakeAction;
    private float pitch, roll, yaw, throttle;

    private Rigidbody rb;

    [Header("Aircraft Settings (F-16C)")]
    private float maxSpeed = 1345f;
    public float initialSpeed = 0f;
    private float realSpeed;
    private float simulatedSpeed;

    private float startAltitude = 2500f;
    private float altitude;

    private bool isLanded = false;
    private bool airBrakeOn = false;

    private bool isFalling = false;
    private float fallingSpeed = 0f;

    [Header("Free Look Settings")]
    [SerializeField] private Transform cameraPivot; 
    [SerializeField] private Transform cameraTransform; 
    [SerializeField] private InputActionReference hatSwitchLook;
    [SerializeField] private float lookSpeed = 50f;
    [SerializeField] private float verticalLimit = 60f;

    private float horizontalLook = 0f;
    private float verticalLook = 0f;


    [SerializeField] private float mouseLookSpeedX = 2f;  
    [SerializeField] private float mouseLookSpeedY = 2f;  
    private float rotationX = 0f;  
    private float rotationY = 0f;

    public Camera targetCamera;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 20f;
    [SerializeField] private float minFOV = 30f;
    [SerializeField] private float maxFOV = 70f;

    [Header("Zoom Buttons")]
    public InputActionReference zoomInButton;
    public InputActionReference zoomOutButton;

    void Start()
    {
        var flightMap = inputActions.FindActionMap("FlightInputs");

        pitchAction = flightMap.FindAction("Pitch");
        rollAction = flightMap.FindAction("Roll");
        yawAction = flightMap.FindAction("Yaw");
        throttleAction = flightMap.FindAction("Throttle");

        airBrakeAction = flightMap.FindAction("AirBrake");
        airBrakeAction.performed += ctx => ToggleAirBrake();
        airBrakeAction.Enable();

        flightMap.Enable();

        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;  
        Cursor.visible = false;
        realSpeed = initialSpeed;

        InitializeSpeed();
    }

    void Update()
    {
        pitch = pitchAction.ReadValue<float>();
        roll = rollAction.ReadValue<float>();
        yaw = yawAction.ReadValue<float>();
        throttle = throttleAction.ReadValue<float>();

        HandleLook();
        ZoomScroll();
        ZoomButton();
    }

    private void FixedUpdate()
    {
        Accelerate();
        Deaccelerate();
        ApplyPitchRollYaw();
        Altitude();

        FallCheck();

        if (!isFalling)
        {
            LiftForce();
        }
        else if(!isLanded)
        {
            ApplyFallingForce();
        }

        //Debug.Log($"Speed: {simulatedSpeed:F1}, Altitude: {altitude:F1}");
    }

    private void ApplyPitchRollYaw()
    {
        float maxBaseRotationSpeed = 40f;
        float pitchRollBaseSpeed = 0f;

        if (simulatedSpeed < 70f)
        {
            pitchRollBaseSpeed = 0f;
        }
        else if (simulatedSpeed >= 70f && simulatedSpeed < 400f)
        {
            float t = Mathf.InverseLerp(100f, 400f, simulatedSpeed);
            pitchRollBaseSpeed = Mathf.Lerp(0f, maxBaseRotationSpeed, t);
        }
        else if (simulatedSpeed >= 400f && simulatedSpeed <= maxSpeed)
        {
            float t = Mathf.InverseLerp(350f, maxSpeed, simulatedSpeed);
            pitchRollBaseSpeed = Mathf.Lerp(maxBaseRotationSpeed, maxBaseRotationSpeed / 4f, t);
        }

        float pitchSpeed = pitch > 0 ? pitchRollBaseSpeed : pitchRollBaseSpeed * 0.55f;
        float rollSpeed = pitchRollBaseSpeed * 2.25f;
        float yawSpeedMax = 50f / 10f;
        float yawSpeed = yawSpeedMax;

        if (simulatedSpeed < 15f)
        {
            yawSpeed = Mathf.Lerp(0f, yawSpeedMax, simulatedSpeed / 15f);
        }

        Quaternion deltaPitch = Quaternion.AngleAxis(-pitch * pitchSpeed * Time.fixedDeltaTime, Vector3.forward);
        Quaternion deltaRoll = Quaternion.AngleAxis(roll * rollSpeed * Time.fixedDeltaTime, Vector3.right);
        Quaternion deltaYaw = Quaternion.AngleAxis(yaw * yawSpeed * Time.fixedDeltaTime, Vector3.up);


        // Speed Penalty by Pitch
        transform.localRotation *= deltaYaw * deltaPitch * deltaRoll;

        if (simulatedSpeed > 100f)
        {
            float pitchInput = Mathf.Abs(pitch); 

            if (pitchInput > 0.2f)
            {
                float sharpness = Mathf.InverseLerp(0.2f, 1f, pitchInput); 
                float slowdownStrength = Mathf.InverseLerp(0f, maxBaseRotationSpeed, pitchRollBaseSpeed); 

                float slowDownMultiplier = 4f; 

                if(pitch > 0)
                {
                    if (pitch < 0.5f)
                        slowDownMultiplier = 0.75f;
                    else if (pitch < 0.75f)
                        slowDownMultiplier = 1.5f;
                    else if (pitch >= 0.75f)
                        slowDownMultiplier = 3.0f;
                }
                else
                {
                    slowDownMultiplier = 0.75f;
                }
                float totalSlowdown = sharpness * slowdownStrength * slowDownMultiplier; 

                float decelerationAmount = totalSlowdown * Time.fixedDeltaTime;

                Vector3 currentVelocity = rb.linearVelocity;
                Vector3 deceleratedVelocity = currentVelocity + currentVelocity.normalized * -decelerationAmount;

                if (Vector3.Dot(currentVelocity, deceleratedVelocity) <= 0)
                {
                    deceleratedVelocity = Vector3.zero;
                }

                rb.linearVelocity = deceleratedVelocity;
            }
        }
    }

    private void Altitude()
    {
        altitude = startAltitude + transform.position.y * 10f;
    }


    private void Accelerate()
    {
        realSpeed = rb.linearVelocity.magnitude;
        simulatedSpeed = realSpeed * 9f; // 7f
        //Debug.Log(simulatedSpeed);

        float currentSpeedMagnitude = rb.linearVelocity.magnitude;
        float speedIncrease; 


        if (simulatedSpeed < 150)
        {
            currentSpeedMagnitude += (throttle / 30f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed < 200f)
        {
            currentSpeedMagnitude += (throttle / 40f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed < 250f)
        {
            currentSpeedMagnitude += (throttle / 50f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed < 300f)
        {
            currentSpeedMagnitude += (throttle / 60f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed < 400f)
        {
            currentSpeedMagnitude += (throttle / 70f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed < 500f)
        {
            currentSpeedMagnitude += (throttle / 80f * Time.fixedDeltaTime);
        }
        else if (simulatedSpeed < 700f)
        {
            currentSpeedMagnitude += (throttle / 90f * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeedMagnitude += (throttle / 100f * Time.fixedDeltaTime);
        }

        speedIncrease = throttle * Time.fixedDeltaTime;
        currentSpeedMagnitude = Mathf.Min(currentSpeedMagnitude, maxSpeed);

        rb.linearVelocity = -transform.right * currentSpeedMagnitude;
    }

    private void Deaccelerate()
    {
        if (simulatedSpeed > 0)
        {
            float decelerationRate = 1f;

            if (airBrakeOn)
            {
                decelerationRate = 3.5f;
            }
            else if (throttle < 0.1f)
            {
                decelerationRate = 0.1f; 
            }
            else if (throttle < 0.3f)
            {
                decelerationRate = 0.050f; 
            }
            else if (throttle < 0.5f)
            {
                decelerationRate = 0.01f; 
            }
            else 
            {
                decelerationRate = 0; 
            }
           

            float decelerationAmount = decelerationRate * Time.fixedDeltaTime;

            Vector3 currentVelocity = rb.linearVelocity;

            Vector3 deceleratedVelocity = currentVelocity + currentVelocity.normalized * -decelerationAmount;

            if (Vector3.Dot(currentVelocity, deceleratedVelocity) <= 0)
            {
                deceleratedVelocity = Vector3.zero;
            }

            rb.linearVelocity = deceleratedVelocity;
        }
    }


    private void LiftForce() 
    {
        float liftForce = 0f;

        if (simulatedSpeed < 84f)
        {
            liftForce = Mathf.Lerp(0f, 7f, simulatedSpeed / 84f);  
        }
        else if (simulatedSpeed < 200f)
        {
            liftForce = 7f;
        }
        else if (simulatedSpeed < 350f)
        {
            liftForce = 7f;
        }
        else if (simulatedSpeed >= 350f)
        {
            liftForce = 7f;
        }
        rb.AddForce(Vector3.up * liftForce, ForceMode.Force);
    }

    private void FallCheck()
    {
        if (simulatedSpeed < 150f && throttle < 0.3f)
        {
            isFalling = true;
        }
        else
        {
            isFalling = false;
            fallingSpeed = 0f; 
        }
    }

    private void ApplyFallingForce()
    {
        fallingSpeed += 9.81f * Time.fixedDeltaTime; 
        rb.AddForce(Vector3.down * fallingSpeed, ForceMode.Force);
    }


    private void HandleHatSwitchLook() // Hat Switch Only (not in use anymore)
    {
        Vector2 input = hatSwitchLook.action.ReadValue<Vector2>();
        if (input == Vector2.zero) return;

        horizontalLook += input.x * lookSpeed * Time.deltaTime;
        verticalLook -= input.y * lookSpeed * Time.deltaTime;
        verticalLook = Mathf.Clamp(verticalLook, -verticalLimit, verticalLimit);

        cameraPivot.localRotation = Quaternion.Euler(0f, 0f, -horizontalLook);
        cameraTransform.localRotation = Quaternion.Euler(0f, verticalLook, 0f);
    }

    private void HandleMouseLook() // Mouse Only (not in use anymore)
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotationY += mouseX * mouseLookSpeedX;
        rotationY = Mathf.Clamp(rotationY, -verticalLimit, verticalLimit);  

        rotationX -= mouseY * mouseLookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -verticalLimit, verticalLimit);  

        cameraPivot.localRotation = Quaternion.Euler(0f, 0f, -rotationY);  
        cameraTransform.localRotation = Quaternion.Euler(0f, rotationX, 0f);  
    }

    private void HandleLook() // Mouse and Hat Switch
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector2 hatInput = hatSwitchLook.action.ReadValue<Vector2>();
        if (hatInput != Vector2.zero)
        {
            mouseX = hatInput.x / 4;
            mouseY = hatInput.y / 4; 
        }

        rotationY += mouseX * mouseLookSpeedX;
        rotationX -= mouseY * mouseLookSpeedY;

        rotationX = Mathf.Clamp(rotationX, -verticalLimit, verticalLimit);

        cameraPivot.localRotation = Quaternion.Euler(0f, 0f, -rotationY);  
        cameraTransform.localRotation = Quaternion.Euler(0f, rotationX, 0f);  
    }

    void ZoomScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            float newFOV = targetCamera.fieldOfView - scroll * zoomSpeed;
            targetCamera.fieldOfView = Mathf.Clamp(newFOV, minFOV, maxFOV);
        }
    }

    void ZoomButton()
    {
        bool zoomInPressed = zoomInButton.action.IsPressed();
        bool zoomOutPressed = zoomOutButton.action.IsPressed();

        if (zoomInPressed)
        {
            targetCamera.fieldOfView = Mathf.Clamp(targetCamera.fieldOfView - zoomSpeed * Time.deltaTime, minFOV, maxFOV);
        }
        else if (zoomOutPressed)
        {
            targetCamera.fieldOfView = Mathf.Clamp(targetCamera.fieldOfView + zoomSpeed * Time.deltaTime, minFOV, maxFOV);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            rb.freezeRotation = false;
            isLanded = true;
            airBrakeOn = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            rb.freezeRotation = true;
            isLanded = false;
        }
    }

    private void ToggleAirBrake()
    {
        if (!isLanded)
        {
            airBrakeOn = !airBrakeOn;
        }
        Debug.Log("Air Brake: " + (airBrakeOn ? "On" : "Off"));
    }

    private void InitializeSpeed()
    {
        rb.linearVelocity = -transform.right * initialSpeed/9f;
    }

    public float GetAltitude()
    {
        return altitude;
    }

    public float GetSimulatedSpeed()
    {
        return simulatedSpeed;
    }


}
