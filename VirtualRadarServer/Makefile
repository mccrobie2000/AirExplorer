
all:
	@echo 1.  Edit profile file add Microsoft.EntityFrameworkCore.Tools.DotNet as a DotNetCliToolReference item
	@echo 2.  dotnet add package Microsoft.EntityFrameworkCore.Design
	@echo 3.  dotnet add package Microsoft.EntityFrameworkCore
	@echo 4.  dotnet add package Microsoft.EntityFrameworkCore.Sqlite
	@echo 5.  dotnet restore
	@echo 6.  add connection string somewhere
	@echo 7.  make dbcontext
	@echo See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet

dbcontext:
	dotnet restore
	dotnet ef dbcontext scaffold -c StandingDataContainer -o Models -t Airport -t Country "Data Source=Database/StandingData.sqb" Microsoft.EntityFrameworkCore.Sqlite
