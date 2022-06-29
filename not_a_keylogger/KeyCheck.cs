using System.Runtime.InteropServices;
using static not_a_keylogger.UserKeyInfo;

namespace not_a_keylogger
{
    class KeyCheck
    {
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        public static bool IsKeyPushedDown(UserKeys vKey)
        {
            return 0 != (GetAsyncKeyState((int)vKey) & 0x8000);
        }
    }
}
