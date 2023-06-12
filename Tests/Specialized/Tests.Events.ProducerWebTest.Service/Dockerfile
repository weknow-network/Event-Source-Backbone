#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Tests.Events.ProducerWebTest/Tests.Events.ProducerWebTest.csproj", "Tests.Events.ProducerWebTest/"]
RUN dotnet restore "Tests.Events.ProducerWebTest/Tests.Events.ProducerWebTest.csproj"
COPY . .
WORKDIR "/src/Tests.Events.ProducerWebTest"
RUN dotnet build "Tests.Events.ProducerWebTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tests.Events.ProducerWebTest.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tests.Events.ProducerWebTest.dll"]