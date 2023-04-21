$tmpPath=".\artifacts\DockerTmp\"
#Download and extract git
mkdir $tmpPath
Invoke-WebRequest -Uri "https://github.com/git-for-windows/git/releases/download/v2.40.0.windows.1/PortableGit-2.40.0-64-bit.7z.exe" -OutFile "$tmpPath\PortableGit.7z.exe"
& "$tmpPath\PortableGit.7z.exe" -o"$tmpPath\git" -y
docker build -t gitex_docker_image .