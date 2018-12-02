cls
set initialPath=%cd%
set srcPath=%cd%\CatFactory.Dapper
set testPath=%cd%\CatFactory.Dapper.Tests
set outputBasePath=C:\Temp\CatFactory.Dapper
cd %srcPath%
dotnet build
cd %testPath%
dotnet test
cd %outputBasePath%\OnLineStore.Core.UnitTests
dotnet test
cd %outputBasePath%\Northwind.Core.UnitTests
dotnet test
cd %outputBasePath%\AdventureWorks.Core.UnitTests
dotnet test
cd %srcPath%
dotnet pack
cd %initialPath%
pause
