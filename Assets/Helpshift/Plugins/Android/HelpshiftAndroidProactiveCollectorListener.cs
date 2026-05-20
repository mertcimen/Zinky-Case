#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using UnityEngine;
using HSMiniJSON;

namespace Helpshift
{
    /// <summary>
    /// Internal event listener to delegate the events
    /// </summary>
    public class HelpshiftAndroidProactiveCollectorListener : AndroidJavaProxy
    {
        private IHelpshiftProactiveAPIConfigCollector externalHelpshiftEventsListener;

        public HelpshiftAndroidProactiveCollectorListener(IHelpshiftProactiveAPIConfigCollector externalEventsListener) :
            base("com.helpshift.unityproxy.HelpshiftProactiveConfigEventDelegate")
        {
            this.externalHelpshiftEventsListener = externalEventsListener;
        }

        string getApiConfigString()
        {
            Dictionary<string, object> configMap = externalHelpshiftEventsListener.getLocalApiConfig();
            if (configMap != null)
            {
                return Json.Serialize(configMap);
            }
            return "";
        }
    }
}
#endif
