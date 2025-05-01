using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponControls : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference shootAction;

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

    private int currentAmmo;
    private float fireCooldown;
    private bool isShooting;

    private Rigidbody rbPlane;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateHud();
        shootAction.action.performed += ctx => isShooting = true;
        shootAction.action.canceled += ctx => isShooting = false;

        rbPlane = GetComponent<Rigidbody>();
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
        currentAmmo -= 3;
        if (currentAmmo < 0)
        {
            currentAmmo = 0;
        }
        UpdateHud();

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

    void UpdateHud()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
        }
    }
}
