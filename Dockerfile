FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ./src/LearningApp.API/*.csproj ./src/LearningApp.API/
COPY ./src/LearningApp.Application/*.csproj ./src/LearningApp.Application/
COPY ./src/LearningApp.Domain/*.csproj ./src/LearningApp.Domain/
COPY ./src/LearningApp.Infrastructure/*.csproj ./src/LearningApp.Infrastructure/

RUN dotnet restore src/LearningApp.API/LearningApp.API.csproj

# copy everything else and build
COPY . .
WORKDIR /src/src/LearningApp.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT

COPY --from=build /app/publish ./

EXPOSE 80

ENTRYPOINT ["dotnet", "LearningApp.API.dll"]
