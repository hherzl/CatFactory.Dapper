cls
set initialPath=%cd%
set srcPath=%cd%\src\CatFactory.Dapper
set testPath=%cd%\test\CatFactory.Dapper.Tests
cd %srcPath%
dotnet build
cd %testPath%
dotnet test
cd %srcPath%
dotnet pack
cd %initialPath%
pause