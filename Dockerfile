FROM ghcr.io/vmelnalksnis/gnomeshade-build:8.0.403 AS build

WORKDIR /gnomeshade
COPY ./ ./
ARG BUILD_NUMBER=123
RUN --mount=type=cache,target=/root/.nuget/packages \
    ./deployment/publish.sh "Gnomeshade.WebApi" "linux-musl-x64" $BUILD_NUMBER

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.4-alpine3.20 as gnomeshade

WORKDIR /gnomeshade
COPY --chmod=-w --from=build [ \
"/gnomeshade/source/Gnomeshade.WebApi/bin/Release/net8.0/linux-musl-x64/publish/Gnomeshade.WebApi", \
"/gnomeshade/source/Gnomeshade.WebApi/bin/Release/net8.0/linux-musl-x64/publish/libe_sqlite3.so", \
"/gnomeshade/source/Gnomeshade.WebApi/bin/Release/net8.0/linux-musl-x64/publish/appsettings.json", \
"/gnomeshade/source/Gnomeshade.WebApi/bin/Release/net8.0/linux-musl-x64/publish/*.xml", \
"./" ]

COPY --chmod=-w --from=build /gnomeshade/source/Gnomeshade.WebApi/bin/Release/net8.0/linux-musl-x64/publish/wwwroot/ ./wwwroot

ENV DOTNET_gcServer=0 \
	Database__Provider="Sqlite" \
	ConnectionStrings__Gnomeshade="Data Source=/home/app/gnomeshade.db" \
	Jwt__ValidAudience="http://localhost:5000" \
	Jwt__ValidIssuer="http://localhost:5000" \
	Jwt__Secret="280ba7e4-d323-4232-8107-3b8c1b0832a8"

USER app
VOLUME /home/app
EXPOSE 8080

ENTRYPOINT ["./Gnomeshade.WebApi"]
