FROM microsoft/dotnet:sdk AS builder
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore --configfile NuGet.config

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app

# Install packages
RUN apt-get install libopus0 libsodium18 libopus-dev libsodium-dev

COPY --from=builder /app/out .
ENTRYPOINT ["dotnet", "yoyo-bot.dll"]
