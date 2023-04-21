git config --global --add safe.directory *
dotnet restore
dotnet build -c Release -o artifacts/DockerBuild