$dialogProps = @{
    Title = "Custom Icon Importer"
    Description = "The dialog allows the removal of custom installed icons."
    Width = 625
    Height = 300
    OkButtonName = "Next"
    CancelButtonName = "Cancel"
    Parameters = @(
        @{ Name = "icon"; Title="Custom installed icons"; Source="DataSource=/sitecore/system/Modules/Custom Icons&DatabaseName=master"; editor="droptree"; }
    )
}

$result = Read-Variable @dialogProps

if($result -ne "ok") {
    Exit
}

$confirmation = Show-Confirm -Title "Are you sure you want to remove?"

if($confirmation -ne "yes"){
	Exit
}

# REMOVE SELECTED ICONS

$iconItem = Get-Item -Path "master:" -ID "$($icon.ID)"

Get-Item -Path "master:" -ID "$($icon.ID)" | Remove-Item

$filename = $iconItem.Fields["Filename"].Value

Get-ChildItem -Path "$AppPath\temp\$filename.*" -Recurse | Remove-Item -Force -Confirm:$false
Get-ChildItem -Path "$AppPath\temp\icons_$filename.*" -Recurse | Remove-Item -Force -Confirm:$false

$params = @{
    "Filename"=$filename
}

$scriptPath = "master:/sitecore/system/Modules/PowerShell/Script Library/Zerex/Icons/IconCache Cleanup"
$scriptItem = Get-Item $scriptPath
$script = [scriptblock]::Create($scriptItem.Script)
Start-ScriptSession -ID "IconCacheCleanup" -ScriptBlock $script -ArgumentList $params -AutoDispose

Wait-ScriptSession -Id "IconCacheCleanup"


$user = Get-User -Current

$userSession = Get-Session -Identity $user.Name

Remove-Session -InstanceId $userSession.SessionID