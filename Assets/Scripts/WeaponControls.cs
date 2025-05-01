using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponControls : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference shootGunAction;
    public InputActionReference fireMissileAction;
    public InputActionReference flightModeAction;
    public InputActionReference gunModeAction;
    public InputActionReference missileModeAction;
    public InputActionReference lockAction;

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

    [Header("Missiles")]
    public GameObject[] missiles;
    private int currentMissileIndex = 0;

    private int currentAmmo;
    private float fireCooldown;
    private bool isShooting = false;

    private bool isFlightMode = true;
    private bool isGunMode = false; 
    private bool isMissileMode = false;

    private Rigidbody rbPlane;

    public List<GameObject> detectedEnemies = new List<GameObject>();
    public GameObject lockedEnemy;
    private int currentLockedIndex = -1;
    public EnemyHUDController enemyHudIcon;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoCountHud();
        shootGunAction.action.performed += ctx => isShooting = true;
        shootGunAction.action.canceled += ctx => isShooting = false;

        fireMissileAction.action.started += ctx => Missiles();

        flightModeAction.action.performed += ctx => SwitchToFlightMode();
        gunModeAction.action.performed += ctx => SwitchToGunMode();
        missileModeAction.action.performed += ctx => SwitchToMissileMode();
        lockAction.action.performed += ctx => LockNextEnemy();

        rbPlane = GetComponent<Rigidbody>();

        SwitchToFlightMode();
    }

    void OnEnable()
    {
        shootGunAction.action.Enable();
        lockAction.action.Enable();
    }

    void OnDisable()
    {
        shootGunAction.action.Disable();
        lockAction.action.Disable();
    }

    void Update()
    {
        detectedEnemies.RemoveAll(enemy => enemy == null);
        Guns();
    }

    void FireGun()
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

    public void AddEnemy(GameObject enemy)
    {
        if (!detectedEnemies.Contains(enemy))
        {
            detectedEnemies.Add(enemy);
            //Debug.Log("Enemy added: " + enemy.name);
        }
    }

    public void RemoveEnemy(GameObject enemy)
    {
        if (detectedEnemies.Contains(enemy))
        {
            detectedEnemies.Remove(enemy);
            //Debug.Log("Enemy removed: " + enemy.name);
        }
    }

    private void LockNextEnemy()
    {
        if (detectedEnemies.Count == 0) return;
        detectedEnemies.RemoveAll(e => e == null);

        if (lockedEnemy != null && enemyHudIcon != null)
        {
            enemyHudIcon.DeactivateLockedHud();
        }

        currentLockedIndex = (currentLockedIndex + 1) % detectedEnemies.Count;
        lockedEnemy = detectedEnemies[currentLockedIndex];

        if (lockedEnemy != null)
        {
            enemyHudIcon = lockedEnemy.GetComponent<EnemyHUDController>();
            if (enemyHudIcon != null)
            {
                enemyHudIcon.ActivateLockedHud();
            }
        }
    }

    private void Guns()
    {
        if (isShooting && currentAmmo > 0 && fireCooldown <= 0f)
        {
            FireGun();
            fireCooldown = 1f / fireRate;
        }


        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    private void Missiles()
    {
        if (isMissileMode)
        {
            FireMissile();
        }
    }

    private void FireMissile()
    {
        if (lockedEnemy == null || missiles.Length == 0 || currentMissileIndex >= missiles.Length)
            return;

        GameObject missile = missiles[currentMissileIndex];
        if (missile == null) return;

        missile.transform.parent = null;

        Transform target = lockedEnemy.transform;

        Rigidbody rb = missile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        MissileController mc = missile.GetComponent<MissileController>();
        if (mc != null)
        {
            mc.SetTarget(target);
        }

        currentMissileIndex++;
    }

}
