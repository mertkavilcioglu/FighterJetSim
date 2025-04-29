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


    [SerializeField] private Transform rootObj; // Roll uygulanacak obje
    private float rollAngleMax = 70f;           // Maksimum roll açýsý
    private float rollSmoothSpeed = 5f;         // Roll geçiþ hýzý
    private float currentRoll = 0f;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        currentSpeed = simulatedCruiseSpeed;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        Movement();
        Roll();
    }

    private void Movement()
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);

        float distance = Vector3.Distance(transform.position, target.position);
        float desiredSpeed = simulatedCruiseSpeed;

        if (distance > 3000f)
        {
            desiredSpeed = maxSimulatedSpeed;
        }
        else if (distance < 1500f)
        {
            desiredSpeed = minSimulatedSpeed;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.fixedDeltaTime);
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    private void Roll()
    {
        if (rootObj == null) return;

        // Yalnýzca yön farký üzerinden roll hesapla
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 flatTargetDir = new Vector3(directionToTarget.x, 0f, directionToTarget.z).normalized;

        float signedAngle = Vector3.SignedAngle(flatForward, flatTargetDir, Vector3.up); // Saða/sola dönüþ farký
        float targetRoll = Mathf.Clamp(signedAngle / 45f, -1f, 1f) * rollAngleMax;       // Normalize edilip roll hesaplanýr

        currentRoll = Mathf.Lerp(currentRoll, targetRoll, rollSmoothSpeed * Time.fixedDeltaTime);

        Vector3 rootEuler = rootObj.localEulerAngles;
        rootEuler.z = -currentRoll; // Z ekseninde yatýþ
        rootObj.localEulerAngles = rootEuler;
    }
}
