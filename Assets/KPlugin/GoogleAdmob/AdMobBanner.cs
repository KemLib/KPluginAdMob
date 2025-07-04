using GoogleMobileAds.Api;
using KTool;
using KTool.Advertisement;
using KTool.Init;
using System;
using System.Collections;
using UnityEngine;

namespace KPlugin.GoogleAdMob
{
    public class AdMobBanner : AdBanner, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad Banner load fail code: {0}",
            ERROR_IS_DESTROY = "Ad Banner is destroy";

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMobAdType.Banner)]
        private int indexAd = 0;

        private int attemptLoad;
        private BannerView adObject;
        private InitTrackingSource initTrackingSource;
        private Coroutine coroutineLoading;
        private AdBannerTrackingSource adTrackingSource;

        public event Action OnAdImpressionRecorded;


        public string AdId
        {
            get
            {
                AdMobSettingAdId settingAdId = AdMobSetting.Instance.Ad_Get(AdMobAdType.Banner, indexAd);
                if (settingAdId != null)
                    return settingAdId.AdID;
                return string.Empty;
            }
        }
        public override bool IsAutoReload
        {
            get => base.IsAutoReload;
            protected set
            {
                if (value == base.IsAutoReload)
                    return;
                base.IsAutoReload = value;
                if (base.IsAutoReload)
                    Load();
            }
        }
        public override bool IsReady => base.IsReady && adObject != null;
        public override KTool.Advertisement.AdPosition PositionType
        {
            get => base.PositionType;
            protected set
            {
                if (value == base.PositionType)
                    return;
                //
                base.PositionType = value;
                if (adObject != null)
                    adObject.SetPosition(Utility.ConvertPosition(base.PositionType));
            }
        }
        public override Vector2 Position
        {
            get => base.Position;
            protected set
            {
                if (value == base.Position)
                    return;
                //
                base.Position = value;
                if (adObject != null)
                {
                    Vector2 point = Utility.Convert_UnityToAdMob(base.Position);
                    adObject.SetPosition((int)point.x, (int)point.y);
                }
            }
        }
        #endregion

        #region Unity Event
        private void OnDestroy()
        {
            if (instance != null && instance.GetInstanceID() == GetInstanceID())
                instance = null;
            Destroy();
        }
        #endregion

        #region Init
        public InitTracking InitBegin()
        {
            initTrackingSource = new InitTrackingSource(initIndispensable);
            Load();
            return initTrackingSource;
        }
        public void InitEnd()
        {

        }
        #endregion

        #region Method
        public override void Init()
        {
            if (IsInited)
                return;
            //
            if (setInstance)
                instance = this;
            IsInited = true;
            PushEvent_Inited();
        }
        public override void Load()
        {
            Init();
            //
            if (IsLoaded)
                return;
            //
            Ad_Create();
        }
        public override void Destroy()
        {
            IsDestroy = true;
            Ad_Destroy();
            PushEvent_Destroy();
        }
        public override AdBannerTracking Show()
        {
            if (IsDestroy)
                return new AdBannerTrackingSource(ERROR_IS_DESTROY);
            //
            if (IsShow)
            {
                return adTrackingSource;
            }
            else
            {
                adTrackingSource = new AdBannerTrackingSource(this);
                IsShow = true;
                if (IsLoaded)
                    CoroutineManager.Instance.Coroutine_Start(Delay_DisplayedAd());
                return adTrackingSource;
            }
        }
        public override void Hide()
        {
            if (IsShow)
            {
                adObject.Hide();
                IsShow = false;
                //
                PushEvent_Hidden();
                adTrackingSource.Hidden();
                adTrackingSource = null;
            }
            else
            {
                IsShow = false;
            }
        }
        private IEnumerator Delay_DisplayedAd()
        {
            yield return new WaitForEndOfFrame();
            adObject.Show();
            PushEvent_Displayed(true);
            adTrackingSource.Displayed(true);
        }
        #endregion

        #region Ad
        private void Ad_Create()
        {
            if (coroutineLoading != null)
                return;
            //
            coroutineLoading = CoroutineManager.Instance.Coroutine_Start(Ad_LoadAd());
        }
        private void Ad_Destroy()
        {
            if (coroutineLoading != null)
            {
                StopCoroutine(coroutineLoading);
                coroutineLoading = null;
            }
            //
            if (adObject == null)
            {
                IsShow = false;
                return;
            }
            //
            if (IsLoaded)
            {
                if (IsShow)
                {
                    adObject.Hide();
                    IsShow = false;
                    //
                    PushEvent_Hidden();
                    adTrackingSource.Hidden();
                    adTrackingSource = null;
                }
                IsLoaded = false;
            }
            else if (IsShow)
                IsShow = false;
            //
            adObject.Destroy();
            adObject = null;
        }
        private IEnumerator Ad_LoadAd()
        {
            if (attemptLoad > 0)
            {
                float delay = Mathf.Pow(2, attemptLoad);
                yield return new WaitForSecondsRealtime(delay);
            }
            //
            while (!AdMobManager.IsInit)
                yield return new WaitForEndOfFrame();
            //
            adObject = Utility.Create_AdBanner(AdId, SizeType, PositionType, Size, Position);
            Ad_EventRegister();
            //
            var adRequest = new AdRequest();
            adObject.LoadAd(adRequest);
        }
        private void Ad_EventRegister()
        {
            adObject.OnBannerAdLoaded += Ad_OnLoaded;
            adObject.OnBannerAdLoadFailed += Ad_OnLoadFailed;
            adObject.OnAdFullScreenContentOpened += Ad_OnFullScreenContentOpened;
            adObject.OnAdFullScreenContentClosed += Ad_OnFullScreenContentClosed;
            adObject.OnAdClicked += Ad_OnClicked;
            adObject.OnAdPaid += Ad_OnPaid;
            adObject.OnAdImpressionRecorded += Ad_OnImpressionRecorded;
        }
        private void Ad_OnLoaded()
        {
            coroutineLoading = null;
            attemptLoad = 0;
            IsLoaded = true;
            //
            if (initTrackingSource != null)
            {
                initTrackingSource.CompleteSuccess();
                initTrackingSource = null;
            }
            //
            if (IsShow)
                adObject.Show();
            else
                adObject.Hide();
            PushEvent_Loaded(true);
        }
        private void Ad_OnLoadFailed(LoadAdError error)
        {
            coroutineLoading = null;
            attemptLoad = Mathf.Min(attemptLoad + 1, 6);
            //
            if (error != null)
                Debug.LogError(string.Format(ERROR_LOAD_FAIL, error.GetCode()));
            if (initTrackingSource != null)
            {
                initTrackingSource.CompleteFail();
                initTrackingSource = null;
            }
            //
            PushEvent_Loaded(false);
            if (!IsDestroy && IsAutoReload)
            {
                var adRequest = new AdRequest();
                adObject.LoadAd(adRequest);
            }
        }
        private void Ad_OnFullScreenContentOpened()
        {
            PushEvent_Expanded(true);
            adTrackingSource.Expanded(true);
        }
        private void Ad_OnFullScreenContentClosed()
        {
            PushEvent_Expanded(false);
            adTrackingSource.Expanded(false);
        }
        private void Ad_OnClicked()
        {
            PushEvent_Clicked();
            adTrackingSource.Clicked();
        }
        private void Ad_OnPaid(AdValue adValue)
        {
            if (adValue == null)
                return;
            //
            AdRevenuePaid revenuePaid = new AdRevenuePaid(
                AdMobManager.ADMOB_SCOURCE,
                AdMobManager.ADMOB_SCOURCE,
                AdId,
                AdMobManager.ADMOB_COUNTRY_CODE,
                AdType,
                adValue.Value,
                adValue.CurrencyCode);
            //
            PushEvent_RevenuePaid(revenuePaid);
            adTrackingSource.RevenuePaid(revenuePaid);
        }
        private void Ad_OnImpressionRecorded()
        {
            OnAdImpressionRecorded?.Invoke();
        }
        #endregion
    }
}
