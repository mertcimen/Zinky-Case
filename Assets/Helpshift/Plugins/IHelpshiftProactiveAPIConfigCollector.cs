using System;
using System.Collections.Generic;

namespace Helpshift
{
    /// <summary>
    /// The interface that needs to be implemented to fetch local map
    /// </summary>
    public interface IHelpshiftProactiveAPIConfigCollector
    {
        Dictionary<string, object> getLocalApiConfig();
    }
}