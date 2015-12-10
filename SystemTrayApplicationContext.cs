using System;
using System.Drawing;
using System.Windows.Forms;

namespace HQLinker
{
  class SystemTrayApplicationContext : ApplicationContext
  {
    private System.ComponentModel.Container components;

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
      notifyIcon.MouseUp += notifyIcon_MouseUp;
    }

    public event Action IconClickEvent;
    public event Action IconDoubleClickEvent;
    public event System.ComponentModel.CancelEventHandler ContextMenuOpeningEvent;
    public NotifyIcon notifyIcon { get; private set; }

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

    void notifyIcon_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left && IconClickEvent != null)
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
  }
}
