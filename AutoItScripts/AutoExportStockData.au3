#AutoIt3Wrapper_Change2CUI=Y

#include <Constants.au3>
#include <WinAPI.au3>
#include <File.au3>

Global $DebugFlag = True

; cmdArgs: tdx_application_full_path
If $CmdLine[0] < 1 Then
   DebugWriteLine("no enough parameter")
   DebugWriteLine("Usage: <exe> tdx_application_full_path [timeout]")
   Exit -100
EndIf

Global $TdxAppFullPath = $CmdLine[1]

Global $TimeoutSecond = 120
If $CmdLine[0] >= 2 Then
   Local $timeoutPara = Int(Number($CmdLine[2]))
   If $timeoutPara <> 0 Then
	  $TimeoutSecond = $timeoutPara
   EndIf
EndIf

Global $MainWindowTitle = "[TITLE:中信证券至信全能版; CLASS:TdxW_MainFrame_Class]"

; check if main window is opened, if not, try to start it.
If Not IsMainWindowOpened() Then
   DebugWriteLine("Main window is not detected, try to start TDX application")
   If StartTdx() == 0 Then
	  DebugWriteLine("Failed to start TDX application")
	  Exit -1
   EndIf
EndIf

; check if the ad window exists and remove it if exists
;Local $adWindowTitle = "[REGEXPTITLE:东方财富(.*); CLASS:#32770]"
;If WinWait($adWindowTitle, "", 10) <> 0 Then
;   WinClose($adWindowTitle)
;EndIf

; get hwnd of main window
Global $MainWindowHwnd = WinGetHandle($MainWindowTitle)

; activate window
WinActivate($MainWindowHwnd)

; find 系统 button
Global $SystemMenuButtonHandle = ControlGetHandle($MainWindowHwnd, "", "[CLASS:AfxWnd42; INSTANCE:12]")

If $SystemMenuButtonHandle = 0 Then
   Exit -2
EndIf

; draw down menu
ControlClick($MainWindowHwnd, "", $SystemMenuButtonHandle)

; download data
Send("{DOWN 11}")
Send("{ENTER}")

; wait dialog
Global $DataDownloadDialogTitle = "[TITLE:盘后数据下载]"
If WinWait($DataDownloadDialogTitle, "", 10) == 0 Then
   DebugWriteLine("Failed to wait for data downloading dialog")
   Exit -3
EndIf

Global $DataDownloadDialogHwnd = WinGetHandle($DataDownloadDialogTitle)

; check "日线"
Global $DailyDataButtonId = "[ID:1424]" ; control id = 0x590

ControlCommand($DataDownloadDialogHwnd, "", $DailyDataButtonId, "Check", "");

; click "开始下载” button
ControlClick($DataDownloadDialogHwnd, "", "[CLASS:Button; INSTANCE:9]")

; wait until "关闭" button is available
While True
   Sleep(1000)

   Local $closeButtonId = "[CLASS:Button; INSTANCE:10]"
   If ControlCommand($DataDownloadDialogHwnd, "", $closeButtonId, "IsEnabled", "") <> 0 Then
	  ControlClick($DataDownloadDialogHwnd, "", $closeButtonId)
	  ExitLoop
   EndIf
WEnd


; draw down menu
ControlClick($MainWindowHwnd, "", $SystemMenuButtonHandle)

; export data
Send("{DOWN 3}")
Send("{ENTER}")

; wait dialog
Global $ExportDialogTitle = "[TITLE:数据导出]"
If WinWait($ExportDialogTitle, "", 10) == 0 Then
   DebugWriteLine("Failed to wait for data exporting dialog")
   Exit -4
EndIf

; click 高级导出 button
ControlClick($ExportDialogTitle, "", "[CLASS:Button; INSTANCE:7]")

; wait dialog
Global $AdvancedExportDialogTitle = "[TITLE:高级导出]"
If WinWait($AdvancedExportDialogTitle, "", 10) == 0 Then
   DebugWriteLine("Failed to wait for advanced data exporting dialog")
   Exit -5
EndIf

Global $AdvancedExportDialogHwnd = WinGetHandle($AdvancedExportDialogTitle)

; set exporting control parameter
ControlCommand($AdvancedExportDialogHwnd, "", "[CLASSNN:Button4]", "Check", "")
ControlCommand($AdvancedExportDialogHwnd, "", "[CLASSNN:ComboBox3]", "SelectString", "前复权")

