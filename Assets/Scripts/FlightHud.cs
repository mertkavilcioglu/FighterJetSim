using System.Buffers.Text;
using TMPro;
using UnityEngine;

public class FlightHud : MonoBehaviour
{
    public FlightControls flightControls; 
    public TMP_Text altitudeText;          
    public TMP_Text speedText;

    public RectTransform linesMove;
    public RectTransform linesRot;

    [Header("Horizon Settings")]
    public float horizonMoveFactor = 0.001f; // Z rotasyon baþýna Y hareketi oraný
    public float baseY = 0.003865f;

    private void Update()
    {
        if (flightControls == null) return;
        UpdateALTandSPD();
        UpdateHorizon();
    }

    private void UpdateALTandSPD()
    {
        altitudeText.text = $"{Mathf.RoundToInt(flightControls.GetAltitude())}";
        speedText.text = $"{Mathf.RoundToInt(flightControls.GetSimulatedSpeed())}";
    }

    private void UpdateHorizon()
    {
        if (linesMove == null) return;

        // Z rotasyonu al
        float zRotation = flightControls.transform.eulerAngles.z;

        if (zRotation > 180f)
            zRotation -= 360f;

        // Y pozisyonu hesapla
        float newY = baseY + (zRotation * horizonMoveFactor);

        // Pozisyonu uygula
        Vector2 newPos = linesRot.anchoredPosition;
        newPos.y = newY;
        linesRot.anchoredPosition = newPos;

        // --- BURAYA YENÝSÝ EKLENDÝ ---

        // X rotasyonu al
        float xRotation = flightControls.transform.eulerAngles.x;

        if (xRotation > 180f)
            xRotation -= 360f;

        // Lines'ýn z rotasyonunu ayarla (TAM TERSÝ YÖN)
        float newZRotation = -xRotation;

        // Lines'ýn rotation'ýný uygula
        linesMove.localRotation = Quaternion.Euler(0f, 0f, newZRotation);
    }

}
