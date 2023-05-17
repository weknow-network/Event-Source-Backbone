FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Tests/Weknow.EventSource.Backbone.WebEventTest/Weknow.EventSource.Backbone.WebEventTest.csproj", "Tests/Weknow.EventSource.Backbone.WebEventTest/"]

RUN dotnet restore "Tests/Weknow.EventSource.Backbone.WebEventTest/Weknow.EventSource.Backbone.WebEventTest.csproj"
COPY . .
WORKDIR "/src/Tests/Weknow.EventSource.Backbone.WebEventTest"
RUN dotnet build "Weknow.EventSource.Backbone.WebEventTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Weknow.EventSource.Backbone.WebEventTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Weknow.EventSource.Backbone.WebEventTest.dll"]