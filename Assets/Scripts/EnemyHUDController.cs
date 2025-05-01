using TMPro;
using UnityEngine;

public class EnemyHUDController : MonoBehaviour
{
    public GameObject hudPrefab; 
    private GameObject hudInstance;
    private RectTransform hudRect;
    private TMP_Text distanceText;

    private Transform mainCamera;
    private Transform player;
    private Canvas canvas;

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
    }
}
