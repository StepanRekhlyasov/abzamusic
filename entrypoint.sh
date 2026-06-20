#!/bin/sh
set -e

export ASPNETCORE_URLS=http://+:8080

dotnet backend.dll &
exec nginx -g 'daemon off;'
