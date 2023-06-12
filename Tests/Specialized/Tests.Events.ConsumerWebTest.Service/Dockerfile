#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Tests.Events.ConsumerWebTest/Tests.Events.ConsumerWebTest.csproj", "Tests.Events.ConsumerWebTest/"]
RUN dotnet restore "Tests.Events.ConsumerWebTest/Tests.Events.ConsumerWebTest.csproj"
COPY . .
WORKDIR "/src/Tests.Events.ConsumerWebTest"
RUN dotnet build "Tests.Events.ConsumerWebTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tests.Events.ConsumerWebTest.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tests.Events.ConsumerWebTest.dll"]