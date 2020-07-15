#AutoIt3Wrapper_Change2CUI=Y

#include <Constants.au3>
#include <WinAPI.au3>
#include <File.au3>

Global $DebugFlag = True

; cmdArgs: east_money_application_full_path DDE_data_file_path
If $CmdLine[0] < 2 Then
   DebugWriteLine("no enough parameter")
   DebugWriteLine("Usage: <exe> east_money_application_full_path DDE_data_file_path")
   Exit -100
EndIf

Global $EastMoneyAppFullPath = $CmdLine[1]
Global $DDEDataFileFullPath = $CmdLine[2]


Global $DefaultEastMoneyRoot = "d:\eastmoney"
Global $MainWindowTitle = "[TITLE:东方财富终端; REGEXPCLASS:Afx(.*)]"

; check if main window is opened, if not, try to start it.
If Not IsMainWindowOpened() Then
   DebugWriteLine("Main window is not detected, try to start east money application")
   If StartEastMoney() == 0 Then
	  DebugWriteLine("Failed to start east money application")
	  Exit -1
   EndIf
EndIf

; check if the ad window exists and remove it if exists
Local $adWindowTitle = "[REGEXPTITLE:东方财富(.*); CLASS:#32770]"
If WinWait($adWindowTitle, "", 10) <> 0 Then
   WinClose($adWindowTitle)
EndIf

; get hwnd of main window
Global $MainWindowHwnd = WinGetHandle($MainWindowTitle)

; activate window
WinActivate($MainWindowHwnd)

; Get control of 沪深排行 and click it
Global $HSPH_X = 177
Global $HSPH_Y = 10

ControlClick($MainWindowHwnd, "", 4096, "left", 1, $HSPH_X, $HSPH_Y)

Sleep(3000)

; click DDE决策
Global $DDEJC_X = 400
Global $DDEJC_Y = 15

Local $frameHwnd = ControlGetHandle($MainWindowHwnd, "", 59648)
DebugWriteLine("Frame HWND = " & $frameHwnd)

Local $xScreen = 0, $yScreen = 0

If Not ClientToScreen($frameHwnd, $DDEJC_X, $DDEJC_Y, $xScreen, $yScreen) Then
   DebugWriteLine("ClientToScreen faile, error = " & @error)
   Exit -2
EndIf

MouseMove($xScreen, $yScreen)
MouseClick("left", $xScreen, $yScreen);

Sleep(3000)

; right click on result area
Global $AREA_X = 420
Global $AREA_Y = 150

ClientToScreen($frameHwnd, $AREA_X, $AREA_Y, $xScreen, $yScreen)

MouseClick("right", $xScreen, $yScreen);

Sleep(1000)

; select "export all data"
MouseMove($xScreen + 50, $yScreen + 180)
Sleep(100)

MouseMove($xScreen + 230, $yScreen + 180)
Sleep(100)

MouseClick("left", $xScreen + 230, $yScreen + 180);

; wait for the dialog
Global $ExportDialogTitle = "[TITLE:导出对话框]"
If WinWait($ExportDialogTitle, "", 10) == 0 Then
   DebugWriteLine("Failed to wait for exporting data dialog")
   Exit -3
EndIf

Global $ExportDialogHwnd = WinGetHandle($ExportDialogTitle)

; set the path of data file
Local $index = 1
Local $originalFileName = ""
While FileExists($DDEDataFileFullPath)
   Local $sDrive = "", $sDir = "", $sFilename = "", $sExtension = ""
   Local $aPathSplit = _PathSplit($DDEDataFileFullPath, $sDrive, $sDir, $sFilename, $sExtension)

   If $originalFileName == "" Then
	  $originalFileName = $sFileName
   EndIf

   $sFileName = $originalFileName & "_" & $index
   $index = $index + 1

   $DDEDataFileFullPath = _PathMake($sDrive, $sDir, $sFileName, $sExtension)
WEnd

If ControlSetText($ExportDialogHwnd, "", 2160, $DDEDataFileFullPath) == 0 Then
   DebugWriteLine("Failed to set file path")
   Exit -4
EndIf

Sleep(3000)

; click next step button
ControlClick($ExportDialogHwnd, "", 2159)

Sleep(1000)

; click next step button
ControlClick($ExportDialogHwnd, "", 2159)

Sleep(3000)

; wait for completion button is enabled
While NOT ControlCommand($ExportDialogHwnd, "", 2159, "IsEnabled")
   Sleep(1000)
WEnd

; click completion button
ControlClick($ExportDialogHwnd, "", 2159)

DebugWriteLine("Finished successfully")

; Start east money and return handle of main window. return 0 if failed.
Func StartEastMoney()
   Run($EastMoneyAppFullPath);

   Local $handle = WinWait($MainWindowTitle, "", 30)

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


