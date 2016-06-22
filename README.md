# TFSSharpTR
TFS Sharp Task Runner for TFS 2015 Web Build

TFS Sharp Task Runner similer to Gulp or Grunt. It runs C# code with single json config file.

This project is started because of customizing DevOps operation at TFS. Designed for TFS Web Build process. Goal is using C# languages power for custom tasks and give developers isolation from TFS build process. You can easily add as a task at your build process. Then you will set your setting in a json file. 

I decided th sperate tasks by their dependencies. So you will see different class libraries. If I don't do this, you will need every external libraries included with your packet.

####Ready Tasks:
1. FileControlTask : Inside "TfsSharpTR.PreBuild" solution. Git has no file lock feature. Some cases you need some files not to modified or modiefied by some specific users. With this task, just add necessary setting to json file ("FileControlTask" part). It checks automatically.
```javascript
{
  "PreBuildTasks": [ "FileControlTask" ], //Must have settings
  "PostBuildTasks": [ "TestTask" ], //Must have settings

  "FileControlTask": [ // It is a array, You can add as much as you need
    {
      "FileName": "GlobalSupression.cs",
      "AllowedUser": [ "DomainName\\MyUser" ],
      "AllowedGroup": [ "MyGroupthatAllowed" ],
      "RestrictedUser": [ ],
      "RestrictedGroup": [ ]
    }, 
  ],
}
```

####Not Ready Tasks (No Promises):
2. Code Analysis
3. Code Metrics
4. StyleCop
5. AutoDeploy (For IIS apppool start/stop; there is a big problem if you are deploying to another domain. ServerManager.OpenRemote method uses NTLM authentication only :( . I am looking for a solution )
6. Code Documentation (thinking to use SandCastle)

## Setting Json File
There is a must have part :
```javascript
{
  "PreBuildTasks": [ "FileControlTask" ],
  "PostBuildTasks": [ "TestTask" ]
}
```

Other settings are all optional. Like "FileControlTask" part below.
```javascript
{
  "PreBuildTasks": [ "FileControlTask" ],
  "PostBuildTasks": [ "TestTask" ],

  "FileControlTask": [
    {
      "FileName": "GlobalSupression.cs",
      "AllowedUser": [ "DomainName\\MyUser" ],
      "AllowedGroup": [ "MyGroupthatAllowed" ],
      "RestrictedUser": [ ],
      "RestrictedGroup": [ ]
    },
  ],
}
```

## How to develop your own task

It is very easy. Look "TestTask.cs" file. 

Add "TfsSharpTR.Core.dll" reference to your class library. Add a class ("MyFirstTask" etc...), this is your task. Inherit your class from BaseTask<T>. "T" should be inherited from BaseBuildSetting. Last step; add your task to "PreBuildTasks" or "PostBuildTasks" of json setting file. That is it ;) .

If you want to add your custom setting, it is very easy. Add a class and inherit from BaseBuildSetting. Your class automatically parsed from setting file and added to UserVariable parameter of the method. Don't forget to add your json format to setting file.

## Install
I am assuming you have enough privliges at TFS server.

1. Install Node to your computer
2. Open command prompt 
3. Run "npm install -g tfx cli"
4. Run "tfx build tasks upload"
5. It will ask you a folder path. Give "TFSPackage" folder.
6. Open TFS web interface and you can add your new build step pre-build and post-build.
7. Do not forget the set "PreBuild" and "PostBuild" comboboxes of TFS Sharp TR

### TfsSharpTR.Core Library Dependencies
Newtonsoft.Json.dll

Microsoft.TeamFoundation.Client.dll

Microsoft.TeamFoundation.Common.dll

Microsoft.TeamFoundation.VersionControl.Client.dll

Microsoft.TeamFoundation.VersionControl.Common.dll

Microsoft.TeamFoundation.Work.WebApi.dll

Microsoft.VisualStudio.Services.Client.dll

Microsoft.VisualStudio.Services.Common.dll

Microsoft.VisualStudio.Services.WebApi.dll

### TfsSharpTR.StyleCopRelated Library Dependencies

StyleCop.dll

StyleCop.CSharp.dll

StyleCop.CSharp.Rules.dll