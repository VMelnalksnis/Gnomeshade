FROM ghcr.io/vmelnalksnis/gnomeshade-build:8.0.101 AS build

WORKDIR /gnomeshade
COPY ./ ./
ARG BUILD_NUMBER=123
RUN ./deployment/publish.sh "Gnomeshade.WebApi" "linux-musl-x64" $BUILD_NUMBER

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.1-alpine3.18 as gnomeshade

WORKDIR /gnomeshade
COPY --from=build /gnomeshade/source/Gnomeshade.WebApi/bin/Release/net8.0/linux-musl-x64/publish/Gnomeshade.WebApi ./
COPY --from=build /gnomeshade/source/Gnomeshade.WebApi/bin/Release/net8.0/linux-musl-x64/publish/libe_sqlite3.so ./
COPY --from=build /gnomeshade/source/Gnomeshade.WebApi/bin/Release/net8.0/linux-musl-x64/publish/appsettings.json ./
COPY --from=build /gnomeshade/source/Gnomeshade.WebApi/bin/Release/net8.0/linux-musl-x64/publish/*.xml ./
COPY --from=build /gnomeshade/source/Gnomeshade.WebApi/bin/Release/net8.0/linux-musl-x64/publish/wwwroot/ ./wwwroot
RUN chmod -R -w *

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
