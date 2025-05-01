using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponControls : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference shootAction;
    public InputActionReference flightModeAction;
    public InputActionReference gunModeAction;
    public InputActionReference missileModeAction;

    [Header("Weapon Settings")]
    [SerializeField] private int maxAmmo = 512;
    [SerializeField] private float fireRate = 22.5f;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float spreadAngle = 0.25f;

    [Header("References")]
    public Transform firePoint;             
    public GameObject bulletPrefab;
    public GameObject bulletHolder;
    public TMP_Text ammoText;

    [Header("HUDs")]
    public GameObject flightHud;
    public GameObject gunHud;
    public GameObject missileHud;

    private int currentAmmo;
    private float fireCooldown;
    private bool isShooting;

    private bool isFlightMode = true;
    private bool isGunMode = false; 
    private bool isMissileMode = false;

    private Rigidbody rbPlane;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoCountHud();
        shootAction.action.performed += ctx => isShooting = true;
        shootAction.action.canceled += ctx => isShooting = false;

        flightModeAction.action.performed += ctx => SwitchToFlightMode();
        gunModeAction.action.performed += ctx => SwitchToGunMode();
        missileModeAction.action.performed += ctx => SwitchToMissileMode();

        rbPlane = GetComponent<Rigidbody>();

        SwitchToFlightMode();
    }

    void OnEnable()
    {
        shootAction.action.Enable();
    }

    void OnDisable()
    {
        shootAction.action.Disable();
    }

    void Update()
    {
        if (isShooting && currentAmmo > 0 && fireCooldown <= 0f)
        {
            Fire();
            fireCooldown = 1f / fireRate;
        }
        

        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    void Fire()
    {
        if (isGunMode)
        {
            currentAmmo -= 3;
            if (currentAmmo < 0)
            {
                currentAmmo = 0;
            }
            UpdateAmmoCountHud();

            if (bulletPrefab && firePoint)
            {
                Vector3 randomSpread = Quaternion.Euler(
                    Random.Range(-spreadAngle, spreadAngle),
                    Random.Range(-spreadAngle, spreadAngle),
                    0f
                ) * -firePoint.right;

                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(randomSpread));
                bullet.transform.SetParent(bulletHolder.transform);
                Rigidbody rb = bullet.GetComponentInChildren<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = rbPlane.linearVelocity + randomSpread.normalized * bulletSpeed;
                }
            }
        }
    }

    void UpdateAmmoCountHud()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
        }
    }

    private void SwitchToFlightMode()
    {
        isFlightMode = true;
        isGunMode = false;
        isMissileMode = false;

        flightHud.SetActive(true);
        gunHud.SetActive(false);
        missileHud.SetActive(false);
    }

    private void SwitchToGunMode()
    {
        isFlightMode = false;
        isGunMode = true;
        isMissileMode = false;

        flightHud.SetActive(false);
        gunHud.SetActive(true);
        missileHud.SetActive(false);
    }

    private void SwitchToMissileMode()
    {
        isFlightMode = false;
        isGunMode = false;
        isMissileMode = true;

        flightHud.SetActive(false);
        gunHud.SetActive(false);
        missileHud.SetActive(true);
    }
}
