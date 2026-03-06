FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 9008

FROM base AS final
WORKDIR /app
COPY ./publish .
ENV ASPNETCORE_URLS=http://*:9008
ENTRYPOINT ["dotnet", "IFinancing360_ICS_UI.dll"]
