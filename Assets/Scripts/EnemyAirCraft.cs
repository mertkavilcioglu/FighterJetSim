using UnityEngine;

public class EnemyAirCraft : MonoBehaviour
{
    private Transform target;
    private float simulatedCruiseSpeed = 35f;
    private float maxSimulatedSpeed = 70f;
    private float minSimulatedSpeed = 35f;

    private float turnSpeed = 20f; 
    private float acceleration = 50f;
    private float currentSpeed;


    [SerializeField] private Transform rootObj; 
    private float rollAngleMax = 70f;           
    private float rollSmoothSpeed = 5f;         
    private float currentRoll = 0f;

    private bool isDead = false;

    private Rigidbody rb;
    private float customGravity = 7f;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        currentSpeed = simulatedCruiseSpeed;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; 
    }

    void FixedUpdate()
    {
        if (target == null) return;

        if (!isDead)
        {
            Movement();
            Roll();
        }
        else
        {
            StartFalling();
        }

        

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ShootDown();
        }
    }

    private void Movement()
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);

        float distance = Vector3.Distance(transform.position, target.position);
        float targetSpeed = simulatedCruiseSpeed;

        if (distance > 3000f)
        {
            targetSpeed = maxSimulatedSpeed;
        }
        else if (distance < 1500f)
        {
            targetSpeed = minSimulatedSpeed;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = transform.forward * currentSpeed; 
    }

    private void Roll()
    {
        if (rootObj == null) return;

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 flatTargetDir = new Vector3(directionToTarget.x, 0f, directionToTarget.z).normalized;

        float signedAngle = Vector3.SignedAngle(flatForward, flatTargetDir, Vector3.up); 
        float targetRoll = Mathf.Clamp(signedAngle / 45f, -1f, 1f) * rollAngleMax;       

        currentRoll = Mathf.Lerp(currentRoll, targetRoll, rollSmoothSpeed * Time.fixedDeltaTime);

        Vector3 rootEuler = rootObj.localEulerAngles;
        rootEuler.z = -currentRoll; 
        rootObj.localEulerAngles = rootEuler;
    }

    public void ShootDown()
    {
        isDead = true;
        Debug.Log("Enemy shot down!");  
    }

    private void StartFalling()
    {
        if (rootObj != null)
        {
            Vector3 rootEuler = rootObj.localEulerAngles;
            rootEuler.z += 200f * Time.fixedDeltaTime;
            rootObj.localEulerAngles = rootEuler;

            Vector3 lastSpeed = transform.forward * currentSpeed;
            rb.linearVelocity = new Vector3(lastSpeed.x, rb.linearVelocity.y, lastSpeed.z);

            //rb.useGravity = true;
            rb.AddForce(Vector3.down * customGravity, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

}
