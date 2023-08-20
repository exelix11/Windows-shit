::CAT: Security
::PRESET: ADV,SIM

:: Disable SMBv1 (powershell)
powershell.exe Disable-WindowsOptionalFeature -Online -FeatureName smb1protocol -norestart

:: Block commonly abused exe files from making outbound connections
Netsh.exe advfirewall firewall add rule name="Block Notepad.exe netconns" program="%systemroot%\system32\notepad.exe" protocol=tcp dir=out enable=yes action=block profile=any
Netsh.exe advfirewall firewall add rule name="Block regsvr32.exe netconns" program="%systemroot%\system32\regsvr32.exe" protocol=tcp dir=out enable=yes action=block profile=any
Netsh.exe advfirewall firewall add rule name="Block calc.exe netconns" program="%systemroot%\system32\calc.exe" protocol=tcp dir=out enable=yes action=block profile=any
Netsh.exe advfirewall firewall add rule name="Block mshta.exe netconns" program="%systemroot%\system32\mshta.exe" protocol=tcp dir=out enable=yes action=block profile=any
Netsh.exe advfirewall firewall add rule name="Block wscript.exe netconns" program="%systemroot%\system32\wscript.exe" protocol=tcp dir=out enable=yes action=block profile=any
Netsh.exe advfirewall firewall add rule name="Block cscript.exe netconns" program="%systemroot%\system32\cscript.exe" protocol=tcp dir=out enable=yes action=block profile=any
Netsh.exe advfirewall firewall add rule name="Block runscripthelper.exe netconns" program="%systemroot%\system32\runscripthelper.exe" protocol=tcp dir=out enable=yes action=block profile=any
Netsh.exe advfirewall firewall add rule name="Block hh.exe netconns" program="%systemroot%\system32\hh.exe" protocol=tcp dir=out enable=yes action=block profile=any

::PRESET: ADV,SIM
:: Disable powershellV2 (not latest powershell)
powershell.exe Disable-WindowsOptionalFeature -Online -FeatureName MicrosoftWindowsPowerShellV2 -norestart
powershell.exe Disable-WindowsOptionalFeature -Online -FeatureName MicrosoftWindowsPowerShellV2Root -norestart

::CAT: Windows settings

:::: The following snippet uses base64 encoding to prevent cmd to powershell escape issues with quotes
::::   It decodes to the following command (UTF16): (Get-CimInstance -Name root\cimv2\power -Class win32_PowerPlan).InstanceID | % { $_.Split(@('{','}'))[1] } | % { Write-Host Applying changes to $_; POWERCFG /SETDCVALUEINDEX $_ 238c9fa8-0aad-41ed-83f4-97be242c8f20 bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d 0; POWERCFG /SETACVALUEINDEX $_ 238c9fa8-0aad-41ed-83f4-97be242c8f20 bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d 0;}
:::: There's a lot to unpack here: 
::::   Get-CimInstance ... % { $_.Split(..) }: gets the current power profile guids (some may be set by oems)
::::   POWERCFG /SETDCVALUEINDEX: sets the option for when battery powered
::::   POWERCFG /SETACVALUEINDEX: sets the option for when AC powered
::::   238c9fa8-0aad-41ed-83f4-97be242c8f20 is the GUID for the sleep options group
::::   bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d is the GUID for the RTCWake option

:: Globally disable Wakeup timers (eg auto resume from sleep for updates)
powershell.exe -ec KABHAGUAdAAtAEMAaQBtAEkAbgBzAHQAYQBuAGMAZQAgAC0ATgBhAG0AZQAgAHIAbwBvAHQAXABjAGkAbQB2ADIAXABwAG8AdwBlAHIAIAAtAEMAbABhAHMAcwAgAHcAaQBuADMAMgBfAFAAbwB3AGUAcgBQAGwAYQBuACkALgBJAG4AcwB0AGEAbgBjAGUASQBEACAAfAAgACUAIAB7ACAAJABfAC4AUwBwAGwAaQB0ACgAQAAoACcAewAnACwAJwB9ACcAKQApAFsAMQBdACAAfQAgAHwAIAAlACAAewAgAFcAcgBpAHQAZQAtAEgAbwBzAHQAIABBAHAAcABsAHkAaQBuAGcAIABjAGgAYQBuAGcAZQBzACAAdABvACAAJABfADsAIABQAE8AVwBFAFIAQwBGAEcAIAAvAFMARQBUAEQAQwBWAEEATABVAEUASQBOAEQARQBYACAAJABfACAAMgAzADgAYwA5AGYAYQA4AC0AMABhAGEAZAAtADQAMQBlAGQALQA4ADMAZgA0AC0AOQA3AGIAZQAyADQAMgBjADgAZgAyADAAIABiAGQAMwBiADcAMQA4AGEALQAwADYAOAAwAC0ANABkADkAZAAtADgAYQBiADIALQBlADEAZAAyAGIANABhAGMAOAAwADYAZAAgADAAOwAgAFAATwBXAEUAUgBDAEYARwAgAC8AUwBFAFQAQQBDAFYAQQBMAFUARQBJAE4ARABFAFgAIAAkAF8AIAAyADMAOABjADkAZgBhADgALQAwAGEAYQBkAC0ANAAxAGUAZAAtADgAMwBmADQALQA5ADcAYgBlADIANAAyAGMAOABmADIAMAAgAGIAZAAzAGIANwAxADgAYQAtADAANgA4ADAALQA0AGQAOQBkAC0AOABhAGIAMgAtAGUAMQBkADIAYgA0AGEAYwA4ADAANgBkACAAMAA7AH0A