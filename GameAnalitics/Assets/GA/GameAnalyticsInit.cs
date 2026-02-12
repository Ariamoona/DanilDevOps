using UnityEngine;
using GameAnalyticsSDK;

public class GameAnalyticsInit : MonoBehaviour
{
    void Start()
    {
        GameAnalytics.SetEnabledManualInitialization(true); 
        GameAnalytics.SetEnabledLog(true);                 
        GameAnalytics.SetBuildVersion(Application.version);

        GameAnalytics.Initialize();

        Debug.Log("[GA] SDK Initialized manually");
    }
}