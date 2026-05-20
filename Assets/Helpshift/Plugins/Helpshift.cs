using HSMiniJSON;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Helpshift
{
    public class HelpshiftSdk
    {

        private const string PLUGIN_VERSION= "10.4.0";
        /// <summary>
        /// Various enums which are used in the Helpshift APIs
        /// </summary>

        public const string ENABLE_LOGGING = "enableLogging";
        public const string ENABLE_INAPP_NOTIFICATION = "enableInAppNotification";

#if UNITY_IOS
        public const string INAPP_NOTIFICATION_APPEARANCE = "inAppNotificationAppearance";
        public const string INAPP_NOTIFICATION_BANNER_BACKGROUND_COLOR = "bannerBackgroundColor";
        public const string INAPP_NOTIFICATION_BANNER_TEXT_COLOR = "textColor";
#endif

#if UNITY_ANDROID
        public const string SCREEN_ORIENTATION = "screenOrientation";
        public const string NOTIFICATION_SOUND_ID = "notificationSoundId";
        public const string NOTIFICATION_CHANNEL_ID = "notificationChannelId";
        public const string NOTIFICATION_ICON = "notificationIcon";
        public const string NOTIFICATION_LARGE_ICON = "notificationLargeIcon";
#endif

#if UNITY_IOS || UNITY_ANDROID
        private static HelpshiftSdk instance = null;
#endif

#if UNITY_IOS
        private static HelpshiftXiOS nativeSdk = null;
#elif UNITY_ANDROID
        private static HelpshiftXAndroid nativeSdk = null;
#endif
        private HelpshiftSdk()
        {
        }


        /// <summary>
        /// Main function which should be used to get the HelpshiftSdk instance.
        /// </summary>
        /// <returns>Singleton HelpshiftSdk instance</returns>
        public static HelpshiftSdk GetInstance()
        {
#if UNITY_IOS || UNITY_ANDROID
            if (instance == null)
            {
                instance = new HelpshiftSdk();
#if UNITY_IOS
                nativeSdk = new HelpshiftXiOS();
#elif UNITY_ANDROID
                nativeSdk = new HelpshiftXAndroid();
#endif
            }
            return instance;
#else
            return null;
#endif
        }

        /// <summary>
        /// Install the HelpshiftSdk with specified apiKey, domainName, appId and config.
        /// When initializing Helpshift you must pass these three tokens. You initialize Helpshift by adding the API call
        /// ideally in the Start method of your game script.
        /// </summary>
        /// <param name="domainName">This is your domain name without any http:// or forward slashes</param>
        /// <param name="appId">This is the unique ID assigned to your app</param>
        /// <param name="config">This is the optional dictionary which contains additional configuration options for the HelpshiftSDK</param>
        public void Install(string appId, string domainName, Dictionary<string, object> config = null)
        {
            if (config == null)
            {
                config = new Dictionary<string, object>();
            }
#if UNITY_IOS || UNITY_ANDROID
            config.Add("sdkType", "unityx");
            config.Add("pluginVersion", PLUGIN_VERSION);
            config.Add("runtimeVersion", Application.unityVersion);
            nativeSdk.Install(appId, domainName, config);
#endif
        }

        /// <summary>
        /// Login a user with a given identifier or email in HelpshiftUser
        /// This API introduces support for multiple login in Helpshift SDK. The user is uniquely identified via identifier and email combination.
        /// If any Support session is active, then any login attempt is ignored.
        /// </summary>
        /// <param helpshiftUser="user">HelpshiftUser model for the user to be logged in</param>
        public Boolean Login(Dictionary<string, string> userData)
        {
#if UNITY_IOS || UNITY_ANDROID
           return nativeSdk.Login(userData);
#else
           return false;
#endif
        }

        /// <summary>
        /// Handle proactive component.
        /// </summary>
        public void HandleProactiveLink(string proactiveLink)
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.HandleProactiveLink(proactiveLink);
#endif
        }

         public void SetHelpshiftProactiveConfigCollector(IHelpshiftProactiveAPIConfigCollector eventsListener)
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.SetHelpshiftProactiveConfigCollector(eventsListener);
#endif
        }

        /// <summary>
        /// Clears the anonymous user.
        /// </summary>
        [Obsolete("ClearAnonymousUserOnLogin is deprecated, please use ClearAnonymousUserOnLogin(Boolean clearAnonymousUser) instead.")]
        public void ClearAnonymousUserOnLogin()
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.ClearAnonymousUserOnLogin(true);
#endif
        }

        // <summary>
        /// Clears the anonymous user with param.
        /// </summary>
        public void ClearAnonymousUserOnLogin(Boolean shouldClear)
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.ClearAnonymousUserOnLogin(shouldClear);
#endif
        }

        // <summary>
        ///
        ///Client can call this public api anytime after install call to track the user trail
        ///@param  trail
        ///it will accept string as param which will be send to backend for analytics
        /// </summary>
        public void AddUserTrail(string trail)
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.AddUserTrail(trail);
#endif
        }

        /// <summary>
        /// Logout the current user
        /// </summary>
        public void Logout()
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.Logout();
#endif
        }

        /// <summary>
        /// Registers the device token with Helpshift.
        /// </summary>
        /// <param name="token">Device token.</param>
        public void RegisterPushToken(string token)
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.RegisterPushToken(token);
#endif
        }

        /// <summary>
        /// Shows the helpshift conversation screen.
        /// </summary>
        /// <param name="configMap">the optional dictionary which will contain the arguments passed to the
        /// Helpshift session (that will start with this method call).</param>
        public void ShowConversation(Dictionary<string, object> configMap = null)
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.ShowConversation(configMap);
#endif
        }

        /// <summary>
        /// Show FAQs screen
        /// </summary>
        /// <param name="configMap">the optional dictionary which will contain the arguments passed to the
        /// Helpshift session (that will start with this method call)</param>
        public void ShowFAQs(Dictionary<string, object> configMap = null)
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.ShowFAQs(configMap);
#endif
        }


        /// <summary>
        /// Shows Single FAQ screen for the passed faqid
        /// </summary>
        /// <param name="faqId">FAQ Id</param>
        /// <param name="configMap">The optional dictionary which will contain the arguments passed to the
        /// Helpshift session (that will start with this method call)</param>
        public void ShowSingleFAQ(string faqId, Dictionary<string, object> configMap = null)
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.ShowSingleFAQ(faqId, configMap);
#endif
        }


        /// <summary>
        /// Shows FAQ section screen for the passed section id
        /// </summary>
        /// <param name="sectionId">Section Id</param>
        /// <param name="configMap">The optional dictionary which will contain the arguments passed to the
        /// Helpshift session (that will start with this method call)</param>
        public void ShowFAQSection(string sectionId, Dictionary<string, object> configMap = null)
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.ShowFAQSection(sectionId, configMap);
#endif
        }


        /// <summary>
        /// Fetches the unread messages count from local or remote based on shouldFetchFromServer flag.
        /// The result of unread count will be passed over the IHelpshiftEventsListener#HandleHelpshiftEvent method
        /// </summary>
        /// <param name="shouldFetchFromServer">Should fetch unread message count from server.</param>
        public void RequestUnreadMessageCount(Boolean shouldFetchFromServer)
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.RequestUnreadMessageCount(shouldFetchFromServer);
#endif
        }


        /// <summary>
        /// Handles the push notification data that is received in the notification packet
        /// </summary>
        /// <param name="pushNotificationData">Push notification data. For iOS this should be the dictionary which is received from APNS.
        /// The structure of the dictionary should be : {"origin" : "helpshift", "aps" : {<notification data>}}
        /// For Android pass in the Dictionary which is received from GCM as is.</param>
        public void HandlePushNotification(Dictionary<string, object> pushNotificationData)
        {
#if UNITY_IOS || UNITY_ANDROID
            // Remove null values from dictonary as null is converted to "null" string in json.
            List<string> keysToRemove = new List<string>();
            foreach (KeyValuePair<string, object> keyValuePair in pushNotificationData)
            {
                if (keyValuePair.Value == null)
                {
                    keysToRemove.Add(keyValuePair.Key);
                }
            }

            foreach (string key in keysToRemove)
            {
                pushNotificationData.Remove(key);
            }

            nativeSdk.HandlePushNotification(pushNotificationData);
#endif
        }


        /// <summary>
        /// Sets the SDK language to the given locale
        /// </summary>
        /// <param name="locale">Language locale.</param>
        public void SetSDKLanguage(string locale)
        {
#if UNITY_ANDROID || UNITY_IOS
            nativeSdk.SetSDKLanguage(locale);
#endif
        }

        public void UpdateAppAttributes(Dictionary<string, object> attritbutes){
        #if UNITY_IOS || UNITY_ANDROID 
                nativeSdk.UpdateAppAttributes(attritbutes);
        #endif      
        }

        public void UpdateMasterAttributes(Dictionary<string, object> attritbutes){
        #if UNITY_IOS || UNITY_ANDROID 
                nativeSdk.UpdateMasterAttributes(attritbutes);
        #endif
        }

        public void AddUserIdentities(string identitiesJwt){
        #if UNITY_IOS || UNITY_ANDROID 
                nativeSdk.AddUserIdentities(identitiesJwt);
        #endif
        }

        public void LoginWithIdentity(string identitiesJwt, Dictionary<string, object> loginConfig, IHelpshiftUserLoginEventListener helpshiftUserLoginEventListener){
        #if UNITY_IOS || UNITY_ANDROID 
                nativeSdk.LoginWithIdentities(identitiesJwt, loginConfig, helpshiftUserLoginEventListener);
        #endif  
        }


        /// <summary>
        /// Adds additional debugging information in your code. 
        /// You can add additional debugging statements to your code, and see exactly what the user was doing right before they started a new conversation.
        /// </summary>
        /// <param name="breadcrumb">Breadcrumb</param>

        public void LeaveBreadcrumb(string breadcrumb)
        {
#if UNITY_ANDROID || UNITY_IOS
            nativeSdk.LeaveBreadcrumb(breadcrumb);
#endif   
        }
        
        /// <summary>
        /// Clears Breadcrumbs list.
        /// </summary>
        public void ClearBreadcrumbs()
        {
#if UNITY_ANDROID || UNITY_IOS
            nativeSdk.ClearBreadcrumbs();
#endif   
        }
        public void SetHelpshiftEventsListener(IHelpshiftEventsListener eventsListener)
        {
#if UNITY_IOS || UNITY_ANDROID
            nativeSdk.SetHelpshiftEventsListener(eventsListener);
#endif
        }

        public string SdkVersion()
        {
#if UNITY_IOS || UNITY_ANDROID
           return PLUGIN_VERSION;
#else
           return "";
#endif
        }

        /// <summary>
        /// Close Helpshift Conversation/FAQs screens
        /// </summary>
        public void CloseSession()
        {
#if UNITY_IOS || UNITY_ANDROID
           nativeSdk.CloseSession();
#endif
        }

        // iOS Specific APIs

