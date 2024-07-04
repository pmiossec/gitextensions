# ************* Build a custom version of dotnet 6 windowsservercore image **WITH GIT**
# because BugReporterTool needs git during the build

# From https://stackoverflow.com/a/61298126/717372
# Build using a docker image: https://github.com/dotnet/dotnet-docker/blob/main/samples/build-in-sdk-container.md
# Available dotnet images: https://hub.docker.com/_/microsoft-dotnet-sdk/?tab=description
FROM mcr.microsoft.com/dotnet/sdk:6.0.408-windowsservercore-ltsc2022

# include git in image
COPY ./artifacts/DockerTmp/git ./git

USER ContainerAdministrator
RUN setx /M Path "C:\git\bin;C:\git\usr\bin;C:\git\mingw64\bin;C:\nodejs;%Path%"
RUN echo %PATH%
RUN git --version

#RUN git config --global --add safe.directory *



