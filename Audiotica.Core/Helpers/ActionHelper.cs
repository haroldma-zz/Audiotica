using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Audiotica.Core.Helpers
{
    public static class ActionHelper
    {
        public static bool Try(Action action, int attempts = 1)
        {
            while (attempts > 0)
            {
                try
                {
                    action();
                    return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"ActionHelper.Try ({attempts}): {e}");
                    attempts--;
                }
            }
            return false;
        }
    }
}