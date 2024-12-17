::CAT: Software
::PRESET: ADV,SIM

:: Instal Windows Store on LTSC
wsreset -i
pause 20

::Preset:

:: Install Xbox Game Bar on LTSC (might require manual confirmation)
:::: https://www.microsoft.com/store/productId/9NZKPSTSNW4P?ocid=libraryshare
winget install -e -i --id=9NZKPSTSNW4P --source=msstore