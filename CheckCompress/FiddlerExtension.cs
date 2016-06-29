using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fiddler;
using System.Windows.Forms;

namespace CheckCompress
{
    public class CheckCompressExt : IFiddlerExtension
    {
        TabPage oPage;
        CCUI oView;

        private void _PrepareTab()
        {
            // Don't do throwaway work
            if (FiddlerApplication.isClosing) { return; }

            if (null != oPage)
            {
                if (!FiddlerApplication.UI.tabsViews.Contains(oPage))
                {
                    FiddlerApplication.UI.tabsViews.Controls.Add(oPage);
                }
                FiddlerApplication.UI.tabsViews.SelectedTab = oPage;
                return;
            }

            oPage = new TabPage("Compressibility");
            FiddlerApplication.UI.tabsViews.TabPages.Add(oPage);
            oView = new CCUI();
            oPage.Controls.Add(oView);
            oView.Dock = DockStyle.Fill;

            FiddlerApplication.UI.tabsViews.SelectedTab = oPage;
        }

        public void OnBeforeUnload() { /*no-op*/ }

        public void OnLoad()
        {
            FiddlerApplication.UI.UNSTABLE_OfferTab("&Compressibility", new EventHandler((o, s) => { _PrepareTab(); }));

            // If we're always enabled, enable ourselves now
            if (FiddlerApplication.Prefs.GetBoolPref("extensions.compressibility.AlwaysOn", false))
            {
                _PrepareTab();
            }
        }
    }
}
