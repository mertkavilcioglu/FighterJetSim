using TMPro;
using UnityEngine;

public class EnemyHUDController : MonoBehaviour
{
    public GameObject hudPrefab;
    public GameObject lockedHudPrefab;
    private GameObject hudInstance;
    private RectTransform hudRect;
    private TMP_Text distanceText;

    private Transform mainCamera;
    private Transform player;
    private Canvas canvas;

    private GameObject lockedHudInstance;

    void Start()
    {
        mainCamera = Camera.main.transform;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        canvas = GameObject.FindGameObjectWithTag("HMD").GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas is null!");
            return;
        }

        hudInstance = Instantiate(hudPrefab, canvas.transform);
        hudRect = hudInstance.GetComponent<RectTransform>();
        distanceText = hudInstance.GetComponentInChildren<TMP_Text>();
    }

    void FixedUpdate()
    {
        if (mainCamera == null || hudInstance == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        //Debug.Log("Screen Position: " + screenPos);

        if (screenPos.z > 0)
        {
            hudInstance.SetActive(true);
            hudRect.position = screenPos;

            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                distanceText.text = Mathf.RoundToInt(distance) + " m";
            }
        }
        else
        {
            hudInstance.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (hudInstance != null)
            Destroy(hudInstance);

        DeactivateLockedHud();
    }

    public void ActivateLockedHud()
    {
        if (lockedHudPrefab == null || hudInstance == null) return;

        if (lockedHudInstance == null)
        {
            lockedHudInstance = Instantiate(lockedHudPrefab, hudInstance.transform);
            lockedHudInstance.transform.localPosition = Vector3.zero;
            lockedHudInstance.transform.localRotation = Quaternion.identity;
            //lockedHudInstance.transform.localScale = Vector3.one;
        }
    }

    public void DeactivateLockedHud()
    {
        if (lockedHudInstance != null)
        {
            Destroy(lockedHudInstance);
            lockedHudInstance = null;
        }
    }
}
