
# use the official base image with a preinstalled Microsoft dotnet 5.0 SDK 
FROM mcr.microsoft.com/dotnet/sdk:5.0

# install procps package (required to use pgrep in entrypoint.sh)
RUN apt-get update && apt-get install -y procps && rm -rf /var/lib/apt/lists/*

# set environment variables
ENV SRC_ROOT=/app/src
ENV BIN_ROOT=/app/bin
ENV SETTINGS_ROOT=/app/settings
ENV LOGS_ROOT=/app/train/logs
ENV MODELS_ROOT=/app/train/models

# set build args
ARG RELEASE_PLATFORM=linux-x64
ARG CONFIG=Release

WORKDIR $SRC_ROOT

# copy only *.sln and *.csproj files to keep the .NET dependencies in docker layer cache
# this avoids having to reload all NuGet packages on each docker image rebuild task
COPY src/nWins.sln $SRC_ROOT/nWins.sln
COPY src/nWins.Lib/nWins.Lib.csproj $SRC_ROOT/nWins.Lib/nWins.Lib.csproj
COPY src/nWins.Lib.Test/nWins.Lib.Test.csproj $SRC_ROOT/nWins.Lib.Test/nWins.Lib.Test.csproj
COPY src/nWins.Training/nWins.Training.csproj $SRC_ROOT/nWins.Training/nWins.Training.csproj
COPY src/nWins.Mappings/nWins.Mappings.csproj $SRC_ROOT/nWins.Mappings/nWins.Mappings.csproj
RUN dotnet restore nWins.sln
RUN dotnet restore nWins.sln -r $RELEASE_PLATFORM

# copy the source code into the image
COPY src $SRC_ROOT

# build the dotnet code, run all unit tests and publish the binaries to bin root
RUN dotnet build --no-restore -c $CONFIG
RUN dotnet test --no-restore --no-build -c $CONFIG -r $RELEASE_PLATFORM 
RUN dotnet publish --no-restore -c $CONFIG -r $RELEASE_PLATFORM -o $BIN_ROOT
WORKDIR $BIN_ROOT

# copy the settings
COPY settings $SETTINGS_ROOT

# start the entrypoint task
COPY entrypoint.sh /
ENTRYPOINT ["/entrypoint.sh"]
