#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using HSMiniJSON;
using UnityEngine;

namespace Helpshift
{

    public class HelpshiftLoginEventProxy : AndroidJavaProxy
    {
        private IHelpshiftUserLoginEventListener helpshiftUserLoginEventListener;

        public HelpshiftLoginEventProxy(IHelpshiftUserLoginEventListener helpshiftUserLoginEventListener) :
            base("com.helpshift.unityproxy.HelpshiftLoginEventDelegate")
        {
            this.helpshiftUserLoginEventListener = helpshiftUserLoginEventListener;
        }

        void onLoginSuccess()
        {
            helpshiftUserLoginEventListener.OnLoginSuccess();
        }

        void onLoginFailure(string reason, string error)
        {
            Dictionary<string, string> errorData = new Dictionary<string, string>();

            if (!String.IsNullOrEmpty(error))
            {
                Dictionary<string, object> errorDictonary = (Dictionary<string, object>)Json.Deserialize(error);
                errorData = errorDictonary.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.ToString() ?? string.Empty
                );
            }
            helpshiftUserLoginEventListener.OnLoginFailure(reason, errorData);
        }
    }
}

#endif