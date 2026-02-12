using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AdsService))]
public class AdsServiceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AdsService ads = (AdsService)target;

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("🧪 ТЕСТОВЫЙ ЦЕНТР", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("▶️ Запустите сцену для тестирования рекламы", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("📢 ПОКАЗАТЬ БАННЕР", GUILayout.Height(35)))
            ads.TestShowBanner();

        if (GUILayout.Button("🔇 СКРЫТЬ БАННЕР", GUILayout.Height(35)))
            ads.TestHideBanner();

        EditorGUILayout.Space(5);

        if (GUILayout.Button("🎬 INTERSTITIAL", GUILayout.Height(35)))
            ads.TestShowInterstitial();

        if (GUILayout.Button("⭐ REWARDED", GUILayout.Height(35)))
            ads.TestShowRewarded();

        EditorGUILayout.Space(5);

        if (GUILayout.Button("📊 СТАТУС", GUILayout.Height(35)))
            ads.TestStatus();

        EditorGUILayout.EndVertical();

        if (ads.androidGameId == "1234567" || string.IsNullOrEmpty(ads.androidGameId))
        {
            EditorGUILayout.HelpBox("⚠️ Введите ваш Android Game ID из Unity Dashboard!", MessageType.Warning);
        }
    }
}