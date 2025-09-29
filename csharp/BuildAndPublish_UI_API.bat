cd MaxPainUI

del "..\..\publish\*.*" /Q
del "..\..\publishAPI\*.*" /Q

"C:\Program Files\Microsoft Visual Studio\2022\Community\dotnet\net8.0\runtime\dotnet.exe" restore MaxPainUI.sln

"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\msbuild.exe" MaxPainUI.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DeployOnBuild=true /p:PublishProfile=FolderProfile

cd ..\MaxPainAPI

"C:\Program Files\Microsoft Visual Studio\2022\Community\dotnet\net8.0\runtime\dotnet.exe" restore MaxPainAPI_ImportOnly.sln

"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\msbuild.exe" MaxPainAPI_ImportOnly.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DeployOnBuild=true /p:PublishProfile=FolderProfileAPI

copy "..\..\publishAPI" "..\..\publish" /y


pause
