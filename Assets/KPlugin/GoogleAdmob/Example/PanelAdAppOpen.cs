using KTool.Advertisement;
using TMPro;
using UnityEngine;

namespace KPlugin.GoogleAdMob.Example
{
    public class PanelAdAppOpen : MonoBehaviour
    {
        #region Properties
        private const string CLICK_INIT = "Ad AppOpen: Click init",
            CLICK_LOAD = "Ad AppOpen: Click load",
            CLICK_SHOW = "Ad AppOpen: Click show";
        private const string AD_EVENT_INIT = "Ad AppOpen: even Init",
            AD_EVENT_LOADED = "Ad AppOpen: even Loaded {0}",
            AD_EVENT_DISPLAYED = "Ad AppOpen: even Displayed {0}",
            AD_EVENT_CLICKED = "Ad AppOpen: even Clicked",
            AD_EVENT_HIDDEN = "Ad AppOpen: even Hidden",
            AD_EVENT_REVENUE_PAID = "Ad AppOpen: even RevenuePaid {0}-{1}",
            AD_EVENT_DESTROY = "Ad AppOpen: even Destroy";
        private const string ERROR_ADD_EMPTY = "Ad AppOpen: No objects to select",
            ERROR_AD_IS_INITED = "Ad AppOpen: ad is inited",
            ERROR_AD_IS_NOT_INIT = "Ad AppOpen: ad not init",
            ERROR_AD_IS_LOADED = "Ad AppOpen: ad is loaded",
            ERROR_AD_SHOW = "Ad AppOpen show success",
            ERROR_AD_SHOW_FAIL = "Ad AppOpen show fail: {0}";

        [SerializeField]
        private TMP_Dropdown dropdownAd;

        private PanelLog panelLog;
        private AdMobAppOpen selectAd;

        private AdMobManager manager => AdMobManager.Instance;
        public bool IsShow => gameObject.activeSelf;
        public int Count => dropdownAd.options.Count;
        public AdMobAppOpen SelectAd => selectAd;
        #endregion

        #region Unity Events

        #endregion

        #region Methods
        public void Init(PanelLog panelLog)
        {
            this.panelLog = panelLog;
            //
            if (manager == null || manager.AppOpen_Count() == 0)
            {
                dropdownAd.options.Clear();
                return;
            }
            //
            dropdownAd.options.Clear();
            int count = manager.AppOpen_Count();
            for (int i = 0; i < count; i++)
            {
                AdMobAppOpen ad = manager.AppOpen_Get(i);
                dropdownAd.options.Add(new TMP_Dropdown.OptionData(ad.Name));
            }
            dropdownAd.value = 0;
        }
        public void Show()
        {
            if (IsShow)
                return;
            //
            if (Count == 0)
            {
                panelLog.AddLog(ERROR_ADD_EMPTY);
                return;
            }
            gameObject.SetActive(true);
            OnSelectAd(dropdownAd.value);
        }
        public void Hide()
        {
            if (!IsShow)
                return;
            //
            gameObject.SetActive(false);
            SelectAd_EventUnRegister();
            selectAd = null;
        }
        #endregion

        #region Unity Events
        public void OnSelectAd(int value)
        {
            SelectAd_EventUnRegister();
            selectAd = manager.AppOpen_Get(value);
            SelectAd_EventRegister();
        }
        public void OnClick_Init()
        {
            if (!IsShow)
                return;
            //
            panelLog.AddLog(CLICK_INIT);
            //
            if (SelectAd.IsInited)
                panelLog.AddLog(ERROR_AD_IS_INITED);
            else
                SelectAd.Init();
        }
        public void OnClick_Load()
        {
            if (!IsShow)
                return;
            //
            panelLog.AddLog(CLICK_LOAD);
            //
            if (!SelectAd.IsInited)
                panelLog.AddLog(ERROR_AD_IS_NOT_INIT);
            else if (SelectAd.IsLoaded)
                panelLog.AddLog(ERROR_AD_IS_LOADED);
            else
                SelectAd.Load();
        }
        public void OnClick_Show()
        {
            if (!IsShow)
                return;
            //
            panelLog.AddLog(CLICK_SHOW);
            //
            AdAppOpenTracking adTracking = SelectAd.Show();
            if (adTracking.IsShow)
                panelLog.AddLog(ERROR_AD_SHOW);
            else
                panelLog.AddLog(string.Format(ERROR_AD_SHOW_FAIL, adTracking.ErrorMessage));
        }
        #endregion

        #region Ad Event
        private void SelectAd_EventRegister()
        {
            if (selectAd == null)
                return;
            //
            selectAd.OnAdInited += SelectAd_OnAdInited;
            selectAd.OnAdLoaded += SelectAd_OnAdLoaded;
            selectAd.OnAdDisplayed += SelectAd_OnAdDisplayed;
            selectAd.OnAdClicked += SelectAd_OnAdClicked;
            selectAd.OnAdHidden += SelectAd_OnAdHidden;
            selectAd.OnAdRevenuePaid += SelectAd_OnAdRevenuePaid;
            selectAd.OnAdDestroy += SelectAd_OnAdDestroy;
        }
        private void SelectAd_EventUnRegister()
        {
            if (selectAd == null)
                return;
            //
            selectAd.OnAdInited -= SelectAd_OnAdInited;
            selectAd.OnAdLoaded -= SelectAd_OnAdLoaded;
            selectAd.OnAdDisplayed -= SelectAd_OnAdDisplayed;
            selectAd.OnAdClicked -= SelectAd_OnAdClicked;
            selectAd.OnAdHidden -= SelectAd_OnAdHidden;
            selectAd.OnAdRevenuePaid -= SelectAd_OnAdRevenuePaid;
            selectAd.OnAdDestroy -= SelectAd_OnAdDestroy;
        }
        private void SelectAd_OnAdInited(Ad adSource, bool isSuccess)
        {
            panelLog.AddLog(AD_EVENT_INIT);
        }
        private void SelectAd_OnAdLoaded(Ad adSource, bool isSuccess)
        {
            panelLog.AddLog(string.Format(AD_EVENT_LOADED, isSuccess));
        }
        private void SelectAd_OnAdDisplayed(Ad adSource, bool isSuccess)
        {
            panelLog.AddLog(string.Format(AD_EVENT_DISPLAYED, isSuccess));
        }
        private void SelectAd_OnAdClicked(Ad adSource)
        {
            panelLog.AddLog(AD_EVENT_CLICKED);
        }
        private void SelectAd_OnAdHidden(Ad adSource)
        {
            panelLog.AddLog(AD_EVENT_HIDDEN);
        }
        private void SelectAd_OnAdRevenuePaid(Ad adSource, AdRevenuePaid revenuePaid)
        {
            panelLog.AddLog(string.Format(AD_EVENT_REVENUE_PAID, revenuePaid.Value, revenuePaid.Currency));
        }
        private void SelectAd_OnAdDestroy(Ad adSource)
        {
            panelLog.AddLog(AD_EVENT_DESTROY);
        }
        #endregion
    }
}
