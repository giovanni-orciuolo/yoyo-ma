FROM microsoft/dotnet:sdk AS builder
WORKDIR /app

# Copy csproj and NuGet.config then run dotnet restore as distinct layers
COPY NuGet.config *.csproj ./
RUN dotnet restore --configfile NuGet.config

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app

# Build environment (Replace your token here if you need it hardcoded everytime!)
ARG token=lol_no_token_here_skid_git_gud_lmao
ENV DISCORD_TOKEN=$token

# Install packages
RUN apt-get update -y && \
	apt-get install -y libopus0 libsodium18 libopus-dev libsodium-dev ffmpeg

COPY --from=builder /app/out .
ENTRYPOINT ["dotnet", "yoyo-bot.dll"]