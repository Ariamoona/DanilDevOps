using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GamePushUI : MonoBehaviour
{
    [Header("Auth Panel")]
    public GameObject authPanel;
    public TextMeshProUGUI playerIdText;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerLevelText;
    public Button authButton;

    [Header("Cloud Save Panel")]
    public InputField saveKeyInput;
    public InputField saveValueInput;
    public Button saveButton;
    public Button loadButton;
    public TextMeshProUGUI loadResultText;

    [Header("Leaderboard Panel")]
    public Button sendScoreButton;
    public Button showLeaderboardButton;
    public InputField scoreInput;

    [Header("Ads Panel")]
    public Button rewardedAdButton;
    public Button interstitialAdButton;

    [Header("Status")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI cacheStatusText;

    private void Start()
    {
        SetupButtons();
        SubscribeEvents();

        if (GamePushManager.Instance == null)
        {
            Debug.LogError(" GamePushManager не найден!");
        }
    }

    private void SetupButtons()
    {
        if (authButton) authButton.onClick.AddListener(() =>
            GamePushManager.Instance?.AuthorizePlayer());

        if (saveButton) saveButton.onClick.AddListener(OnSaveButtonClick);
        if (loadButton) loadButton.onClick.AddListener(OnLoadButtonClick);

        if (sendScoreButton) sendScoreButton.onClick.AddListener(OnSendScoreClick);
        if (showLeaderboardButton) showLeaderboardButton.onClick.AddListener(OnShowLeaderboardClick);

        if (rewardedAdButton) rewardedAdButton.onClick.AddListener(() =>
            GamePushManager.Instance?.ShowRewardedAd("rewarded", () => ShowMessage("Награда получена!")));

        if (interstitialAdButton) interstitialAdButton.onClick.AddListener(() =>
            GamePushManager.Instance?.ShowInterstitialAd("interstitial"));
    }

    private void SubscribeEvents()
    {
        var mgr = GamePushManager.Instance;
        if (mgr == null) return;

        mgr.OnPlayerAuthorized += UpdatePlayerInfo;
        mgr.OnCloudSaveSuccess += (key) => ShowMessage($" Сохранено: {key}");
        mgr.OnCloudLoadSuccess += (key) => ShowMessage($" Загружено: {key}");
        mgr.OnCloudSaveFailed += (key) => ShowMessage($" Ошибка сохранения: {key}");
        mgr.OnCloudLoadFailed += (key) => ShowMessage($" Ошибка загрузки: {key}");
        mgr.OnRewardedAdComplete += () => ShowMessage(" Награда выдана!");
    }

    private void UpdatePlayerInfo()
    {
        if (playerIdText) playerIdText.text = $"ID: {GamePushManager.Instance.GetPlayerId()}";
        if (playerNameText) playerNameText.text = $"Name: {GamePushManager.Instance.GetPlayerName()}";
        if (playerLevelText) playerLevelText.text = $"Level: {GamePushManager.Instance.GetPlayerLevel()}";

        ShowMessage(" Авторизация успешна!");
    }

    private void OnSaveButtonClick()
    {
        if (!string.IsNullOrEmpty(saveKeyInput.text) && !string.IsNullOrEmpty(saveValueInput.text))
        {
            GamePushManager.Instance.SaveCloudData(saveKeyInput.text, saveValueInput.text);
        }
    }

    private void OnLoadButtonClick()
    {
        if (!string.IsNullOrEmpty(saveKeyInput.text))
        {
            GamePushManager.Instance.LoadCloudData(saveKeyInput.text, (value) =>
            {
                loadResultText.text = $"Загружено: {value}";
                saveValueInput.text = value;
            });
        }
    }

    private void OnSendScoreClick()
    {
        if (int.TryParse(scoreInput.text, out int score))
        {
            GamePushManager.Instance.SendScoreToLeaderboard("main_leaderboard", score);
        }
    }

    private void OnShowLeaderboardClick()
    {
        GamePushManager.Instance.ShowLeaderboard("main_leaderboard");
    }

    private void ShowMessage(string message)
    {
        Debug.Log($"[UI] {message}");
        if (statusText) statusText.text = message;

        Invoke(nameof(ClearStatus), 3f);
    }

    private void ClearStatus()
    {
        if (statusText) statusText.text = "";
    }

    private void Update()
    {
        if (Time.frameCount % 120 == 0)
        {
            UpdateCacheStatus();
        }
    }

    private void UpdateCacheStatus()
    {
        if (cacheStatusText == null) return;

        bool isAuthorized = GamePushManager.Instance?.IsAuthorized ?? false;
        string status = $"SDK: {(GamePushManager.Instance?.IsInitialized ?? false ? "" : "")}\n";
        status += $"Auth: {(isAuthorized ? "" : "")}\n";
        status += $"Cache: Active";

        cacheStatusText.text = status;
    }
}
