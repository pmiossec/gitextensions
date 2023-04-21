# This script builds the docker image that will be used to build GitExtensions
$tmpPath=".\artifacts\DockerTmp\"
#Download and extract git (to include it in the docker image)
mkdir $tmpPath
Invoke-WebRequest -Uri "https://github.com/git-for-windows/git/releases/download/v2.40.0.windows.1/PortableGit-2.40.0-64-bit.7z.exe" -OutFile "$tmpPath\PortableGit.7z.exe"
& "$tmpPath\PortableGit.7z.exe" -o"$tmpPath\git" -y
docker build -t image_dotnet_and_git_for_gitex .