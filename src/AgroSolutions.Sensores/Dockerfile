# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Configurar UTF-8 para Alpine Linux
ENV LANG=C.UTF-8 \
    LC_ALL=C.UTF-8 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Instalar dependências de build
RUN apk add --no-cache icu-libs icu-data-full

# Copiar os arquivos de projeto e restaurar dependências
COPY ["src/AgroSolutions.Sensores/AgroSolutions.Sensores.csproj", "AgroSolutions.Sensores/"]
COPY ["src/AgroSolutions.SharedKernel/AgroSolutions.SharedKernel.csproj", "AgroSolutions.SharedKernel/"]
RUN dotnet restore "AgroSolutions.Sensores/AgroSolutions.Sensores.csproj" --runtime linux-musl-x64

# Copiar o código fonte e compilar
COPY src/AgroSolutions.Sensores/ AgroSolutions.Sensores/
COPY src/AgroSolutions.SharedKernel/ AgroSolutions.SharedKernel/
WORKDIR "/src/AgroSolutions.Sensores"
RUN dotnet build "AgroSolutions.Sensores.csproj" -c Release -o /app/build --no-restore

# Publish Stage
FROM build AS publish
RUN dotnet publish "AgroSolutions.Sensores.csproj" -c Release -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# Instalar dependências de runtime
RUN apk add --no-cache icu-libs icu-data-full tzdata ca-certificates

# Configurar timezone e globalization
ENV TZ=America/Sao_Paulo \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_HTTP_PORTS=8080

# Criar usuário não-root para segurança
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser && \
    chown -R appuser:appgroup /app

# Copiar arquivos publicados
COPY --from=publish --chown=appuser:appgroup /app/publish .

# Mudar para usuário não-root
USER appuser

# Expor porta
EXPOSE 8080

ENTRYPOINT ["dotnet", "AgroSolutions.Sensores.dll"]
