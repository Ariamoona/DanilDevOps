using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class AdsService : MonoBehaviour
{
    public static AdsService Instance { get; private set; }

    [Header("Game IDs")]
    public string androidGameId = "1234567";
    public string iosGameId = "7654321";
    
    [Header("Settings")]
    public bool testMode = true;
    
    private string gameId;
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAds()
    {
        #if UNITY_ANDROID
            gameId = androidGameId;
        #elif UNITY_IOS
            gameId = iosGameId;
        #else
            gameId = androidGameId;
        #endif

        if (string.IsNullOrEmpty(gameId) || gameId == "1234567")
        {
            Debug.LogError("[AdsService] ❌ Введите Game ID в инспекторе!");
            return;
        }

        if (!Advertisement.isInitialized)
        {
            Advertisement.Initialize(gameId, testMode);
            isInitialized = true;
            Debug.Log($"[AdsService] 🚀 Инициализация... GameID: {gameId}, TestMode: {testMode}");
        }
    }

    // ============= БАННЕР =============
    public void ShowBanner()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.Log("[AdsService] ⏳ SDK не инициализирован");
            return;
        }

        // Проверяем готовность баннера
        if (Advertisement.Banner != null)
        {
            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            Advertisement.Banner.Show("banner");
            Debug.Log("[AdsService] 📢 Баннер показан");
        }
        else
        {
            Debug.Log("[AdsService] ⏳ Баннер не готов");
        }
    }

    public void HideBanner()
    {
        if (Advertisement.Banner != null)
        {
            Advertisement.Banner.Hide();
            Debug.Log("[AdsService] 🔇 Баннер скрыт");
        }
    }

    // ============= INTERSTITIAL =============
    public void ShowInterstitial()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.Log("[AdsService] ⏳ SDK не инициализирован");
            return;
        }

        if (Advertisement.IsReady("video"))
        {
            ShowOptions options = new ShowOptions();
            options.resultCallback = HandleInterstitialResult;
            Advertisement.Show("video", options);
            Debug.Log("[AdsService] 🎬 Interstitial показан");
        }
        else
        {
            Debug.Log("[AdsService] ⏳ Interstitial не готов");
        }
    }

    private void HandleInterstitialResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("[AdsService] ✅ Interstitial завершен");
                break;
            case ShowResult.Skipped:
                Debug.Log("[AdsService] ⏩ Interstitial пропущен");
                break;
            case ShowResult.Failed:
                Debug.Log("[AdsService] ❌ Interstitial ошибка");
                break;
        }
    }

    // ============= REWARDED =============
    public void ShowRewardedVideo()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.Log("[AdsService] ⏳ SDK не инициализирован");
            return;
        }

        if (Advertisement.IsReady("rewardedVideo"))
        {
            ShowOptions options = new ShowOptions();
            options.resultCallback = HandleRewardedResult;
            Advertisement.Show("rewardedVideo", options);
            Debug.Log("[AdsService] ⭐ Rewarded видео показано");
        }
        else
        {
            Debug.Log("[AdsService] ⏳ Rewarded видео не готово");
        }
    }

    private void HandleRewardedResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("[AdsService] 🎉 НАГРАДА ВЫДАНА! +100 монет");
                // Здесь выдаем награду
                PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins", 0) + 100);
                break;
            case ShowResult.Skipped:
                Debug.Log("[AdsService] ⏩ Rewarded пропущено - награда не выдана");
                break;
            case ShowResult.Failed:
                Debug.Log("[AdsService] ❌ Rewarded ошибка показа");
                break;
        }
    }

    // ============= ТЕСТОВЫЕ МЕТОДЫ =============
    [ContextMenu("📢 Показать баннер")]
    public void TestShowBanner()
    {
        ShowBanner();
    }

    [ContextMenu("🔇 Скрыть баннер")]
    public void TestHideBanner()
    {
        HideBanner();
    }

    [ContextMenu("🎬 Показать Interstitial")]
    public void TestShowInterstitial()
    {
        ShowInterstitial();
    }

    [ContextMenu("⭐ Показать Rewarded")]
    public void TestShowRewarded()
    {
        ShowRewardedVideo();
    }

    [ContextMenu("📊 Статус")]
    public void TestStatus()
    {
        Debug.Log("===== 📊 СТАТУС ADS =====");
        Debug.Log($"SDK Инициализирован: {Advertisement.isInitialized}");
        Debug.Log($"Game ID: {gameId}");
        Debug.Log($"Test Mode: {testMode}");
        Debug.Log($"Banner готов: {Advertisement.IsReady("banner")}");
        Debug.Log($"Interstitial готов: {Advertisement.IsReady("video")}");
        Debug.Log($"Rewarded готов: {Advertisement.IsReady("rewardedVideo")}");
        Debug.Log("========================");
    }

    private void Update()
    {
        // Показываем статус каждые 10 секунд для отладки
        if (isInitialized && Time.frameCount % 600 == 0)
        {
            TestStatus();
        }
    }
}