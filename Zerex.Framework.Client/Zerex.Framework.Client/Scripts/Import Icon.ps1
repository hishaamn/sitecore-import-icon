$dialogProps = @{
    Title = "Custom Icon Importer"
    Description = "The dialog allows the import of custom icon and be available on the Select Icon Dialog."
    Width = 500
    Height = 400
    OkButtonName = "Next"
    CancelButtonName = "Cancel"
    Parameters = @(
        @{ Name = "filename"; Value=""; Title="Filename"; Tooltip="The name of the file without extension"; Placeholder="The file name"; Mandatory = $true},
        @{ Name = "header"; Value=""; Title="Header"; Tooltip="The text that will appear on the dropdown of the icon dialog"; Placeholder="Dropdown text on icon dialog"; Mandatory = $true},
        @{ Name = "unzip"; Value=$false; Title="Unzip file after import"; Tooltip="Tick to unzip the file"; Editor="checkbox"; },
		@{ Name = "isFeatured"; Value=$false; Title="Is Featured"; Tooltip="Tick to add to Featured List"; Editor="checkbox"; }
    )
	
	Validator = {
		if($variables.header.Value -eq ""){
			$variables.header.Error = "Please provide a value."
		}

		$validateHeader = $variables.header.Value -replace '\s',''

		$isHeaderExist = Test-Path -Path "master:/sitecore/system/Modules/Custom Icons/$validateHeader"
		
		$filename = Get-ChildItem -Path "master:/sitecore/system/Modules/Custom Icons" -recurse | Where-Object { $_.Fields["Filename"].Value -eq $variables.filename.Value }

		if($variables.filename.Value -eq ""){
			$variables.filename.Error = "Please provide a value."
		} ElseIf ($isHeaderExist){
			$variables.header.Error = "Name is already in use. Please use a different one."
		} ElseIf($filename.Count -gt 0){
			$variables.filename.Error = "Package name is already in use. Maybe you want to perform an update?"
		}
    }
}

$result = Read-Variable @dialogProps

if($result -ne "ok") {
    Exit
}

# Create icon item
$itemName = $header -replace '\s',''
$iconItem = New-Item -Path "master:/sitecore/system/Modules/Custom Icons" -Name "$itemName" -ItemType "{F163168B-3B21-4011-B777-A5C57A6D7401}"

New-UsingBlock (New-Object Sitecore.SecurityModel.SecurityDisabler) {
	$iconItem.Editing.BeginEdit()
	$iconItem.Fields["Header"].Value = $header
	$iconItem.Fields["Filename"].Value = $filename

	if($isFeatured){
		$iconItem.Fields["Is Featured Icon"].Value = "1"
	} else{
		$iconItem.Fields["Is Featured Icon"].Value = "0"
	}
	
	$iconItem.Editing.EndEdit()
}

$uploadResult = "";

if($unzip){
	$uploadResult = Receive-File -Title "Upload Icon ZIP" -Description "Upload icon zip file only" -Path "$AppPath\sitecore\shell\Themes\Standard" -Unpack
} else{
	$uploadResult = Receive-File -Title "Upload Icon ZIP" -Description "Upload icon zip file only" -Path "$AppPath\sitecore\shell\Themes\Standard"
}

Expand-Archive -Path $uploadResult -DestinationPath "$AppPath\temp\IconCache" -Force

if($isFeatured -eq "1"){
	$itemId = $iconItem.ID
	
	Show-ModalDialog -HandleParameters @{
		"iconfilename"=$filename;
		"iconItemId"=$itemId;
	} -Control "IsFeatured" -Width 1130 -Height 800
}

$user = Get-User -Current

$userSession = Get-Session -Identity $user.Name

Remove-Session -InstanceId $userSession.SessionID