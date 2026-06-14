# Delete the .quickpulse directory
Remove-Item ".quickpulse" -Recurse -Force -ErrorAction SilentlyContinue
# Delete the .quickcheckr directory
Remove-Item ".quickcheckr" -Recurse -Force -ErrorAction SilentlyContinue
# Recursively delete all .log files 
Get-ChildItem -Path . -Filter *.log -Recurse -Force | Remove-Item -Force -ErrorAction SilentlyContinue
# Recursively delete all temp md files 
Get-ChildItem -Path . -Filter temp*.md -Recurse -Force | Remove-Item -Force -ErrorAction SilentlyContinue