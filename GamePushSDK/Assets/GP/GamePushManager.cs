using GamePush;
using GP;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePushManager : MonoBehaviour
{
    public static GamePushManager Instance { get; private set; }

    [Header("GamePush Settings")]
    [SerializeField] private string gameId = "ваш_game_id";
    [SerializeField] private string secretKey = "ваш_secret_key";

    [Header("Debug")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool autoAuth = true;

    public event Action OnSDKInitialized;
    public event Action OnPlayerAuthorized;
    public event Action<string> OnCloudSaveSuccess;
    public event Action<string> OnCloudSaveFailed;
    public event Action<string> OnCloudLoadSuccess;
    public event Action<string> OnCloudLoadFailed;
    public event Action OnRewardedAdComplete;
    public event Action OnLeaderboardScoreSent;

    public bool IsInitialized { get; private set; }
    public bool IsAuthorized { get; private set; }
    public GP_Player Player { get; private set; }

    private Queue<CloudSaveOperation> pendingOperations = new Queue<CloudSaveOperation>();
    private Coroutine retryCoroutine;
    private const int MAX_RETRY_ATTEMPTS = 3;
    private const float RETRY_DELAY = 5f;

    private class CloudSaveOperation
    {
        public string key;
        public string value;
        public int retryCount;
        public Action<bool, string> callback;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSDK();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Инициализация

    public void InitializeSDK()
    {
        Log("🚀 Инициализация GamePush SDK...");

        GP_Settings.Init(gameId, secretKey);

        GP_Game.OnGameReady += OnSDKReady;
        GP_Game.OnGameError += OnSDKError;

        GP_Init.Initialize();
    }

    private void OnSDKReady()
    {
        IsInitialized = true;
        Log("✅ GamePush SDK инициализирован!");
        OnSDKInitialized?.Invoke();

        if (autoAuth)
        {
            AuthorizePlayer();
        }
    }

    private void OnSDKError(string error)
    {
        LogError($"❌ SDK Error: {error}");
    }

    #endregion

    #region Авторизация

    public void AuthorizePlayer()
    {
        if (!IsInitialized)
        {
            LogWarning("⏳ SDK не инициализирован, авторизация отложена");
            StartCoroutine(DelayedAuthorize());
            return;
        }

        Log("🔑 Авторизация игрока...");

        GP_Player.Authorize((success) =>
        {
            if (success)
            {
                Player = GP_Player.Current;
                IsAuthorized = true;
                Log($"✅ Авторизация успешна! ID: {GetPlayerId()}");
                Log($"   Имя: {GetPlayerName()}");
                Log($"   Уровень: {GetPlayerLevel()}");

                OnPlayerAuthorized?.Invoke();

                LoadCloudData();
            }
            else
            {
                LogError("❌ Ошибка авторизации");
                ShowMessage("Не удалось авторизоваться. Проверьте подключение к интернету.");
            }
        });
    }

    private IEnumerator DelayedAuthorize()
    {
        float timeout = 0;
        while (!IsInitialized && timeout < 10f)
        {
            timeout += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }

        if (IsInitialized)
            AuthorizePlayer();
        else
            LogError("❌ Таймаут инициализации SDK");
    }

    public string GetPlayerId()
    {
        return IsAuthorized ? Player.id.ToString() : "Not Authorized";
    }

    public string GetPlayerName()
    {
        return IsAuthorized ? Player.name : "Guest";
    }

    public int GetPlayerLevel()
    {
        return IsAuthorized ? Player.level : 0;
    }

    #endregion

    #region Облачные сохранения

    public void SaveCloudData(string key, string value, Action<bool, string> callback = null)
    {
        if (!IsAuthorized)
        {
            LogWarning("⚠️ Игрок не авторизован, сохранение в локальный кэш");
            CacheOperation(key, value, callback);
            return;
        }

        Log($"💾 Сохранение: {key} = {value}");

        var data = new Dictionary<string, string> { { key, value } };

        GP_Player.SetData(data);
        GP_Player.Sync((success) =>
        {
            if (success)
            {
                Log($"✅ Облачное сохранение успешно: {key}");
                OnCloudSaveSuccess?.Invoke(key);
                callback?.Invoke(true, key);
            }
            else
            {
                LogError($"❌ Ошибка сохранения: {key}");
                CacheOperation(key, value, callback);
                OnCloudSaveFailed?.Invoke(key);
                callback?.Invoke(false, key);
            }
        });
    }

    public void LoadCloudData(string key, Action<string> callback = null)
    {
        if (!IsAuthorized)
        {
            LogWarning("⚠️ Игрок не авторизован, загрузка из локального кэша");
            LoadFromCache(key, callback);
            return;
        }

        Log($"📂 Загрузка: {key}");

        GP_Player.Fetch((success) =>
        {
            if (success)
            {
                string value = GP_Player.GetString(key, "");
                Log($"✅ Загрузка успешна: {key} = {value}");
                OnCloudLoadSuccess?.Invoke(key);
                callback?.Invoke(value);
            }
            else
            {
                LogError($"❌ Ошибка загрузки: {key}");
                LoadFromCache(key, callback);
                OnCloudLoadFailed?.Invoke(key);
            }
        });
    }

    #endregion

    #region Локальный кэш и повторные попытки

    private void CacheOperation(string key, string value, Action<bool, string> callback)
    {
        var operation = new CloudSaveOperation
        {
            key = key,
            value = value,
            retryCount = 0,
            callback = callback
        };

        pendingOperations.Enqueue(operation);
        SaveToLocalCache(key, value);
        Log($"📦 Операция закеширована: {key} (в очереди: {pendingOperations.Count})");

        if (retryCoroutine == null)
        {
            retryCoroutine = StartCoroutine(ProcessPendingOperations());
        }
    }

    private IEnumerator ProcessPendingOperations()
    {
        while (pendingOperations.Count > 0)
        {
            var operation = pendingOperations.Peek();

            if (!IsAuthorized)
            {
                Log("⏳ Ожидание авторизации...");
                yield return new WaitForSeconds(RETRY_DELAY);
                continue;
            }

            Log($"🔄 Повторная попытка сохранения: {operation.key} (попытка {operation.retryCount + 1}/{MAX_RETRY_ATTEMPTS})");

            var data = new Dictionary<string, string> { { operation.key, operation.value } };

            GP_Player.SetData(data);
            GP_Player.Sync((success) =>
            {
                if (success)
                {
                    Log($"✅ Успешно сохранено из кэша: {operation.key}");
                    operation.callback?.Invoke(true, operation.key);
                    OnCloudSaveSuccess?.Invoke(operation.key);
                    pendingOperations.Dequeue();
                }
                else
                {
                    operation.retryCount++;
                    if (operation.retryCount >= MAX_RETRY_ATTEMPTS)
                    {
                        LogError($"❌ Превышен лимит попыток: {operation.key}");
                        operation.callback?.Invoke(false, operation.key);
                        pendingOperations.Dequeue();
                    }
                }
            });

            yield return new WaitForSeconds(RETRY_DELAY);
        }

        retryCoroutine = null;
    }

    private void SaveToLocalCache(string key, string value)
    {
        PlayerPrefs.SetString($"cached_{key}", value);
        PlayerPrefs.SetString($"cached_{key}_timestamp", DateTime.Now.ToString());
        PlayerPrefs.Save();
        Log($"💿 Локальный кэш сохранен: {key}");
    }

    private void LoadFromCache(string key, Action<string> callback)
    {
        if (PlayerPrefs.HasKey($"cached_{key}"))
        {
            string value = PlayerPrefs.GetString($"cached_{key}");
            string timestamp = PlayerPrefs.GetString($"cached_{key}_timestamp", "unknown");
            Log($"📀 Загружено из локального кэша: {key} = {value} (от {timestamp})");
            callback?.Invoke(value);
        }
        else
        {
            Log($"📀 Нет данных в кэше: {key}");
            callback?.Invoke("");
        }
    }

    public void ClearCache()
    {
        pendingOperations.Clear();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Log("🧹 Кэш очищен");
    }

    #endregion

    #region Лидерборд

    public void SendScoreToLeaderboard(string leaderboardId, int score)
    {
        if (!IsAuthorized)
        {
            LogWarning("⚠️ Игрок не авторизован, невозможно отправить рекорд");
            return;
        }

        Log($"🏆 Отправка рекорда: {leaderboardId} = {score}");

        GP_Leaderboard.PushScore(leaderboardId, score, (success) =>
        {
            if (success)
            {
                Log($"✅ Рекорд отправлен: {score}");
                OnLeaderboardScoreSent?.Invoke();
            }
            else
            {
                LogError($"❌ Ошибка отправки рекорда");
            }
        });
    }

    public void ShowLeaderboard(string leaderboardId)
    {
        GP_Leaderboard.Open(leaderboardId);
    }

    #endregion

    #region Реклама и монетизация

    public void ShowRewardedAd(string placementName = "rewarded", Action onComplete = null)
    {
        Log($"📢 Показ rewarded рекламы: {placementName}");

        GP_Ads.ShowRewarded(placementName, (result) =>
        {
            if (result.isSuccess)
            {
                Log("✅ Реклама просмотрена полностью");
                OnRewardedAdComplete?.Invoke();
                onComplete?.Invoke();
            }
            else
            {
                LogWarning($"⚠️ Реклама не завершена: {result.error}");
            }
        });
    }

    public void ShowInterstitialAd(string placementName = "interstitial")
    {
        Log($"📢 Показ interstitial рекламы: {placementName}");
        GP_Ads.ShowInterstitial(placementName);
    }

    #endregion

    #region UI Helpers

    private void Log(string message)
    {
        if (enableLogging)
            Debug.Log($"[GamePush] {message}");
    }

    private void LogWarning(string message)
    {
        if (enableLogging)
            Debug.LogWarning($"[GamePush] {message}");
    }

    private void LogError(string message)
    {
        if (enableLogging)
            Debug.LogError($"[GamePush] {message}");
    }

    private void ShowMessage(string message)
    {
        Debug.Log($"[GamePush UI] {message}");
    }

    #endregion

    private void OnDestroy()
    {
        GP_Game.OnGameReady -= OnSDKReady;
        GP_Game.OnGameError -= OnSDKError;
    }
}