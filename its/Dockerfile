# Get build image
FROM mcr.microsoft.com/dotnet/sdk:2.1 AS dotnet-sdk
WORKDIR /app

# Copy source
COPY . ./

# Run restore and build
RUN dotnet build -c Release

# Publish
RUN dotnet publish -c Release -o "/app/publish/" "its/" --no-build

# Get runtime image
FROM mcr.microsoft.com/dotnet/aspnet:2.1
WORKDIR /app

# Bring in metadata via --build-arg
ARG BRANCH=unknown
ARG IMAGE_CREATED=unknown
ARG IMAGE_REVISION=unknown
ARG IMAGE_VERSION=unknown

# Configure image labels
LABEL branch=$branch \
maintainer="Maricopa County Library District developers <development@mcldaz.org>" \
org.opencontainers.image.authors="Maricopa County Library District developers <development@mcldaz.org>" \
org.opencontainers.image.created=$IMAGE_CREATED \
org.opencontainers.image.description="visualize aggregated EnvisionWare data on a Web page" \
org.opencontainers.image.licenses="MIT" \
org.opencontainers.image.revision=$IMAGE_REVISION \
org.opencontainers.image.source="https://github.com/MCLD/EnvisionWareUsage" \
org.opencontainers.image.title="its" \
org.opencontainers.image.vendor="Maricopa County Library District" \
org.opencontainers.image.version=$IMAGE_VERSION

# Default image environment variable settings
ENV org.opencontainers.image.created=$IMAGE_CREATED \
org.opencontainers.image.revision=$IMAGE_REVISION \
org.opencontainers.image.version=$IMAGE_VERSION

# Copy source
COPY --from=dotnet-sdk "/app/publish/" .

# Port 80 for http
EXPOSE 80

# Set entrypoint
ENTRYPOINT ["dotnet", "its.dll"]
