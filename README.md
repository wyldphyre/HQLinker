HQLinker
========

## What it does
Detects HQ links on the clipboard and opens them in HQ.

## Description
This is a simple application that sits in the Windows system tray and monitors the clipboard for hq:// links that are copied. From it then tries to execute the url as if the user had clicked on the link. If HQ is installed correctly then the link should open in HQ.

## Possible Future Features
- Bring HQ to the front when a link is triggered (with an option to disable)
- Add enabling/disabling of monitoring from the popup menu
- Optionally play a sound when a link is detected and executed
- Ask the user for confirmation as to whether or not to keep monitoring the application the link was copied from.
- Provide a UI for enabling or disabling monitoring of the various applications that links have been detected being copied from.
- Maybe start the HQ client if it isn't already running?

