# Multi-stage build for ASP.NET Core application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory
WORKDIR /src

# Copy project files
COPY ["SimpleLMS.csproj", "./"]
RUN dotnet restore "SimpleLMS.csproj"

# Copy all source code
COPY . .

# Build the application
RUN dotnet build "SimpleLMS.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SimpleLMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Set working directory
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Create directory for uploads
RUN mkdir -p /app/wwwroot/uploads/pdfs

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "SimpleLMS.dll"] 