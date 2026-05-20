/*
 * Copyright 2020, Helpshift, Inc.
 * All rights reserved
 */

#if UNITY_IOS
using System.Runtime.InteropServices;
using System.Collections.Generic;
using HSMiniJSON;
using System;

namespace Helpshift
{
    /// <summary>
    /// Contains the event names that can be received from HelpshiftDelegate
    /// </summary>
    static class HelpshiftEventConstants
    {
        public const string HandleHelpshiftEvent = "handleHelpshiftEvent";
        public const string AuthenticationFailed = "authenticationFailedForUserWithReason";
        public const string LoginWithIdentitySuccess = "loginWithIdentitySuccess";
        public const string LoginWithIdentityFailed = "loginWithIdentityFailed";

    }

    public class HelpshiftXiOSDelegate
    {
        /// <summary>
        /// The signature of the delegate that is invoked when we receive message on unity side from native
        /// </summary>
        /// <param name="methodName">The name of the method that was invoked</param>
        /// <param name="parametersJson">The JSON representation of the parameters of the method</param>
        private delegate void UnitySupportMessageCallback(string methodName, string parametersJson);


        /// <summary>
        /// The signature of the delegate that is invoked when handleProactive is triggered
        /// </summary>
        /// return type dictionary convertable string
        private delegate string unityGetApiConfig();

        /// <summary>
        /// Runtimed linked C method to register callback with the declared signature
        /// </summary>
        /// <param name="callback">The callback object</param>
        [DllImport("__Internal")]
        private static extern void HsRegisterHelpshiftDelegateCallback(UnitySupportMessageCallback callback);


        /// <summary>
        /// Runtimed linked C method to register callback with the declared signature
        /// </summary>
        [DllImport("__Internal")]
        private static extern void HsRegisterHelpshiftProactiveDelegateCallback(unityGetApiConfig callback);

        // Import the Helpshift loginWithIdentity function
        [DllImport("__Internal")]
        private static extern void HsLoginWithIdentity(string identityJWT,string loginConfigJson,UnitySupportMessageCallback callback);

        /// <summary>
        /// The shared instance of this class.
        /// </summary>
        private static HelpshiftXiOSDelegate sharedDelegate;

        /// <summary>
        /// The external delegate object that has been passed by the developer
        /// </summary>
        internal IHelpshiftEventsListener externalDelegate;

        /// <summary>
        /// The external delegate object for pro active config that has been passed by the developer
        /// </summary>
        internal IHelpshiftProactiveAPIConfigCollector externalProactiveDelegate;

        /// <summary>
        /// The external delegate object for loginWithIdentitiesDelegate
        /// </summary>
        internal IHelpshiftUserLoginEventListener externalLoginWithIdentityDelegate;

        private HelpshiftXiOSDelegate() { }


        public static HelpshiftXiOSDelegate GetInstance()
        {
            if (sharedDelegate == null)
            {
                sharedDelegate = new HelpshiftXiOSDelegate();
            }

            return sharedDelegate;
        }

        /// <summary>
        /// Call this method to set the external delegate to listen to Helpshift Events
        /// </summary>
        /// <param name="helpshiftEventListener"></param>
        public static void SetExternalDelegate(IHelpshiftEventsListener helpshiftEventListener)
        {
            GetInstance().externalDelegate = helpshiftEventListener;
            RegisterHelpshiftDelegateCallback();
        }

        /// <summary>
        /// Call this method to set the external delegate to listen to Helpshift Events for proactive
        /// </summary>
        /// <param name="proactiveDelegate"></param>
        public static void SetExternalProactiveDelegate(IHelpshiftProactiveAPIConfigCollector proactiveDelegate)
        {
            GetInstance().externalProactiveDelegate = proactiveDelegate;
            RegisterHelpshifProactiveDelegateCallback();
        }

        /// <summary>
        /// Call this method to login with identity
        /// </summary>
        /// <param name="loginWithIdentities"></param>
        public static void LoginWithIdentities(string identitiesJwt,string loginConfig, IHelpshiftUserLoginEventListener helpshiftUserLoginEventListener)
        {
            GetInstance().externalLoginWithIdentityDelegate = helpshiftUserLoginEventListener;
            HsLoginWithIdentity(identitiesJwt,loginConfig,UnityLoginWithIdentityCallbackImpl);
        }

        // Private Helpers

        private static void RegisterHelpshiftDelegateCallback()
        {
            HsRegisterHelpshiftDelegateCallback(UnityHelpshiftDelegateCallbackImpl);
        }

        // register pro active getconfig method callback

        private static void RegisterHelpshifProactiveDelegateCallback()
        {
            HsRegisterHelpshiftProactiveDelegateCallback(UnityGetApiConfigProactiveDelegateCallbackImpl);
        }


