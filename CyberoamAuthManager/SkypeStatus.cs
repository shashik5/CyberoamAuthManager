using System;
using SKYPE4COMLib;

namespace CyberoamAuthManager
{
    static class SkypeStatus
    {
        private static bool ChageStatus(TUserStatus status)
        {
            try
            {
                var skype = new Skype();
                skype.Attach(7, true);
                skype.ChangeUserStatus(status);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static bool GoOnline()
        {
            return ChageStatus(TUserStatus.cusOnline);
        }
        public static bool GoAway()
        {
            return ChageStatus(TUserStatus.cusAway);
        }
    }
}
