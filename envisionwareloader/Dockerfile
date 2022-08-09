# Get build image
FROM mcr.microsoft.com/dotnet/sdk:2.1 AS dotnet-sdk
WORKDIR /app

# Copy source
COPY . ./

# Publish
RUN dotnet publish -c Release -o "/app/publish/"

# Get runtime image
FROM mcr.microsoft.com/dotnet/runtime:2.1
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
org.opencontainers.image.description="Load EnvisionWare data into a database for reporting" \
org.opencontainers.image.licenses="MIT" \
org.opencontainers.image.revision=$IMAGE_REVISION \
org.opencontainers.image.source="https://github.com/MCLD/EnvisionWareUsage" \
org.opencontainers.image.title="envisionwareloader" \
org.opencontainers.image.vendor="Maricopa County Library District" \
org.opencontainers.image.version=$IMAGE_VERSION

# Default image environment variable settings
ENV org.opencontainers.image.created=$IMAGE_CREATED \
org.opencontainers.image.revision=$IMAGE_REVISION \
org.opencontainers.image.version=$IMAGE_VERSION

# Copy source
COPY --from=dotnet-sdk "/app/publish/" .

# Set entrypoint
ENTRYPOINT ["dotnet", "EnvisionwareLoader.dll"]
