using UnityEngine;
using GameAnalyticsSDK;

public class GAEventsTester : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(SendAllEvents), 2f);
    }

    void SendAllEvents()
    {
        SendProgressionEvents();     
        SendDesignEvents();          
        SendResourceEvents();         
        SendErrorEvent();            
    }

    void SendProgressionEvents()
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "world_1", "level_3");
        Debug.Log("[GA] Progression: Start world_1/level_3");

        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "world_1", "level_3", 92); 
        Debug.Log("[GA] Progression: Complete world_1/level_3 (92s)");

    }

    void SendDesignEvents()
    {
        GameAnalytics.NewDesignEvent("shop:open");
        Debug.Log("[GA] Design: shop:open");

        GameAnalytics.NewDesignEvent("enemy:kill:robot_large", 1); 
        Debug.Log("[GA] Design: enemy:kill:robot_large (1)");
    }

    void SendResourceEvents()
    {
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "gold", 50, "weapon", "sword_of_flame");
        Debug.Log("[GA] Resource: Sink 50 gold on weapon:sword_of_flame");
    }

    void SendErrorEvent()
    {
        GameAnalytics.NewErrorEvent(GAErrorSeverity.Error, "Manual test error — button pressed without internet");
        Debug.Log("[GA] Error: Manual test error sent");
    }

    [ContextMenu("Send Progression Start")]
    void TestProgStart() => GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "test_level", "1");

    [ContextMenu("Send Design (shop)")]
    void TestDesignShop() => GameAnalytics.NewDesignEvent("shop:open");

    [ContextMenu("Send Resource (spend gold)")]
    void TestResourceSpend() => GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "gold", 10, "potion", "health_potion");

    [ContextMenu("Send Error")]
    void TestError() => GameAnalytics.NewErrorEvent(GAErrorSeverity.Warning, "Test warning from editor");
}