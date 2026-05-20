
/*
 * Copyright 2020, Helpshift, Inc.
 * All rights reserved
 */

#if UNITY_IPHONE

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HSMiniJSON;

namespace Helpshift
{
    /// <summary>
    /// This class contains all the iOS APIs corresponding to the public APIs
    /// </summary>
    public class HelpshiftXiOS
    {
        // Runtime linked C methods. These methods should be named exactlly as they are declared in the native code.

        [DllImport("__Internal")]
        private static extern void HsInstallForPlatformIdWithConfig(string platformId, string domainName, string jsonOptionsDict);

        [DllImport("__Internal")]
        private static extern void HsShowConversationWithConfig(string jsonConfigDictionary);

        [DllImport("__Internal")]
        private static extern void HsSetLanguage(string languageCode);

        [DllImport("__Internal")]
        private static extern bool HsLogin(string jsonUserDetailsDict);

        [DllImport("__Internal")]
        private static extern void HsLogout();


        [DllImport("__Internal")]
        private static extern void HsRegisterDeviceToken(string deviceToken);

        [DllImport("__Internal")]
        private static extern void HsPauseDisplayOfInAppNotification(bool pauseInApp);

        [DllImport("__Internal")]
        private static extern void HsHandleNotificationWithUserInfoDictionary(string jsonNotificationDataDict, bool isAppLaunch);

        [DllImport("__Internal")]
        private static extern void HsShowFaqsWithConfig(string configDictionaryString);

        [DllImport("__Internal")]
        private static extern void HsShowFaqSectionWithConfig(string faqSectionPublishID, string configDictionaryString);

        [DllImport("__Internal")]
        private static extern void HsShowSingleFaqWithConfig(string faqPublishID, string configDictionaryString);

        [DllImport("__Internal")]
        private static extern void HsRequestUnreadMessageCount(bool shouldFetchFromServer);

        [DllImport("__Internal")]
        private static extern void HsLeaveBreadcrumb(string breadcrumb);

        [DllImport("__Internal")]
        private static extern void HsClearBreadcrumbs();

        [DllImport("__Internal")]
        private static extern void HsHandleProactiveLink(string proactiveLink);


        [DllImport("__Internal")]
        private static extern void HsClearAnonymousUserOnLogin(bool clearAnonymousUser);

        [DllImport("__Internal")]
        private static extern void HsAddUserTrail(string userTrail);

        [DllImport("__Internal")]
        private static extern void HsCloseSession();

        [DllImport("__Internal")]
        private static extern void HsAddUserIdentities(string identitiesJWT);

        [DllImport("__Internal")]
        private static extern void HsUpdateMasterAttributes(string attributesJson);

        [DllImport("__Internal")]
        private static extern void HsUpdateAppAttributes(string attributesJson);

        public HelpshiftXiOS()
        {

        }

        // Public APIs

        public void Install(string appId, string domainName, Dictionary<string, object> installConfig)
        {
            HelpshiftInternalLogger.d("Install called : Domain : " + domainName + ", app ID : " + appId + ", Config : " + SerializeDictionary(installConfig));
            HsInstallForPlatformIdWithConfig(appId, domainName, SerializeDictionary(installConfig));
        }

        public void ShowConversation(Dictionary<string, object> config)
        {
            HelpshiftInternalLogger.d("show conversation api called with config" + SerializeDictionary(config));
            HsShowConversationWithConfig(SerializeDictionary(config));
        }

        public void ShowFAQs(Dictionary<string, object> configMap)
        {
            HelpshiftInternalLogger.d("show FAQs api called with config" + SerializeDictionary(configMap));
            HsShowFaqsWithConfig(SerializeDictionary(configMap));
        }

        public void ShowSingleFAQ(string faqId, Dictionary<string, object> configMap)
        {
            HelpshiftInternalLogger.d("show single FAQ api called with faqId" + faqId + " config" + SerializeDictionary(configMap));
            HsShowSingleFaqWithConfig(faqId, SerializeDictionary(configMap));
        }

        public void ShowFAQSection(string sectionId, Dictionary<string, object> configMap)
        {
            HelpshiftInternalLogger.d("show FAQ section api called with sectionId" + sectionId+ " config" + SerializeDictionary(configMap));
            HsShowFaqSectionWithConfig(sectionId, SerializeDictionary(configMap));
        }

        public void RequestUnreadMessageCount(Boolean shouldFetchFromServer)
        {
            HelpshiftInternalLogger.d("request unread message count api called with remote fetch : " + shouldFetchFromServer); 
            HsRequestUnreadMessageCount(shouldFetchFromServer);
        }

        public void SetSDKLanguage(string languageCode)
        {
            HelpshiftInternalLogger.d("setLanguage api called for language " + languageCode);
            HsSetLanguage(languageCode);
        }

        public Boolean Login(Dictionary<string, string> userDetails)
        {
            if(userDetails == null)
            {
                HelpshiftInternalLogger.e("userDetails are null in Login API!");
                userDetails = new Dictionary<string, string>();
            }
            HelpshiftInternalLogger.d("Login called : " + userDetails);
            return HsLogin(Json.Serialize(userDetails));;
        }

        public void Logout()
        {
            HelpshiftInternalLogger.d("logout api called");
            HsLogout();
        }

