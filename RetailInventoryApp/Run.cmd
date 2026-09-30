@echo off
cd /d "%~dp0"
dotnet run --project "RetailInventorySales\RetailInventorySales.csproj"
if errorlevel 1 (
  echo.
  echo The application could not start. Check the error above and README.md.
  pause
)
