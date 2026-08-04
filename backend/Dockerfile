FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AssignmentSystem.slnx ./
COPY src/AssignmentSystem.Api/AssignmentSystem.Api.csproj src/AssignmentSystem.Api/
RUN dotnet restore src/AssignmentSystem.Api/AssignmentSystem.Api.csproj

COPY src/AssignmentSystem.Api/ src/AssignmentSystem.Api/
RUN dotnet publish src/AssignmentSystem.Api/AssignmentSystem.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app .

ENTRYPOINT ["dotnet", "AssignmentSystem.Api.dll"]
