#https://github.com/dotnet/dotnet-docker/blob/main/samples/build-in-sdk-container.md
docker run --rm -v ${pwd}:c:\app -w c:\app gitex_docker_image docker_build_gitex.cmd
