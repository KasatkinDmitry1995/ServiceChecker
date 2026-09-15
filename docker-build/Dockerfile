# Используем базовый образ без жесткого указания платформы.
# Docker сам подберет нужную архитектуру.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем файлы проекта
COPY ["src/", "./"]
COPY ["data/", "/app/data/"]
# --- КЛЮЧЕВОЙ МОМЕНТ ---
# Объявляем аргумент, который BuildKit автоматически заполнит
# архитектурой целевой платформы (amd64, arm64 и т.д.)
ARG TARGETARCH

# Передаем архитектуру в dotnet restore.
# Флаг -a (или --arch) заставляет .NET скачивать пакеты
# именно для целевой архитектуры, а не для архитектуры хоста.
RUN dotnet restore -a $TARGETARCH

# Собираем и публикуем приложение также с указанием архитектуры.
RUN dotnet publish -c Release -o /app/publish -a $TARGETARCH

# --- Финальный образ ---
# Убираем жесткое указание платформы и здесь.
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS final
WORKDIR /app

# Устанавливаем tini (правильно, на отдельной строке!)
RUN apk add --no-cache tini

# Копируем опубликованные файлы
COPY --from=build /app/publish .

# Указываем точку входа. Используем tini для правильной обработки сигналов.
ENTRYPOINT ["/sbin/tini", "--", "dotnet", "ServiceChecker.dll"]