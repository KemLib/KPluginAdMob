using KTool.Advertisement;
using TMPro;
using UnityEngine;

namespace KPlugin.AdMob.Example
{
    public class PanelAdRewarded : MonoBehaviour
    {
        #region Properties
        private const string AD_EVENT_LOADED = "Ad Rewarded: even Loaded {0}",
            AD_EVENT_DISPLAYED = "Ad Rewarded: even Displayed {0}",
            AD_EVENT_CLICKED = "Ad Rewarded: even Clicked",
            AD_EVENT_HIDDEN = "Ad Rewarded: even Hidden",
            AD_EVENT_REVENUE_PAID = "Ad Rewarded: even RevenuePaid {0}-{1}",
            AD_EVENT_DESTROY = "Ad Rewarded: even Destroy",
            AD_EVENT_RECEIVED_REWARD = "Ad Rewarded: even ReceivedReward {0}-{1}";
        private const string ERROR_ADD_EMPTY = "Ad Rewarded: No objects to select",
            ERROR_AD_IS_LOADED = "Ad Rewarded: ad is loaded",
            AD_SHOW_SUCCESS = "Ad Rewarded show success",
            ERROR_AD_SHOW_FAIL = "Ad Rewarded show fail: {0}";

        [SerializeField]
        private TMP_Dropdown dropdownAd;

        private PanelLog panelLog;
        private AdMobRewarded selectAd;

        private AdMobManager manager => AdMobManager.Instance;
        public bool IsShow => gameObject.activeSelf;
        public int Count => dropdownAd.options.Count;
        public AdMobRewarded SelectAd => selectAd;
        #endregion

        #region Unity Events

        #endregion

        #region Methods
        public void Init(PanelLog panelLog)
        {
            this.panelLog = panelLog;
            //
            if (manager == null || manager.Rewarded_Count() == 0)
            {
                dropdownAd.options.Clear();
                return;
            }
            //
            dropdownAd.options.Clear();
            int count = manager.Rewarded_Count();
            for (int i = 0; i < count; i++)
            {
                AdMobRewarded ad = manager.Rewarded_Get(i);
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
            selectAd = manager.Rewarded_Get(value);
            SelectAd_EventRegister();
        }
        public void OnClick_Load()
        {
            if (!IsShow)
                return;
            //
            if (SelectAd.IsLoaded)
                panelLog.AddLog(ERROR_AD_IS_LOADED);
            else
                SelectAd.Load();
        }
        public void OnClick_Show()
        {
            if (!IsShow)
                return;
            //
            IAdRewardedTracking adTracking = SelectAd.Show();
            if (adTracking.IsComplete)
                panelLog.AddLog(AD_SHOW_SUCCESS);
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
            selectAd.OnAdLoaded += SelectAd_OnAdLoaded;
            selectAd.OnAdDisplayed += SelectAd_OnAdDisplayed;
            selectAd.OnAdClicked += SelectAd_OnAdClicked;
            selectAd.OnAdHidden += SelectAd_OnAdHidden;
            selectAd.OnAdRevenuePaid += SelectAd_OnAdRevenuePaid;
            selectAd.OnAdDestroy += SelectAd_OnAdDestroy;
            selectAd.OnAdReceivedReward += SelectAd_OnAdReceivedReward;
        }
        private void SelectAd_EventUnRegister()
        {
            if (selectAd == null)
                return;
            //
            selectAd.OnAdLoaded -= SelectAd_OnAdLoaded;
            selectAd.OnAdDisplayed -= SelectAd_OnAdDisplayed;
            selectAd.OnAdClicked -= SelectAd_OnAdClicked;
            selectAd.OnAdHidden -= SelectAd_OnAdHidden;
            selectAd.OnAdRevenuePaid -= SelectAd_OnAdRevenuePaid;
            selectAd.OnAdReceivedReward -= SelectAd_OnAdReceivedReward;
            selectAd.OnAdDestroy -= SelectAd_OnAdDestroy;
        }
        private void SelectAd_OnAdLoaded(AdBase adSource, bool isSuccess)
        {
            panelLog.AddLog(string.Format(AD_EVENT_LOADED, isSuccess));
        }
        private void SelectAd_OnAdDisplayed(AdBase adSource, bool isSuccess, string placement)
        {
            panelLog.AddLog(string.Format(AD_EVENT_DISPLAYED, isSuccess));
        }
        private void SelectAd_OnAdClicked(AdBase adSource, string placement)
        {
            panelLog.AddLog(AD_EVENT_CLICKED);
        }
        private void SelectAd_OnAdHidden(AdBase adSource, string placement)
        {
            panelLog.AddLog(AD_EVENT_HIDDEN);
        }
        private void SelectAd_OnAdRevenuePaid(AdBase adSource, AdRevenuePaid revenuePaid, string placement)
        {
            panelLog.AddLog(string.Format(AD_EVENT_REVENUE_PAID, revenuePaid.Value, revenuePaid.Currency));
        }
        private void SelectAd_OnAdReceivedReward(AdBase adSource, AdRewardReceived rewardReceived, string placement)
        {
            panelLog.AddLog(string.Format(AD_EVENT_RECEIVED_REWARD, rewardReceived.Label, rewardReceived.Value));
        }
        private void SelectAd_OnAdDestroy(AdBase adSource)
        {
            panelLog.AddLog(AD_EVENT_DESTROY);
        }
        #endregion
    }
}
