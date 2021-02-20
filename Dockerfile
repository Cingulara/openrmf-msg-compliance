FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
RUN mkdir /app
WORKDIR /app

# copy the project and restore as distinct layers in the image
COPY src/*.csproj ./
RUN dotnet restore

# copy the rest and build
COPY src/ ./
RUN dotnet build
RUN dotnet publish --runtime alpine-x64 -c Release -o out --self-contained true /p:PublishTrimmed=true

# build runtime image
FROM cingulara/openrmf-base-api:1.03.00
RUN apk update && apk upgrade

RUN mkdir /app
WORKDIR /app
COPY --from=build-env /app/out .
COPY ./nlog.config /app/nlog.config

# Create a group and user
RUN addgroup --system --gid 1001 openrmfprogroup \
&& adduser --system -u 1001 --ingroup openrmfprogroup --shell /bin/sh openrmfprouser
RUN chown openrmfprouser:openrmfprogroup /app

USER 1001
ENTRYPOINT ["./openrmf-msg-compliance"]
