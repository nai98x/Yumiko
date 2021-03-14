using System.Diagnostics;

namespace YumikoBot
{
    public interface IDebuggingService
    {
        bool RunningInDebugMode();
    }

    public class DebuggingService : IDebuggingService
    {
        private bool debugging;

        public bool RunningInDebugMode()
        {
            Chequear();
            return debugging;
        }

        [Conditional("DEBUG")]
        private void Chequear()
        {
            debugging = true;
        }
    }
}
