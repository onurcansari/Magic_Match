using System;
using System.Collections.Generic;
using UnityEngine;
#if ADS_ENABLED
using GoogleMobileAds.Api;
#endif

// Thin wrapper around the Google Mobile Ads Unity Plugin. Compiles and runs fine even
// before the SDK is installed: every real AdMob call lives behind "#if ADS_ENABLED".
// Add ADS_ENABLED to Project Settings > Player > Scripting Define Symbols once the SDK
// package is imported (see the project plan notes for the full external setup checklist).
// API shape below matches Google Mobile Ads Unity Plugin v9+; double-check method names
// against whatever version you actually install, Google revises this periodically.
public class AdsManager : MonoBehaviour
{
    private static AdsManager instance;

    [Header("Rewarded ad unit IDs (Google test IDs by default)")]
    public string androidRewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
    public string iosRewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";

    [Header("Banner ad unit IDs (Google test IDs by default)")]
    public string androidBannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
    public string iosBannerAdUnitId = "ca-app-pub-3940256099942544/2934735716";

#if ADS_ENABLED
    private RewardedAd rewardedAd;
    private readonly Dictionary<string, BannerView> bannerViews = new Dictionary<string, BannerView>();
#endif

    private static AdsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("AdsManager");
                instance = go.AddComponent<AdsManager>();
                DontDestroyOnLoad(go);
                instance.Init();
            }

            return instance;
        }
    }

    private void Init()
    {
#if ADS_ENABLED
        MobileAds.Initialize(initStatus => LoadRewardedAdInternal());
#endif
    }

    private string RewardedAdUnitId
    {
        get
        {
#if UNITY_IOS
            return iosRewardedAdUnitId;
#else
            return androidRewardedAdUnitId;
#endif
        }
    }

    private string BannerAdUnitId
    {
        get
        {
#if UNITY_IOS
            return iosBannerAdUnitId;
#else
            return androidBannerAdUnitId;
#endif
        }
    }

    public static void LoadRewardedAd()
    {
        Instance.LoadRewardedAdInternal();
    }

    private void LoadRewardedAdInternal()
    {
#if ADS_ENABLED
        RewardedAd.Load(RewardedAdUnitId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogWarning("AdsManager: rewarded ad failed to load: " + error);
                return;
            }

            rewardedAd = ad;
        });
#endif
    }

    public static bool IsRewardedAdReady
    {
        get
        {
#if ADS_ENABLED
            return Instance.rewardedAd != null && Instance.rewardedAd.CanShowAd();
#else
            return true;
#endif
        }
    }

    // Without the SDK installed, this grants the reward immediately so gameplay/testing
    // is never blocked on a missing ad system. Once ADS_ENABLED is defined, it shows a
    // real rewarded ad and only invokes onRewardEarned if the player actually finishes it.
    public static void ShowRewardedAd(Action onRewardEarned, Action onClosedWithoutReward)
    {
        Instance.ShowRewardedAdInternal(onRewardEarned, onClosedWithoutReward);
    }

    private void ShowRewardedAdInternal(Action onRewardEarned, Action onClosedWithoutReward)
    {
#if ADS_ENABLED
        if (rewardedAd == null || !rewardedAd.CanShowAd())
        {
            onClosedWithoutReward?.Invoke();
            LoadRewardedAdInternal();
            return;
        }

        bool earnedReward = false;

        rewardedAd.OnAdFullScreenContentClosed += () =>
        {
            LoadRewardedAdInternal();

            if (earnedReward)
            {
                onRewardEarned?.Invoke();
            }
            else
            {
                onClosedWithoutReward?.Invoke();
            }
        };

        rewardedAd.Show((Reward reward) => { earnedReward = true; });
#else
        onRewardEarned?.Invoke();
#endif
    }

    // slotId lets multiple independent banners exist at once (e.g. "left"/"right" of the
    // main menu). placeholderArea is a RectTransform sitting where the banner should
    // visually appear; its on-screen position is converted to pixel coordinates for the
    // native banner view.
    public static void ShowBanner(string slotId, RectTransform placeholderArea)
    {
        Instance.ShowBannerInternal(slotId, placeholderArea);
    }

    private void ShowBannerInternal(string slotId, RectTransform placeholderArea)
    {
#if ADS_ENABLED
        if (bannerViews.TryGetValue(slotId, out BannerView existing))
        {
            existing.Destroy();
        }

        Vector3[] corners = new Vector3[4];
        placeholderArea.GetWorldCorners(corners);

        Canvas canvas = placeholderArea.GetComponentInParent<Canvas>();
        Camera renderCamera = canvas != null ? canvas.worldCamera : null;
        Vector2 topLeftScreenPoint = RectTransformUtility.WorldToScreenPoint(renderCamera, corners[1]);

        int pixelX = Mathf.RoundToInt(topLeftScreenPoint.x);
        int pixelY = Mathf.RoundToInt(Screen.height - topLeftScreenPoint.y);

        BannerView bannerView = new BannerView(BannerAdUnitId, AdSize.MediumRectangle, pixelX, pixelY);
        bannerView.LoadAd(new AdRequest());
        bannerViews[slotId] = bannerView;
#endif
    }

    public static void HideBanner(string slotId)
    {
#if ADS_ENABLED
        if (instance != null && instance.bannerViews.TryGetValue(slotId, out BannerView bannerView))
        {
            bannerView.Hide();
        }
#endif
    }
}
