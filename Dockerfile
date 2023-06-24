FROM ghcr.io/vmelnalksnis/gnomeshade-build:7.0.305 AS build

WORKDIR /gnomeshade
COPY ./ ./
ARG BUILD_NUMBER=123
RUN ./deployment/publish.sh "Gnomeshade.WebApi" "linux-musl-x64" $BUILD_NUMBER

FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.5-alpine3.17 as gnomeshade

COPY --from=build /gnomeshade/source/Gnomeshade.WebApi/bin/Release/net7.0/linux-musl-x64/publish/Gnomeshade.WebApi /gnomeshade/
COPY --from=build /gnomeshade/source/Gnomeshade.WebApi/bin/Release/net7.0/linux-musl-x64/publish/libe_sqlite3.so /gnomeshade/
COPY --from=build /gnomeshade/source/Gnomeshade.WebApi/bin/Release/net7.0/linux-musl-x64/publish/appsettings.json /gnomeshade/
COPY --from=build /gnomeshade/source/Gnomeshade.WebApi/bin/Release/net7.0/linux-musl-x64/publish/*.xml /gnomeshade/
COPY --from=build /gnomeshade/source/Gnomeshade.WebApi/bin/Release/net7.0/linux-musl-x64/publish/wwwroot/ /gnomeshade/wwwroot

ENV Database__Provider="Sqlite" \
	ConnectionStrings__Gnomeshade="Data Source=/data/gnomeshade.db" \
	Jwt__ValidAudience="http://localhost:5000" \
	Jwt__ValidIssuer="http://localhost:5000" \
	Jwt__Secret="280ba7e4-d323-4232-8107-3b8c1b0832a8"

VOLUME /data
EXPOSE 80

WORKDIR /gnomeshade
ENTRYPOINT ["./Gnomeshade.WebApi"]
