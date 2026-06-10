#!/bin/sh
set -e

APP_PORT="${PORT:-8080}"

PORT=8081 dotnet /app/api/Alphabit.API.dll &
API_PID="$!"

trap 'kill "$API_PID" 2>/dev/null || true' INT TERM EXIT

export PORT="$APP_PORT"
export AlphabitApi__BaseUrl="${AlphabitApi__BaseUrl:-http://127.0.0.1:8081/}"

dotnet /app/web/Alphabit.App.dll
