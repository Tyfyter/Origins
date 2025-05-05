@echo off
fxcompiler.exe>log.log
powershell -Command "(gc log.log) -replace 'C:\\Users\\%USERNAME%\\Documents\\My Games\\Terraria\\tModLoader\\ModSources\\Origins\\Effects\\', '' | Out-File -encoding ASCII log.log"
log.log