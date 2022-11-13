using LangFromTextWinApp.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LangFromTextWinApp.Helpers
{
    public class LTTimer
    {
        public DispatcherTimer timer = new DispatcherTimer();

        public LTTimer()
        {
            timer.Tick += Timer_Tick;
        }

        public void SetLTActivationTimer(int activateLTModuleIntervalMinutes)
        {
            timer.Stop();

            if (activateLTModuleIntervalMinutes > 0)
            {
                timer.Interval = TimeSpan.FromMinutes(activateLTModuleIntervalMinutes);
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!FEContext.MainWin.IsVisible && !NotifWindow.IsOpened && FEContext.LangFromText.HasEnoughtWords)
            {
                new NotifWindow().Show();
                // In new Windows is not supported
                // notifIcon.ShowBalloonTip(5000, "NA SLOVICKO", "Pod trenovat", ToolTipIcon.Info);
            }
        }

        public void Stop()
        {
            timer.Stop();
        }

    }
}
