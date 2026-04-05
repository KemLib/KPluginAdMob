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
        private const string ERROR_LOAD_FAIL = "Ad load fail code: {0}",
            ERROR_IS_DESTROY = "Ad is destroy";

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMobAdType.Banner)]
        private int indexAd = 0;

        private bool isIniting,
            isLoading;
        private int attemptLoad;
        private BannerView adObject;
        private InitTrackingSource initTrackingSource;
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
            get => true;
            protected set
            {

            }
        }
        public override KTool.Advertisement.AdPosition PositionType
        {
            get => base.PositionType;
            protected set
            {
                base.PositionType = value;
                if (IsInited)
                {
                    adObject.SetPosition(Utility.ConvertPosition(base.PositionType));
                }
            }
        }
        public override Vector2 Position
        {
            get => base.Position;
            protected set
            {
                base.Position = value;
                if (IsInited)
                {
                    Vector2 point = Utility.Convert_UnityToAdMob(base.Position);
                    adObject.SetPosition((int)point.x, (int)point.y);
                }
            }
        }
        #endregion

        #region Unity Event
        public IInitTracking InitBegin()
        {
            if (IsDestroy || IsInited || initTrackingSource != null)
                return IInitTracking.Fail;
            //
            initTrackingSource = new InitTrackingSource(initIndispensable);
            OnAdInited += Init_OnAdInited;
            Init();
            return initTrackingSource;
        }
        public void InitEnd()
        {

        }
        private void Init_OnAdInited(Ad source, bool isSuccess)
        {
            OnAdInited -= Init_OnAdInited;
            if (isSuccess)
            {
                OnAdLoaded += Init_OnLoaded;
                if (!isLoading)
                {
                    isLoading = true;
                    StartCoroutine(Ad_Load());
                }
            }
            else
            {
                initTrackingSource.CompleteFail();
                initTrackingSource = null;
            }
        }
        private void Init_OnLoaded(Ad source, bool isSuccess)
        {
            OnAdLoaded -= Init_OnLoaded;
            if (isSuccess)
                initTrackingSource.CompleteSuccess();
            else
                initTrackingSource.CompleteFail();
            initTrackingSource = null;
        }
        #endregion

        #region Method
        public override void Init()
        {
            if (IsInited || isIniting)
                return;
            isIniting = true;
            //
            if (setInstance)
                instance = this;
            StartCoroutine(Ad_Create());
        }
        public override void Load()
        {
            if (!IsInited || IsLoaded || IsAutoReload || isLoading)
                return;
            isLoading = true;
            //
            StartCoroutine(Ad_Load());
        }
        public override void Destroy()
        {
            IsDestroy = true;
            if (isIniting || isLoading)
                return;
            if (IsInited)
            {
                if (IsShow)
                    Hide();
                Ad_Destroy();
            }
            else
                PushEvent_Destroy();
        }
        public override IAdBannerTracking Show()
        {
            if (IsDestroy)
                return new AdBannerTrackingSource(this, ERROR_IS_DESTROY);
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
                adTrackingSource.PushEvent_Hidden();
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
            adTrackingSource.PushEvent_Displayed(true);
        }
        #endregion

        #region Ad
        private IEnumerator Ad_Create()
        {
            while (!AdMobManager.Instance.IsInit)
                yield return new WaitForEndOfFrame();
            //
            if (IsDestroy)
            {
                isIniting = false;
                PushEvent_Inited(false);
                //
                PushEvent_Destroy();
                yield break;
            }
            //
            adObject = Utility.Create_AdBanner(AdId, SizeType, PositionType, Size, Position);
            Ad_EventRegister();
            //
            IsInited = true;
            isIniting = false;
            PushEvent_Inited(true);
        }
        private void Ad_Destroy()
        {
            if (adObject != null)
            {
                adObject.Destroy();
                adObject = null;
            }
            PushEvent_Destroy();
        }
        private IEnumerator Ad_Load()
        {
            if (attemptLoad > 0)
                yield return new WaitForSecondsRealtime(attemptLoad * 2);
            else
                yield return new WaitForEndOfFrame();
            //
            if (IsDestroy)
            {
                isLoading = false;
                PushEvent_Loaded(false);
                //
                Ad_Destroy();
                yield break;
            }
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
            attemptLoad = 0;
            IsLoaded = true;
            isLoading = false;
            //
            if (IsDestroy)
            {
                PushEvent_Loaded(true);
                //
                Ad_Destroy();
            }
            else
            {
                PushEvent_Loaded(true);
                if (IsShow)
                {
                    adObject.Show();
                    PushEvent_Displayed(true);
                    adTrackingSource.PushEvent_Displayed(true);
                }
                else
                {
                    adObject.Hide();
                }
            }
        }
        private void Ad_OnLoadFailed(LoadAdError error)
        {
            if (error != null)
                Debug.LogWarning(string.Format(ERROR_LOAD_FAIL, error.GetCode()));
            //
            attemptLoad = Mathf.Min(attemptLoad + 1, 6);
            IsLoaded = false;
            if (IsDestroy)
            {
                isLoading = false;
                PushEvent_Loaded(false);
                //
                Ad_Destroy();
            }
            else
            {
                if (IsAutoReload)
                    StartCoroutine(Ad_Load());
                else
                    isLoading = false;
                PushEvent_Loaded(false);
            }
        }
        private void Ad_OnFullScreenContentOpened()
        {
            PushEvent_Expanded(true);
            adTrackingSource.PushEvent_Expanded(true);
        }
        private void Ad_OnFullScreenContentClosed()
        {
            PushEvent_Expanded(false);
            adTrackingSource.PushEvent_Expanded(false);
        }
        private void Ad_OnClicked()
        {
            PushEvent_Clicked();
            adTrackingSource.PushEvent_Clicked();
        }
        private void Ad_OnPaid(AdValue adValue)
        {
            if (adValue == null)
                return;
            //
            AdRevenuePaid revenuePaid = new AdRevenuePaid(
                source: AdMobManager.ADMOB_SCOURCE,
                network_name: AdMobManager.ADMOB_SCOURCE,
                idAd: AdId,
                adType: AdType,
                countryCode: AdMobManager.ADMOB_COUNTRY_CODE,
                placement: string.Empty,
                value: adValue.Value / AdMobManager.VALUE_SCALE,
                currency: adValue.CurrencyCode);
            //
            PushEvent_RevenuePaid(revenuePaid);
            adTrackingSource.PushEvent_RevenuePaid(revenuePaid);
        }
        private void Ad_OnImpressionRecorded()
        {
            OnAdImpressionRecorded?.Invoke();
        }
        #endregion
    }
}