        [Obsolete("ClearAnonymousUserOnLogin is deprecated, please use ClearAnonymousUserOnLogin(Boolean clearAnonymousUser) instead.")]
        public void ClearAnonymousUserOnLogin()
        {
            HelpshiftInternalLogger.d("ClearAnonymouseUserOnLogin api called");
            HsClearAnonymousUserOnLogin(true);
        }

        public void RegisterPushToken(string deviceToken)
        {
            HelpshiftInternalLogger.d("Register device token :" + deviceToken);
            HsRegisterDeviceToken(deviceToken);
        }

        public void PauseDisplayOfInAppNotification(bool pauseInAppNotifications)
        {
            HelpshiftInternalLogger.d("Pause in-app notification called with shouldPause :" + pauseInAppNotifications);
            HsPauseDisplayOfInAppNotification(pauseInAppNotifications);
        }

        public void HandlePushNotification(Dictionary<string, object> notificationDataDict)
        {
            HelpshiftInternalLogger.d("Handle push notification data :" + SerializeDictionary(notificationDataDict));
            HsHandleNotificationWithUserInfoDictionary(SerializeDictionary(notificationDataDict), false);
        }

        public void SetHelpshiftEventsListener(IHelpshiftEventsListener listener)
        {
            HelpshiftInternalLogger.d("Event listener is set");
            HelpshiftXiOSDelegate.SetExternalDelegate(listener);
        }

        public void SetHelpshiftProactiveConfigCollector(IHelpshiftProactiveAPIConfigCollector listener)
        {
            HelpshiftInternalLogger.d("Proactive Event listener is set");
            HelpshiftXiOSDelegate.SetExternalProactiveDelegate(listener);
        }

        public void LeaveBreadcrumb(string breadcrumb)
        {
            HsLeaveBreadcrumb(breadcrumb);
        }

        public void ClearBreadcrumbs()
        {
            HsClearBreadcrumbs();
        }

        public void HandleProactiveLink(string proactiveLink)
        {
            HelpshiftInternalLogger.d("handleProactiveLink Called from unity: " + proactiveLink);
            HsHandleProactiveLink(proactiveLink);
        }

        public void ClearAnonymousUserOnLogin(Boolean shouldClear)
        {
            HelpshiftInternalLogger.d("ClearAnonymouseUserOnLogin api called with param  " + shouldClear);
            HsClearAnonymousUserOnLogin(shouldClear);
        }

        public void AddUserTrail(string trail)
        {
            HelpshiftInternalLogger.d("addUserTrail api called with param  " + trail);
            HsAddUserTrail(trail);
        }

        public void CloseSession()
        {
            HsCloseSession();
        }

        public void LoginWithIdentities(string identitiesJwt, Dictionary<string, object> loginConfig, IHelpshiftUserLoginEventListener helpshiftUserLoginEventListener)
        {
            HelpshiftInternalLogger.d("loginWithIdentities api called with " + identitiesJwt + ", loginConfig " + loginConfig);
            HelpshiftXiOSDelegate.LoginWithIdentities(identitiesJwt,SerializeDictionary(loginConfig),helpshiftUserLoginEventListener);
        }

        public void UpdateAppAttributes(Dictionary<string, object> appAttributes)
        {
            HelpshiftInternalLogger.d("updateAppAttributes api called " + appAttributes);
            HsUpdateAppAttributes(SerializeDictionary(appAttributes));
        }

        public void UpdateMasterAttributes(Dictionary<string, object> masterAttributes)
        {
            HelpshiftInternalLogger.d("updateMasterAttributes api called " + masterAttributes);
            HsUpdateMasterAttributes(SerializeDictionary(masterAttributes));
        }

        public void AddUserIdentities(string identitiesJwt)
        {
            HelpshiftInternalLogger.d("addUserIdentities api called " + identitiesJwt);
            HsAddUserIdentities(identitiesJwt);
        }

        // Private Helpers

        private string SerializeDictionary(Dictionary<string, object> configMap)
        {
            if (configMap == null)
            {
                configMap = new Dictionary<string, object>();
            }
            return Json.Serialize(configMap);
        }
    }

    /// <summary>
    /// Class for adding debug logs for iOS
    /// </summary>
    public class HelpshiftiOSLog
    {

        [DllImport("__Internal")]
        private static extern void HsAddDebugLog(string log);

        private HelpshiftiOSLog()
        {
        }

        public static int v(String tag, String log)
        {
            HsAddDebugLog("HelpshiftLog:Verbose::" + tag + "::" + log);
            return 0;
        }

        public static int d(String tag, String log)
        {
            HsAddDebugLog("HelpshiftLog:Debug::" + tag + "::" + log);
            return 0;
        }

        public static int i(String tag, String log)
        {
            HsAddDebugLog("HelpshiftLog:Info::" + tag + "::" + log);
            return 0;
        }

        public static int w(String tag, String log)
        {
            HsAddDebugLog("HelpshiftLog:Warn::" + tag + "::" + log);
            return 0;
        }

        public static int e(String tag, String log)
        {
            HsAddDebugLog("HelpshiftLog:Error::" + tag + "::" + log);
            return 0;
        }

        public static int log(String log)
        {
            HsAddDebugLog(log);
            return 0;
        }

    }
}

#endif