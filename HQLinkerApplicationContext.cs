﻿using System;
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

    [DllImport("user32.dll")]
    public static extern int SetActiveWindow(int hwnd);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

    [DllImport("user32.dll")]
    static extern bool ShowWindowAsync(IntPtr hWnd, ShowWindowEnum flags);
    
    private enum ShowWindowEnum
    {
      Hide = 0,
      ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
      Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
      Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
      Restore = 9, ShowDefault = 10, ForceMinimized = 11
    };

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    private struct WINDOWPLACEMENT
    {
      public int length;
      public int flags;
      public int showCmd;
      public System.Drawing.Point ptMinPosition;
      public System.Drawing.Point ptMaxPosition;
      public System.Drawing.Rectangle rcNormalPosition;
    }

    public HQLinkerApplicationContext() : base()
    {
      base.IconDoubleClickEvent += HQLinkerApplicationContext_IconDoubleClickEvent;
      base.ContextMenuOpeningEvent += HQLinkerApplicationContext_ContextMenuOpeningEvent;

      Initialise();
    }

    private void Initialise()
    {
      FocusHQTimer = new Timer
      {
        Interval = 500
      };
      FocusHQTimer.Tick += (Sender, Args) =>
      {
        FocusHQTimer.Stop();

        var HqProcess = Process.GetProcessesByName("HQClient").FirstOrDefault();
        if (HqProcess != null)
        {
          WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
          GetWindowPlacement(HqProcess.MainWindowHandle, ref placement);

          switch (placement.showCmd)
          {
            case (int)ShowWindowEnum.ShowNormal: // Normal
            case (int)ShowWindowEnum.Maximize: // Maximized
              ShowWindow(HqProcess.MainWindowHandle, ShowWindowEnum.Show);
              break;
            case (int)ShowWindowEnum.Hide: // Hide
            case (int)ShowWindowEnum.ShowMinimized: // ShowMinimized
            case (int)ShowWindowEnum.ShowMinNoActivate: // ShowMinNoActive
              ShowWindow(HqProcess.MainWindowHandle, ShowWindowEnum.Restore);
              break;

            default:
              ShowWindow(HqProcess.MainWindowHandle, ShowWindowEnum.Show);
              break;
          }

          //SetActiveWindow((int)HqProcess.MainWindowHandle);
          SetForegroundWindow(HqProcess.MainWindowHandle);
        }
      };

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
          var ClipboardOwnerAppName = string.Empty;

          if (!Text.StartsWith("hq://") || Text.Contains(" "))
            return; // Not a HQ link

          if (!Process.GetProcessesByName("HQClient").Any())
          {
            LastText = string.Empty; // Forget the text so the user can open HQ and try again
            notifyIcon.ShowBalloonTip(5000, "Cannot open HQ link", "The HQ client isn't running", ToolTipIcon.Warning);
            return; // HQClient isn't running
          }

          if (OwnerWindowHandle != IntPtr.Zero)
          {
            unsafe
            {
              var ProcessId = Win32.GetWindowProcessID((int)OwnerWindowHandle.ToPointer());
              ClipboardOwnerAppName = Process.GetProcessById(ProcessId).ProcessName;
            }
          }

          if (ClipboardOwnerAppName != "HQClient") // if the app putting the link on the clipboard isn't HQ itself, proceed
          {
            System.Diagnostics.Process.Start(Text);
            notifyIcon.ShowBalloonTip(5000, "HQ link detected on clipboard", "The link has been opened in HQ", ToolTipIcon.Info);

            FocusHQTimer.Stop();
            FocusHQTimer.Start();
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
    private Timer FocusHQTimer;
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

    private static readonly NotificationForm _form = new NotificationForm();

    /// <summary>
    /// Raises the <see cref="ClipboardUpdate"/> event.
    /// </summary>
    /// <param name="e">Event arguments for the event.</param>
    private static void OnClipboardUpdate(EventArgs e)
    {
      ClipboardUpdate?.Invoke(null, e);
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

  class Win32
  {
    [DllImport("user32")]
    private static extern UInt32 GetWindowThreadProcessId(
      Int32 hWnd,
      out Int32 lpdwProcessId
    );

    public static Int32 GetWindowProcessID(Int32 hwnd)
    {
      GetWindowThreadProcessId(hwnd, out int pid);
      return pid;
    }

    public Win32()
    {
    }
  }
}
