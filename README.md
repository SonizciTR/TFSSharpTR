# TFSSharpTR
TFS Sharp Task Runner for TFS 2015 Web Build

This project is started for customizing DevOps operation at TFS. Designed for TFS Web Build process. Goal is using C# languages power for custom tasks and give developers isolation from TFS build process. You can easily add as a task at build process. Then you will set your setting in a json file. 

####Ready Tasks:
1. FileControlTask : Inside "TfsSharpTR.PreBuild" solution. Git has no file lock feature. Some cases you need some files not to modified or modiefied by some specific users. With this task, just add necessary setting to json file. It checks automatically.
```javascript
{
  "PreBuildTasks": [ "FileControlTask" ], //Must have settings
  "PostBuildTasks": [ "TestTask" ],//Must have settings

  "FileControlTask": [
    {
      "FileName": "GlobalSupression.cs",
      "AllowedUser": [ ],
      "AllowedGroup": [ "DirectBackendCore" ],
      "RestrictedUser": [ ],
      "RestrictedGroup": [ ]
    }, // It is a array, You can add as much as you need
  ],
}
```

####Not Ready Tasks (No Promises):
2. Code Analysis
3. Code Metrics
4. StyleCop
5. AutoDeploy
6. Code Documentation (thinking to use SandCastle)

## Setting Json File
There is a must have part :
```
{
  "PreBuildTasks": [ "FileControlTask" ],
  "PostBuildTasks": [ "TestTask" ]
}
```

Other settings are all optional. Like "FileControlTask" part below.
```
{
  "PreBuildTasks": [ "FileControlTask" ],
  "PostBuildTasks": [ "TestTask" ],

  "FileControlTask": [
    {
      "FileName": "GlobalSupression.cs",
      "AllowedUser": [ ],
      "AllowedGroup": [ "DirectBackendCore" ],
      "RestrictedUser": [ ],
      "RestrictedGroup": [ ]
    },
  ],
}
```

## How to develop your own task

It is very easy. Look "TestTask.cs" file. 

Add "TfsSharpTR.Core.dll" reference to your class library. Add a class ("MyTask"), this is your task. Inherit your class from BaseTask<T>. "T" should be inherited from BaseBuildSetting. Last step; add your task to "PreBuildTasks" or "PostBuildTasks" of json setting file. That is it ;) .

If you want to add your custom setting, it is very easy. Add a class and inherit from BaseBuildSetting. Don't forget to add json format to setting file.


### Libraries Required Externally
Newtonsoft.Json.dll

Microsoft.TeamFoundation.Client.dll

Microsoft.TeamFoundation.Common.dll

Microsoft.TeamFoundation.VersionControl.Client.dll

Microsoft.TeamFoundation.VersionControl.Common.dll

Microsoft.TeamFoundation.Work.WebApi.dll

Microsoft.VisualStudio.Services.Client.dll

Microsoft.VisualStudio.Services.Common.dll

Microsoft.VisualStudio.Services.WebApi.dll

