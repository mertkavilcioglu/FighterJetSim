using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    public Transform mainObj;            
    private float rotationSpeed = 75f;
    private float maxAngle = 30f;         

    private float currentAngle = 0f;
    private int direction = 1;

    public WeaponControls weaponControls;

    private Dictionary<GameObject, Coroutine> exitTimers = new Dictionary<GameObject, Coroutine>();

    void FixedUpdate()
    {
        RotateRadarArm();
    }

    private void RotateRadarArm()
    {
        if (mainObj == null) return;

        float deltaAngle = rotationSpeed * Time.fixedDeltaTime * direction;
        currentAngle += deltaAngle;

        if (currentAngle >= maxAngle)
        {
            currentAngle = maxAngle;
            direction = -1;
        }
        else if (currentAngle <= -maxAngle)
        {
            currentAngle = -maxAngle;
            direction = 1;
        }

        mainObj.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            GameObject enemy = other.gameObject;

            weaponControls.AddEnemy(enemy);

            if (exitTimers.ContainsKey(enemy))
            {
                StopCoroutine(exitTimers[enemy]);
                exitTimers[enemy] = StartCoroutine(RemoveAfterDelay(enemy));
            }
            else
            {
                exitTimers[enemy] = StartCoroutine(RemoveAfterDelay(enemy));
            }
        }
    }

    private IEnumerator RemoveAfterDelay(GameObject enemy)
    {
        yield return new WaitForSeconds(2f);

        weaponControls.RemoveEnemy(enemy);
        exitTimers.Remove(enemy);

        if (weaponControls.lockedEnemy == enemy)
        {
            if (weaponControls.enemyHudIcon != null)
            {
                weaponControls.enemyHudIcon.DeactivateLockedHud();
            }
            weaponControls.lockedEnemy = null;
        }
    }
}
