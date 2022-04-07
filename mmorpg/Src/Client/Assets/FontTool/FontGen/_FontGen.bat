@Echo Off&Setlocal Enabledelayedexpansion
set outPath=%1
cd %~dp0\BMFont
echo %cd%
pause
set exePath=%cd%\bmfont.com
"%exePath%" -t "%outPath%\text.txt" -c "%outPath%\config.bmfc"  -o "%outPath%\font.fnt"
exit