using System.Collections.Generic;

namespace Helpshift
{

    // Login listener needed to be implemented when using loginWithIdentities
    public interface IHelpshiftUserLoginEventListener
    {

        void OnLoginSuccess();

        void OnLoginFailure(string reason, Dictionary<string, string> errorMap);
    }
}