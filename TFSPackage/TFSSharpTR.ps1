[CmdletBinding()]
param
(
   [Parameter(Mandatory=$true)][string] $sharpTRLibraryFolder,
   [Parameter(Mandatory=$true)][string] $settingFile,
   [Parameter(Mandatory=$true)][string] $action 
)
 
try {
	$ErrorActionPreference = "Stop"
    $destDir = "$env:BUILD_REPOSITORY_LOCALPATH\TFSSharpTR"
    if(!(Test-Path -Path $destDir)){
        New-Item -ItemType directory -Path $destDir 
        Get-ChildItem $sharpTRLibraryFolder | ForEach-Object {Copy-Item -Path $_.FullName -Destination "$destDir" -Force} 
    }
    $tfsVariables = New-Object 'system.collections.generic.dictionary[string,string]' 
    $tfsVariables["TF_BUILD"] = $env:TF_BUILD  
    $tfsVariables["AGENT_WorkFolder"] = $env:AGENT_WORKFOLDER
    $tfsVariables["BUILD_DEFINITIONNAME"] = $env:BUILD_DEFINITIONNAME
    $tfsVariables["SYSTEM_COLLECTIONID"] = $env:SYSTEM_COLLECTIONID
    $tfsVariables["SYSTEM_TEAMFOUNDATIONCOLLECTIONURI"] = $env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI
    $tfsVariables["SYSTEM_TEAMPROJECT"] = $env:SYSTEM_TEAMPROJECT
    $tfsVariables["BUILD_REQUESTEDFOR"] = $env:BUILD_REQUESTEDFOR
    $tfsVariables["BUILD_REPOSITORY_PROVIDER"] = $env:BUILD_REPOSITORY_PROVIDER
    $tfsVariables["BUILD_REPOSITORY_NAME"] = $env:BUILD_REPOSITORY_NAME
    $tfsVariables["BUILD_REPOSITORY_URI"] = $env:BUILD_REPOSITORY_URI
    $tfsVariables["BUILD_SOURCETFVCSHELVESET"] = $env:BUILD_SOURCETFVCSHELVESET
    $tfsVariables["BUILD_SOURCEBRANCH"] = $env:BUILD_SOURCEBRANCH
    $tfsVariables["BUILD_SOURCEBRANCHNAME"] = $env:BUILD_SOURCEBRANCHNAME
    $tfsVariables["AGENT_BUILDDIRECTORY"] = $env:AGENT_BUILDDIRECTORY
    $tfsVariables["AGENT_HOMEDIRECTORY"] = $env:AGENT_HOMEDIRECTORY
    $tfsVariables["AGENT_ID"] = $env:AGENT_ID
    $tfsVariables["AGENT_NAME"] = $env:AGENT_NAME
	$tfsVariables["BUILD_ARTIFACTSTAGINGDIRECTORY"] = $env:BUILD_ARTIFACTSTAGINGDIRECTORY
    $tfsVariables["BUILD_BUILDID"] = $env:BUILD_BUILDID
    $tfsVariables["BUILD_BUILDNUMBER"] = $env:BUILD_BUILDNUMBER
    $tfsVariables["BUILD_BUILDURI"] = $env:BUILD_BUILDURI
    $tfsVariables["BUILD_BINARIESDIRECTORY"] = $env:BUILD_BINARIESDIRECTORY
    $tfsVariables["BUILD_DEFINITIONVERSION"] = $env:BUILD_DEFINITIONVERSION
    $tfsVariables["BUILD_QUEUEDBY"] = $env:BUILD_QUEUEDBY
    $tfsVariables["BUILD_REPOSITORY_LOCALPATH"] = $env:BUILD_REPOSITORY_LOCALPATH
	$tfsVariables["BUILD_REPOSITORY_TFVC_WORKSPACE"] = $env:BUILD_REPOSITORY_TFVC_WORKSPACE
    $tfsVariables["BUILD_SOURCESDIRECTORY"] = $env:BUILD_SOURCESDIRECTORY
    $tfsVariables["BUILD_SOURCEVERSION"] = $env:BUILD_SOURCEVERSION
    $tfsVariables["BUILD_STAGINGDIRECTORY"] = $env:BUILD_STAGINGDIRECTORY
    $tfsVariables["COMMON_TESTRESULTSDIRECTORY"] = $env:COMMON_TESTRESULTSDIRECTORY
    $tfsVariables["SYSTEM_ACCESSTOKEN"] = $env:SYSTEM_ACCESSTOKEN
	$tfsVariables["SYSTEM_DEFAULTWORKINGDIRECTORY"] = $env:SYSTEM_DEFAULTWORKINGDIRECTORY
    $tfsVariables["SYSTEM_DEFINITIONID"] = $env:SYSTEM_DEFINITIONID
    $tfsVariables["SYSTEM_TEAMPROJECTID"] = $env:SYSTEM_TEAMPROJECTID           
           
    $userVariables = New-Object 'system.collections.generic.dictionary[string,string]'
    $userVariables["Action"] = $action
    $userVariables["SettingFile"] = "$destDir\$settingFile"
 
    Write-Host ("settings file: "  + $userVariables["SettingFile"])
    Write-Host ("action: "  + $userVariables["Action"])

    Set-Location $destDir
    Add-Type -Path "TfsSharpTR.Core.dll"
    Write-Host "liste cagrilmadan once"
    $taskList = [TfsSharpTR.Core.InitialLoader]::Get($tfsVariables, $userVariables)
    Write-Host "liste cagrildiktan sonra"

    Write-Host $taskList.Count
    foreach($task in $taskList){
       $dllName = $task.DLLName.Trim()
       $className = $task.ClassName.Trim()
       $methodName = $task.MethodName.Trim()      
       Write-Host "dllName: "$dllName - " className: " $className - " methodName: " $methodName
 
       Add-Type -Path $dllName
       $taskObj = New-Object -TypeName $className
       Write-Host "taskObj type: " $taskObj.GetType()
       $taskStatus = $taskObj.$methodName($dllName, $className, $methodName, $tfsVariables, $userVariables)
       Write-Host "taskStatus type: " $taskStatus.GetType()
	   Write-Host "task Status: " $taskStatus.IsSuccess
       #Write-Host "taskStatus: " $taskStatus.Code
	   foreach($msg in $taskStatus.Msgs){
			Write-Host($msg)
	   }
	   if(!taskStatus.IsSuccess){
			Throw [Sytem.Exception] "Operation not allowed"
	   }
    }  
 
} 
catch {
    Write-Host ("Hata: " + $_.Exception.Message)
}
finally {
 
}