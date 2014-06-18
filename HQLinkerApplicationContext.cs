using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HQLinker
{
  class HQLinkerApplicationContext : SystemTrayApplicationContext
  {
    public HQLinkerApplicationContext() : base()
    {
      base.IconDoubleClickEvent += HQLinkerApplicationContext_IconDoubleClickEvent;
      base.ContextMenuOpeningEvent += HQLinkerApplicationContext_ContextMenuOpeningEvent;

      Initialise();
    }

    private void Initialise()
    {
      AddMenuItem("Exit", null, ExitMenuItemClick);

      StartMonitoring();
    }

    private void ExitMenuItemClick(object sender, EventArgs e)
    {
      ExitApplication();
    }

    private void ToggleMonitoring()
    {
      if (Monitoring)
        StopMonitoring();
      else
        StartMonitoring();
    }
    private void StartMonitoring()
    {
      Monitoring = true;
      SetIconImage(HQLinker.Properties.Resources.Link16x16);
    }
    private void StopMonitoring()
    {
      Monitoring = false;
      SetIconImage(HQLinker.Properties.Resources.LinkBroken16x16);
    }

    private void HQLinkerApplicationContext_ContextMenuOpeningEvent(object sender, System.ComponentModel.CancelEventArgs e)
    {
    }
    private void HQLinkerApplicationContext_IconDoubleClickEvent()
    {
      ToggleMonitoring();
    }

    private bool Monitoring;
  }
}
