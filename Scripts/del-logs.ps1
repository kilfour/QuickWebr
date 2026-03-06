# Delete the .quickpulse directory
Remove-Item ".quickpulse" -Recurse -Force -ErrorAction SilentlyContinue
# Recursively delete all .log files 
Get-ChildItem -Path . -Filter *.log -Recurse -Force | Remove-Item -Force -ErrorAction SilentlyContinue