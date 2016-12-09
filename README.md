# TFSSharpTR
TFS Sharp Task Runner is; C# code runner on TFS 2015 Web Build. Basic need came from DevOps. If you are implementing DevOps using TFS, you will need this.

TFS Sharp Task Runner similer to Gulp or Grunt. It runs C# code with single json config file.

This project is started because of customizing DevOps operation at TFS. Designed for TFS Web Build process. Goal is using C# languages power for custom tasks and give developers isolation from TFS build process. You can easily add as a task at your build process. Then you will set your setting in a json file. 

I decided th sperate tasks by their dependencies. So you will see different class libraries. If I don't do this, you will need every external libraries included with your packet.

####Ready Tasks:
* FileControlTask : Inside "TfsSharpTR.PreBuild" solution. Git has no file lock feature. Some cases you need some files not to modified or modiefied by some specific users. With this task, just add necessary setting to json file ("FileControlTask" part). It checks automatically.
```javascript
{
  "PreBuildTasks": [ "FileControlTask" ], //Must have settings
  "PostBuildTasks": [ "TestTask" ], //Must have settings
  
  "FileControlTask": {// It is a array, You can add as much as you need
    "Files": [
      {
        "FileNames": [ "GlobalSupression.cs", "Settings.StyleCop" ],// It is a array, You can add as much as you need
        "AllowedUser": [ "DomainName\\MyUser" ],
        "AllowedGroup": [ "MyGroupthatAllowed" ]
      },
    ],
  },
}
```

* StyleCop : Inside "TfsSharpTR.StyleCopRelated" solution. Runs your stylecop rules. You can excluded specific files.
```javascript
{
  "PreBuildTasks": [ "StyleCopTask" ], //Must have settings
  "PostBuildTasks": [ "TestTask" ], //Must have settings

  "StyleCopTask": {
		"ExcludedFiles": [ "GlobalSupression.cs", "AssemblyInfo.cs" ],
		"MaxErrorCount": 0,
		"MaxLogCount": 30,
		"TreatWarnAsError": false,
		"SettingFile": "Settings.StyleCop"
	  },
}
```

* Code Metrics : Inside "TfsSharpTR.Roslyn" solution. Using ArchiMetrics project on github. Analyze all solutions in the project or SolutiontoBuild variable solution.
 
```javascript
{
  "PreBuildTasks": [ "CodeMetricTask" ], //Must have settings
  "PostBuildTasks": [ "TestTask" ], //Must have settings

  "CodeMetric": {
    "MaxLogCount": 50,
    "MaxCouplingComplexityforMember": 40,
    "MaxCouplingComplexityforClass": 40,
    "MaxClassCouplingforMember": 20,
    "MaxClassCouplingforClass": 40,
    "MaxCyclomaticComplexityforMember": 20,
    "MaxCyclomaticComplexityforClass": 40,
    "MaxDepthofInheritenceforClass": 7,
    "MaxDepthofInheritenceforMember": 7,
    "MaxLinesofCodeforClass": 1000,
    "MaxLinesofCodeforMembers": 40,
    "MinMaintainabilityIndexforMember": 50,
    "MinMaintainabilityIndexforClass": 40,
    "MaxEfferentCoupling": 5,
    "MaxNumberOfParameters": 5,
    "ExcludedSolutions": [ ],
    "ExcludedProjects": [ ]
  }
}
```

* AnalyzerEnforcer : Inside "TfsSharpTR.Roslyn" solution. Sometimes we are controlling solution with custom packages that has over custom rules. If there is new project added to solution, there will be no control over this. AnalyzerEnforcer; at build time addes your nuget packages to soluiton if they are not exist. This is very usefull forcing custom nuget packages.

```javascript
{
  "PreBuildTasks": [ "AnalyzerEnforcerTask" ], //Must have settings
  "PostBuildTasks": [ "TestTask" ], //Must have settings

  "AnalyzerEnforcerTask": {
    "References": [
      {
        "DLLName": "",
        "DLLPath": "",
        "ExcludedSolutions": [ ],
        "ExcludedProjects": [ ]
      }
    ]
  },
}
```

####Not Ready Tasks (No Promises):

* Partial Unit Test
* AutoDeploy (For IIS apppool start/stop; there is a big problem if you are deploying to another domain. ServerManager.OpenRemote method uses NTLM authentication only :( . I am looking for a solution )
* Code Documentation (thinking to use SandCastle)

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

  "FileControlTask": {
    "Files": [
      {
        "FileNames": [ "GlobalSupression.cs", "Settings.StyleCop" ],
        "AllowedUser": [ "DomainName\\MyUser" ],
        "AllowedGroup": [ "MyGroupthatAllowed" ]
      },
    ],
  },
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
6. Open TFS web interface and you can add TFS Sharp TR, before and after MSBuild.
7. Do not forget the set "PreBuild" and "PostBuild" comboboxes of TFS Sharp TR

### TfsSharpTR.Core Library and Dependencies
Newtonsoft.Json.dll <br />
Microsoft.TeamFoundation.Client.dll <br />
Microsoft.TeamFoundation.Common.dll <br />
Microsoft.TeamFoundation.VersionControl.Client.dll <br />
Microsoft.TeamFoundation.VersionControl.Common.dll <br />
Microsoft.TeamFoundation.Work.WebApi.dll <br />
Microsoft.VisualStudio.Services.Client.dll <br />
Microsoft.VisualStudio.Services.Common.dll <br />
Microsoft.VisualStudio.Services.WebApi.dll

### TfsSharpTR.StyleCopRelated Library and Dependencies

There is a catch here. If you want to check SA1650 rule, you should run powershell x86 mode. 2 more files needed too.

#### Normally :
StyleCop.dll <br />
StyleCop.CSharp.dll <br />
StyleCop.CSharp.Rules.dll

#### For SA1650 :
mssp7en.dll<br />
mssp7en.lex
