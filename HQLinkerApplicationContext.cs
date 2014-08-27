using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;

namespace HQLinker
{
  class HQLinkerApplicationContext : SystemTrayApplicationContext
  {
    [DllImport("user32.dll")] 
    static extern IntPtr GetClipboardOwner();
    
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

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
      StopMonitoring();
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

      this.ClipboardNotification = new ClipboardNotification();
      ClipboardNotification.ClipboardUpdate += ClipboardNotification_ClipboardUpdate;
      IconText = "Monitoring clipboard";
    }
    private void StopMonitoring()
    {
      this.ClipboardNotification = null;

      Monitoring = false;
      SetIconImage(HQLinker.Properties.Resources.LinkBroken16x16);
      IconText = "Not monitoring clipboard";
    }
    void ClipboardNotification_ClipboardUpdate(object sender, EventArgs e)
    {
      if (Clipboard.ContainsText())
      {
        var Text = Clipboard.GetText().Trim();
        if (Text != LastText)
        {
          LastText = Text;

          IntPtr OwnerWindowHandle = GetClipboardOwner();
          var AppName = "";

          if (!Text.StartsWith("hq://"))
            return; // Not a HQ link

          if (!Process.GetProcessesByName("HQClient").Any())
            return; // HQClient isn't running

          if (OwnerWindowHandle != IntPtr.Zero)
          {
            unsafe
            {
              var ProcessId = win32.GetWindowProcessID((int)OwnerWindowHandle.ToPointer());
              AppName = Process.GetProcessById(ProcessId).ProcessName;
            }
          }

          if (AppName != "HQClient")
          {
            System.Diagnostics.Process.Start(Text);

            var HqProcess = Process.GetProcessesByName("HQClient").FirstOrDefault();
            if (HqProcess != null)
              SetForegroundWindow(HqProcess.MainWindowHandle);
          }
        }
      }
    }
    
    private void HQLinkerApplicationContext_ContextMenuOpeningEvent(object sender, System.ComponentModel.CancelEventArgs e)
    {
    }
    private void HQLinkerApplicationContext_IconDoubleClickEvent()
    {
      ToggleMonitoring();
    }

    private bool Monitoring;
    private ClipboardNotification ClipboardNotification;
    private string LastText;
  }

  /// <summary>
  /// Provides notifications when the contents of the clipboard is updated.
  /// </summary>
  public sealed class ClipboardNotification
  {
    /// <summary>
    /// Occurs when the contents of the clipboard is updated.
    /// </summary>
    public static event EventHandler ClipboardUpdate;

    private static NotificationForm _form = new NotificationForm();

    /// <summary>
    /// Raises the <see cref="ClipboardUpdate"/> event.
    /// </summary>
    /// <param name="e">Event arguments for the event.</param>
    private static void OnClipboardUpdate(EventArgs e)
    {
      var handler = ClipboardUpdate;
      if (handler != null)
      {
        handler(null, e);
      }
    }

    /// <summary>
    /// Hidden form to recieve the WM_CLIPBOARDUPDATE message.
    /// </summary>
    private class NotificationForm : Form
    {
      public NotificationForm()
      {
        NativeMethods.SetParent(Handle, NativeMethods.HWND_MESSAGE);
        NativeMethods.AddClipboardFormatListener(Handle);
      }

      protected override void WndProc(ref Message m)
      {
        if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
        {
            OnClipboardUpdate(null);
        }
        base.WndProc(ref m);
      }
    }
  }

  internal static class NativeMethods
  {
    // See http://msdn.microsoft.com/en-us/library/ms649021%28v=vs.85%29.aspx
    public const int WM_CLIPBOARDUPDATE = 0x031D;
    public static IntPtr HWND_MESSAGE = new IntPtr(-3);

    // See http://msdn.microsoft.com/en-us/library/ms632599%28VS.85%29.aspx#message_only
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AddClipboardFormatListener(IntPtr hwnd);

    // See http://msdn.microsoft.com/en-us/library/ms633541%28v=vs.85%29.aspx
    // See http://msdn.microsoft.com/en-us/library/ms649033%28VS.85%29.aspx
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
  }

  class win32
  {
    [DllImport("user32")]
    private static extern UInt32 GetWindowThreadProcessId(
      Int32 hWnd,
      out Int32 lpdwProcessId
    );

    public static Int32 GetWindowProcessID(Int32 hwnd)
    {
      Int32 pid = 1;
      GetWindowThreadProcessId(hwnd, out pid);
      return pid;
    }

    public win32()
    {

    }
  }
}