#if UNITY_IOS
        /// <summary>
        /// Pauses the in-app notifications
        /// </summary>
        /// <param name="pauseInAppNotifications">bool representing whether to pause / resume the in app notifications</param>
        public void PauseDisplayOfInAppNotification(bool pauseInAppNotifications)
        {
            nativeSdk.PauseDisplayOfInAppNotification(pauseInAppNotifications);
        }
#endif
    }


    /// Utility class to add logs in Helpshift SDK. These logs will be attached to an issue filed by the user.    
    public class HelpshiftLog
    {
        public static int v(String tag, String log)
        {
#if UNITY_IOS
            return HelpshiftiOSLog.v(tag, log);
#elif UNITY_ANDROID
            return HelpshiftAndroidLog.v(tag, log);
#else
            return 0;
#endif
        }

        public static int d(String tag, String log)
        {
#if UNITY_IOS
            return HelpshiftiOSLog.d(tag, log);
#elif UNITY_ANDROID
            return HelpshiftAndroidLog.d(tag, log);
#else
            return 0;
#endif
        }

        public static int i(String tag, String log)
        {
#if UNITY_IOS
            return HelpshiftiOSLog.i(tag, log);
#elif UNITY_ANDROID
            return HelpshiftAndroidLog.i(tag, log);
#else
            return 0;
#endif
        }

        public static int w(String tag, String log)
        {
#if UNITY_IOS
            return HelpshiftiOSLog.w(tag, log);
#elif UNITY_ANDROID
            return HelpshiftAndroidLog.w(tag, log);
#else
            return 0;
#endif
        }

        public static int e(String tag, String log)
        {
#if UNITY_IOS
            return HelpshiftiOSLog.e(tag, log);
#elif UNITY_ANDROID
            return HelpshiftAndroidLog.e(tag, log);
#else
            return 0;
#endif
        }

    }

}
