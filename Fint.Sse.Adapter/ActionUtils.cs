using System;
using Fint.Pwfa.Model;
using Fint.Event.Model;

namespace Fint.Sse.Adapter
{
    public class ActionUtils
    {
        public static bool IsValidPwfaAction(string eventAction)
        {
            if(Enum.TryParse(eventAction, true, out PwfaActions action))
            {
                if (Enum.IsDefined(typeof(PwfaActions), action))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsValidStatusAction(string eventAction)
        {
            if(Enum.TryParse(eventAction, true, out DefaultActions action))
            {
                if (Enum.IsDefined(typeof(DefaultActions), action))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
