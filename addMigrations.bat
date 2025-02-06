@echo off
SET MIGRATION_NAME=%1
IF "%MIGRATION_NAME%"=="" (
    echo "Migration name is required."
    exit /b 1
)
dotnet ef migrations add %MIGRATION_NAME% --project "C:\Users\elnar\source\repos\DIplomServer\DIplomServer" --startup-project "C:\Users\elnar\source\repos\DIplomServer\DIplomServer" --context "Hbtcontext"