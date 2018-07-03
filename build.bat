cls
set initialPath=%cd%
set srcPath=%cd%\CatFactory.Dapper\CatFactory.Dapper
set testPath=%cd%\CatFactory.Dapper\CatFactory.Dapper.Tests
set outputBasePath=C:\Temp\CatFactory.Dapper
cd %srcPath%
dotnet build
cd %testPath%
dotnet test
cd %outputBasePath%\Store\Store.Dapper.API.Tests
dotnet test
cd %outputBasePath%\Northwind\Northwind.Dapper.API.Tests
dotnet test
cd %outputBasePath%\AdventureWorks\AdventureWorks.Dapper.API.Tests
dotnet test
cd %srcPath%
dotnet pack
cd %initialPath%
pause
