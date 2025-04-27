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
    public float horizonMoveFactor = 0.001f; 
    public float baseY = -0.0855f;


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

        float zRotation = flightControls.transform.eulerAngles.z;

        if (zRotation > 180f)
            zRotation -= 360f;

        float newY = baseY + (zRotation * horizonMoveFactor);

        Vector2 newPos = linesRot.anchoredPosition;
        newPos.y = newY;
        linesRot.anchoredPosition = newPos;

        // Angle to ground
        Vector3 forward = -flightControls.transform.right; 
        Vector3 right = -flightControls.transform.forward;    

        float angleToGround = Vector3.Angle(Vector3.up, forward); 

        float newVerticalPos = baseY + (angleToGround * horizonMoveFactor);

        Vector2 adjustedPos = linesRot.anchoredPosition;
        adjustedPos.y = newVerticalPos;
        linesRot.anchoredPosition = adjustedPos;


        // Roll
        float xRotation = flightControls.gameObject.transform.localRotation.x; 
        //Debug.Log(xRotation);
        if (xRotation > 180f)
            xRotation -= 360f;

        float newZRotation = -xRotation;

        Quaternion currentRotation = linesMove.rotation;
        linesMove.rotation = Quaternion.Lerp(currentRotation, Quaternion.Euler(currentRotation.eulerAngles.x, currentRotation.eulerAngles.y, newZRotation), Time.fixedDeltaTime * 7.5f);  
    }
}
