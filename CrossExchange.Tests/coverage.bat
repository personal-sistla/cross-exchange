@echo off

dotnet clean
dotnet build /p:DebugType=Full
dotnet minicover instrument --workdir ../ --assemblies CrossExchange.Tests/**/bin/**/*.dll --sources CrossExchange/**/*.cs --exclude-sources CrossExchange/Migrations/**/*.cs --exclude-sources CrossExchange/*.cs --exclude-sources CrossExchange\Repository\ExchangeContext.cs

dotnet minicover reset --workdir ../

dotnet test --no-build
dotnet minicover uninstrument --workdir ../
dotnet minicover htmlreport --workdir ../ --threshold 90
dotnet minicover report --workdir ../ --threshold 60