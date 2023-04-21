#https://github.com/dotnet/dotnet-docker/blob/main/samples/build-in-sdk-container.md
docker run --rm -v ${pwd}:c:\app -w c:\app image_dotnet_and_git_for_gitex docker_build_gitex.cmd
