FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY Transcriptor.Api/Transcriptor.Api.csproj Transcriptor.Api/
RUN dotnet restore Transcriptor.Api/Transcriptor.Api.csproj
COPY Transcriptor.Api/ Transcriptor.Api/
RUN dotnet publish Transcriptor.Api/Transcriptor.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Transcriptor.Api.dll"]
