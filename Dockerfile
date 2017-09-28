FROM microsoft/dotnet:2.0-sdk AS build-env
WORKDIR /app

COPY . ./
RUN apt-get update \
    && apt-get install nuget -y \
    && nuget sources Add -Name Bintray -Source https://api.bintray.com/nuget/fint/nuget \
    && mkdir /root/.nuget \
    && mkdir /root/.nuget/NuGet \
    && cp /root/.config/NuGet/NuGet.Config /root/.nuget/NuGet/
RUN dotnet restore

RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.0-runtime
WORKDIR /app
COPY --from=build-env /app/Fint.Sse.Adapter.Console/out .
COPY --from=build-env /app/Fint.Sse.Adapter.Console/appsettings.json .
ENTRYPOINT ["dotnet", "Fint.Sse.Adapter.Console.dll"]