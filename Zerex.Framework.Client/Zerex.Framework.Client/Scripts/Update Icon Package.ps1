$dialogProps = @{
    Title = "Custom Icon Importer"
    Description = "Upload the .zip package to update an existing one."
    Width = 400
    Height = 200
    OkButtonName = "Next"
    CancelButtonName = "Cancel"
    Parameters = @(
		@{ Name = "filename"; Value=""; Title="Filename"; Tooltip="The name of the file without extension"; Placeholder="The file name"; Mandatory = $true},
        @{ Name = "unzip"; Value=$false; Title="Unzip file"; Tooltip="Tick to unzip the file"; Editor="checkbox"; },
		@{ Name = "isFeatured"; Value=$false; Title="Is Featured"; Tooltip="Tick to add to Featured List"; Editor="checkbox"; }
    )
	
	Validator = {		
		$filename = Get-ChildItem -Path "master:/sitecore/system/Modules/Custom Icons" -recurse | Where-Object { $_.Fields["Filename"].Value -eq $variables.filename.Value }

		if($filename.Count -eq 0){
			$variables.filename.Error = "No package with name $($variables.filename.Value) exists."
		}
    }
}

$result = Read-Variable @dialogProps

if($result -ne "ok") {
    Exit
}

Get-ChildItem -Path "$AppPath\temp\$filename.*" -Recurse | Remove-Item -Force -Confirm:$false
Get-ChildItem -Path "$AppPath\temp\icons_$filename.*" -Recurse | Remove-Item -Force -Confirm:$false

$params = @{
    "Filename"=$filename
}

$scriptPath = "master:/sitecore/system/Modules/PowerShell/Script Library/Zerex/Icons/IconCache Cleanup"
$scriptItem = Get-Item $scriptPath
$script = [scriptblock]::Create($scriptItem.Script)
Start-ScriptSession -ID "IconCacheCleanup" -ScriptBlock $script -ArgumentList $params

Wait-ScriptSession -Id "IconCacheCleanup"
Write-Host "PROCEEDING"
$uploadResult = ""

if($unzip){
	$uploadResult = Receive-File -Title "Upload Icon ZIP" -Description "Upload icon zip file only" -Path "$AppPath\sitecore\shell\Themes\Standard" -Unpack -Overwrite
} else{
	$uploadResult = Receive-File -Title "Upload Icon ZIP" -Description "Upload icon zip file only" -Path "$AppPath\sitecore\shell\Themes\Standard" -Overwrite
}

$scriptPath = "master:/sitecore/system/Modules/PowerShell/Script Library/Zerex/Icons/Extractor"
$scriptItem = Get-Item $scriptPath
$script = [scriptblock]::Create($scriptItem.Script)

$uploadParams = @{
    "UploadResult"=$uploadResult
}
Start-ScriptSession -ID "Extractor" -ScriptBlock $script -ArgumentList $uploadParams
Wait-ScriptSession -Id "Extractor"

$iconItem = Get-ChildItem -Path "master:/sitecore/system/Modules/Custom Icons" -recurse | Where-Object { $_.Fields["Filename"].Value -eq $filename }

if($isFeatured -eq "1"){
	
	Show-ModalDialog -HandleParameters @{
		"iconfilename"=$filename;
		"iconItemId"=$iconItem.ID;
	} -Control "IsFeatured" -Width 1130 -Height 800
}

$user = Get-User -Current

$userSession = Get-Session -Identity $user.Name

Remove-Session -InstanceId $userSession.SessionID