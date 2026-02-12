using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private ScreenOrientation lastOrientation;

    [Header("Settings")]
    [SerializeField] private bool logToConsole = true;
    [SerializeField] private bool simulateSafeArea = false;
    [SerializeField] private Rect simulatedSafeArea = new Rect(0, 0, 1080, 1920);

    [Header("Colors (для визуализации)")]
    [SerializeField] private bool visualizeSafeArea = true;
    [SerializeField] private Color safeAreaColor = new Color(0, 1, 0, 0.1f);
    [SerializeField] private Color unsafeAreaColor = new Color(1, 0, 0, 0.1f);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        ApplySafeArea();
    }

    private void Update()
    {
        if (Screen.safeArea != lastSafeArea || Screen.orientation != lastOrientation)
        {
            ApplySafeArea();
        }
    }

    public void ApplySafeArea()
    {
        Rect safeArea = simulateSafeArea ? simulatedSafeArea : Screen.safeArea;

        if (logToConsole)
        {
            LogSafeAreaInfo(safeArea);
        }

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        lastSafeArea = Screen.safeArea;
        lastOrientation = Screen.orientation;

        Debug.Log($"[SafeAreaFitter] ✅ Applied: {anchorMin:F3} - {anchorMax:F3}");
    }

    private void LogSafeAreaInfo(Rect safeArea)
    {
        Debug.Log("=== 📱 SAFE AREA INFO ===");
        Debug.Log($"📏 Screen: {Screen.width} x {Screen.height}");
        Debug.Log($"🧭 Orientation: {Screen.orientation}");
        Debug.Log($"🟢 SafeArea: X={safeArea.x:F0}, Y={safeArea.y:F0}, W={safeArea.width:F0}, H={safeArea.height:F0}");
        Debug.Log($"📱 Device: {SystemInfo.deviceModel}");
        Debug.Log("==========================");
    }

    [ContextMenu("📱 Simulate iPhone 14 Pro Max")]
    public void SimulateIPhone14ProMax()
    {
        simulatedSafeArea = new Rect(0, 132, 1290, 2634);
        simulateSafeArea = true;
        ApplySafeArea();
    }

    [ContextMenu("📱 Simulate iPhone SE")]
    public void SimulateIPhoneSE()
    {
        simulatedSafeArea = new Rect(0, 0, 750, 1334);
        simulateSafeArea = true;
        ApplySafeArea();
    }

    [ContextMenu("📱 Simulate Pixel 6 Pro")]
    public void SimulatePixel6Pro()
    {
        simulatedSafeArea = new Rect(0, 110, 1440, 2890);
        simulateSafeArea = true;
        ApplySafeArea();
    }

    [ContextMenu("🔄 Toggle Simulate Mode")]
    public void ToggleSimulateMode()
    {
        simulateSafeArea = !simulateSafeArea;
        ApplySafeArea();
    }

    [ContextMenu("🔄 Reset to Real SafeArea")]
    public void ResetToRealSafeArea()
    {
        simulateSafeArea = false;
        ApplySafeArea();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!visualizeSafeArea || rectTransform == null) return;

        Rect safeArea = simulateSafeArea ? simulatedSafeArea : Screen.safeArea;

        Gizmos.color = safeAreaColor;
        Vector3 topLeft = new Vector3(safeArea.x, Screen.height - safeArea.y, 0);
        Vector3 bottomRight = new Vector3(safeArea.x + safeArea.width, Screen.height - safeArea.y - safeArea.height, 0);
        Vector3 center = (topLeft + bottomRight) / 2;
        Vector3 size = new Vector3(safeArea.width, safeArea.height, 0);

        Gizmos.DrawCube(center, size);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = unsafeAreaColor;

        if (safeArea.y > 0)
        {
            Gizmos.DrawCube(
                new Vector3(Screen.width / 2, Screen.height - safeArea.y / 2, 0),
                new Vector3(Screen.width, safeArea.y, 0)
            );
        }

        if (Screen.height - (safeArea.y + safeArea.height) > 0)
        {
            float bottomHeight = Screen.height - (safeArea.y + safeArea.height);
            Gizmos.DrawCube(
                new Vector3(Screen.width / 2, bottomHeight / 2, 0),
                new Vector3(Screen.width, bottomHeight, 0)
            );
        }
    }
#endif
}