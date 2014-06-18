using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HQLinker
{
  class SystemTrayApplicationContext : ApplicationContext
  {
    public SystemTrayApplicationContext()
    {
      components = new System.ComponentModel.Container();
      notifyIcon = new NotifyIcon(components)
      {
        ContextMenuStrip = new ContextMenuStrip(),
        Text = Application.ProductName,
        Visible = true
      };
      notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
      notifyIcon.DoubleClick += notifyIcon_DoubleClick;
      notifyIcon.Click += notifyIcon_Click;
    }

    public event Action IconClickEvent;
    public event Action IconDoubleClickEvent;
    public event System.ComponentModel.CancelEventHandler ContextMenuOpeningEvent;

    public string IconText
    {
      get { return notifyIcon.Text; }
      set { notifyIcon.Text = value; }
    }
    public Icon IconImage
    {
      get { return notifyIcon.Icon; }
    }
    public void SetIconImage(Bitmap image)
    {
      notifyIcon.Icon = Icon.FromHandle(image.GetHicon());
    }

    protected ToolStripItem AddMenuItem(string text, Image image = null, EventHandler clickHandler = null)
    {
      return notifyIcon.ContextMenuStrip.Items.Add(text, image, clickHandler);
    }
    protected void AddMenuSeparator()
    {
      notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
    }
    protected void ExitApplication()
    {
      ExitThread();
    }
    protected void SetIcon(Icon image)
    {
      notifyIcon.Icon = image;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null) { components.Dispose(); }
    }
    protected override void ExitThreadCore()
    {
      //if (mainForm != null) { mainForm.Close(); }
      notifyIcon.Visible = false; // should remove lingering tray icon!
      base.ExitThreadCore();
    }

    private void notifyIcon_Click(object sender, EventArgs e)
    {
      if (IconClickEvent != null)
        IconClickEvent();
    }
    private void notifyIcon_DoubleClick(object sender, EventArgs e)
    {
      if (IconDoubleClickEvent != null)
        IconDoubleClickEvent();
    }
    private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (ContextMenuOpeningEvent != null)
        ContextMenuOpeningEvent(sender, e);
    }


    private System.ComponentModel.Container components;
    private NotifyIcon notifyIcon;
  }
}