        [MonoPInvokeCallback(typeof(UnitySupportMessageCallback))]
        private static void UnityHelpshiftDelegateCallbackImpl(string methodName, string parametersJson)
        {
            IHelpshiftEventsListener externalDelegate = GetInstance().externalDelegate;

            if (externalDelegate == null)
            {
                return;
            }

            if (methodName == HelpshiftEventConstants.HandleHelpshiftEvent)
            {
                Dictionary<string, object> eventJson = (Dictionary<string, object>)Json.Deserialize(parametersJson);
                string eventName = "";
                if(eventJson.ContainsKey("eventName"))
                {
                    eventName = (string)eventJson["eventName"];
                }

                object eventData = null;
                if(eventJson.ContainsKey("eventData"))
                {
                    eventData = eventJson["eventData"];
                }

                Dictionary<string, object> eventDataJson = null;
                if(eventData != null)
                {
                    eventDataJson = (Dictionary<string, object>)eventData;
                }

                externalDelegate.HandleHelpshiftEvent(eventName, eventDataJson);
            }
            else if (methodName == HelpshiftEventConstants.AuthenticationFailed)
            {
                Dictionary<string, object> failureReasonJson = (Dictionary<string, object>)Json.Deserialize(parametersJson);
                object failureReasonObject = failureReasonJson["reason"];
                int failureReason = System.Convert.ToInt32(failureReasonObject);

                HelpshiftAuthenticationFailureReason authFailureReason = HelpshiftAuthenticationFailureReason.UNKNOWN;

                if(failureReason == 0) // Auth token not provided
                {
                    authFailureReason = HelpshiftAuthenticationFailureReason.AUTH_TOKEN_NOT_PROVIDED;
                }
                else if(failureReason == 1) // Invalid auth token
                {
                    authFailureReason = HelpshiftAuthenticationFailureReason.INVALID_AUTH_TOKEN;
                }


                externalDelegate.AuthenticationFailedForUser(authFailureReason);
            }
        }

        [MonoPInvokeCallback(typeof(unityGetApiConfig))]
        private static string UnityGetApiConfigProactiveDelegateCallbackImpl()
        {
            IHelpshiftProactiveAPIConfigCollector externalDelegate = GetInstance().externalProactiveDelegate;
            Dictionary<string, object> config = externalDelegate.getLocalApiConfig();
            return SerializeDictionary(config);
        }

        [MonoPInvokeCallback(typeof(UnitySupportMessageCallback))]
        static void UnityLoginWithIdentityCallbackImpl(string methodName, string parametersJson)
        {
            IHelpshiftUserLoginEventListener externalDelegate = GetInstance().externalLoginWithIdentityDelegate;

            if (externalDelegate == null)
            {
                HelpshiftInternalLogger.e("externalDelegate is null, skipping callback invocation.");
                return;
            }

            try
            {

                if (methodName == HelpshiftEventConstants.LoginWithIdentitySuccess)
                {
                    externalDelegate.OnLoginSuccess();

                } else if (methodName == HelpshiftEventConstants.LoginWithIdentityFailed) {

                    Dictionary<string, object> parametersDictionary = (Dictionary<string, object>)Json.Deserialize(parametersJson);

                    if (parametersDictionary == null)
                    {
                        HelpshiftInternalLogger.d("Failed to parse parametersDictionary: " + parametersJson);
                    }

                    string reason = "";
                    if (parametersDictionary.ContainsKey("reason"))
                    {
                        reason = parametersDictionary["reason"] as string ?? "unknownError";
                    }

                    // Extract and parse "errors" field
                    Dictionary<string, string> eventDataDictionary = new Dictionary<string, string>();
                    if (parametersDictionary.ContainsKey("errors"))
                    {
                        Dictionary<string, object> errorsObject = parametersDictionary["errors"] as Dictionary<string, object>;
                        if (errorsObject != null)
                        {
                            // Convert to Dictionary<string, string>, ignoring invalid value types
                            foreach (var keyValue in errorsObject)
                            {
                                if (keyValue.Value is string stringValue)
                                {
                                    eventDataDictionary[keyValue.Key] = stringValue;
                                }
                                else
                                {
                                    HelpshiftInternalLogger.d($"Ignoring key '{keyValue.Key}' with non-string value.");
                                }
                            }
                        }
                        else
                        {
                            HelpshiftInternalLogger.d("Errors field is not a dictionary: " + parametersDictionary["errors"]);
                        }
                    }
                    // Check if externalDelegate exists and invoke callback
                    if (externalDelegate != null)
                    {
                        externalDelegate.OnLoginFailure(reason, eventDataDictionary);
                    }
                }
            }
            catch (Exception ex)
            {
                HelpshiftInternalLogger.e($"Exception in UnityLoginWithIdentityCallbackImpl: {ex}");
                externalDelegate.OnLoginFailure("unknownError", new Dictionary<string, string>());
            }
            finally
            {
                HelpshiftInternalLogger.d("Setting externalLoginWithIdentityDelegate to null");
                GetInstance().externalLoginWithIdentityDelegate = null;
            }
        }

        private static string SerializeDictionary(Dictionary<string, object> configMap)
        {
            if (configMap == null)
            {
                configMap = new Dictionary<string, object>();
            }
            return Json.Serialize(configMap);
        }
    }
}
#endif