; select data to export
Global $AddObjectButtonId = "[CLASSNN:Button5]"

Local $step = 1
Do
   WinActivate($AdvancedExportDialogHwnd)
   ControlClick($AdvancedExportDialogHwnd, "", $AddObjectButtonId)

   If SelectDataToExport($step) Then
	  $step = $step + 1
   Else
	  ExitLoop
   EndIf
Until False

; begin export
ControlClick($AdvancedExportDialogHwnd, "", "[CLASSNN:Button7]")

; wait for confirmation and finish
WinWait("TdxW")
WinClose("TdxW")
Sleep(1000)
WinWait("TdxW")
WinClose("TdxW")

; close dialog
ControlClick($AdvancedExportDialogHwnd, "", "[CLASSNN:Button8]")

; Start east money and return handle of main window. return 0 if failed.
Func StartTdx()
   Run($TdxAppFullPath);

   ; wait for login windows
   Local $loginWindowTitle = "[Title:中信证券至信全能版; Class:#32770]"
   Local $loginWindowHandle = WinWait($loginWindowTitle, "", $TimeoutSecond)

   If $loginWindowHandle = 0 Then
	  Return 0
   EndIf

   ; find 独立行情 button
   Local $buttonHandle = ControlGetHandle($loginWindowHandle, "", "[CLASS:AfxWnd42; INSTANCE:12]")
   If $buttonHandle = 0 Then
	  Return 0
   EndIf

   ControlClick($loginWindowHandle, "", $buttonHandle)

   Local $handle = WinWait($MainWindowTitle, "", $TimeoutSecond)

   Return $handle
EndFunc

Func IsMainWindowOpened()
   return WinWait($MainWindowTitle, "", 5) <> 0
EndFunc

Func DebugWriteLine($s)
   If $DebugFlag Then
	  ConsoleWrite(@HOUR & ":" & @MIN & ":" & @SEC & " " & $s & @CRLF)
   EndIf
EndFunc

Func SelectDataToExport($step)
   Local $dialogTitle = "[Title:选择品种]"
   If WinWait($dialogTitle, "", 10) == 0 Then
	  DebugWriteLine("Failed to wait for selecting data dialog")
	  Exit -6
   EndIf

   Local $dialogHandle = WinGetHandle($dialogTitle)
   Local $buttonSelectAll = ControlGetHandle($dialogHandle, "", "[CLASSNN:Button3]")
   Local $buttonOk = ControlGetHandle($dialogHandle, "", "[CLASSNN:Button1]")
   Local $listView = ControlGetHandle($dialogHandle, "", "[CLASSNN:SysListView321]")

   If $step = 1 Then
	  SelectListViewItem($dialogHandle, $listView, "沪深Ａ股")
	  ControlClick($dialogHandle, "", $buttonSelectAll)
	  Sleep(1000)
	  ControlClick($dialogHandle, "", $buttonOk)
   ElseIf $step = 2 Then
	  Send("399005")
	  Sleep(1000)
	  Send("{ENTER}")
	  Sleep(1000)
   ElseIf $step = 3 Then
	  Send("399006")
	  Sleep(1000)
	  Send("{ENTER}")
	  Sleep(1000)
   ElseIf $step = 4 Then
	  Send("399300")
	  Sleep(1000)
	  Send("{ENTER}")
	  Sleep(1000)
   Else
	  WinClose($dialogHandle)
   EndIf

   Sleep(1000)
   Return $step > 0 AND $step < 5
EndFunc


Func SelectListViewItem($winHandle, $listviewHandle, $item)
   Local $id = ControlListView($winHandle, "", $listviewHandle, "FindItem", $item, "")
   If $id < 0 Then
	  DebugWriteLine("failed to find item " & $item & " from list view")
	  Exit -100
   EndIf

   ControlListView($winHandle, "", $listviewHandle, "Select", $id, "")
   Sleep(500)
EndFunc

Func ClientToScreen($hwnd, $xClient, $yClient, ByRef $xScreen, ByRef $yScreen)
   Local $tPoint = DllStructCreate("int X;int Y")
   DllStructSetData($tPoint, "X", $xClient)
   DllStructSetData($tPoint, "Y", $yClient)

   _WinAPI_ClientToScreen($hwnd, $tPoint)

   If @error == 0 Then
	  $xScreen = DllStructGetData($tPoint, "X")
	  $yScreen = DllStructGetData($tPoint, "Y")
	  return True
   EndIf

   return False
EndFunc


