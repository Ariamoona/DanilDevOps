using UnityEngine;

public class SafeAreaMonitor : MonoBehaviour
{
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private float checkInterval = 0.5f;

    private Rect lastSafeArea;
    private ScreenOrientation lastOrientation;
    private float timer;

    private void Start()
    {
        lastSafeArea = Screen.safeArea;
        lastOrientation = Screen.orientation;
        timer = checkInterval;

        LogInitialInfo();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            CheckForChanges();
            timer = checkInterval;
        }
    }

    private void CheckForChanges()
    {
        if (Screen.safeArea != lastSafeArea)
        {
            Debug.Log($" Safe Area CHANGED!");
            Debug.Log($"   From: {lastSafeArea}");
            Debug.Log($"   To: {Screen.safeArea}");
            lastSafeArea = Screen.safeArea;
        }

        if (Screen.orientation != lastOrientation)
        {
            Debug.Log($" Orientation CHANGED: {lastOrientation} → {Screen.orientation}");
            lastOrientation = Screen.orientation;
        }
    }

    private void LogInitialInfo()
    {
        Debug.Log("===  DEVICE INFO ===");
        Debug.Log($"Device Model: {SystemInfo.deviceModel}");
        Debug.Log($"Device Type: {SystemInfo.deviceType}");
        Debug.Log($"Screen: {Screen.width} x {Screen.height}");
        Debug.Log($"DPI: {Screen.dpi}");
        Debug.Log($"Orientation: {Screen.orientation}");
        Debug.Log($"SafeArea: {Screen.safeArea}");
        Debug.Log($"Resolution: {Screen.currentResolution}");
        Debug.Log("======================");
    }

    [ContextMenu(" Log Current Status")]
    public void LogCurrentStatus()
    {
        LogInitialInfo();
    }
}
