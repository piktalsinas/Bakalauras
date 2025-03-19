# Use the official .NET SDK as the base image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the project files and restore dependencies
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o /out

# Use the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

# Expose the correct port
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Start the application
CMD ["dotnet", "Bakalauras.API.dll"]
