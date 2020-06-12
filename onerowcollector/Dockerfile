#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim 
#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
FROM mcr.microsoft.com/dotnet/core/sdk:3.1
LABEL maintainer="Tej Redkar <dynamicdeploy@live.com>"
COPY . App/
WORKDIR /App
ENTRYPOINT ["dotnet", "onerowcollector.dll"]
CMD ["www.microsoft.com"]

#override entrypoint docker run -it --rm --entrypoint "dotnet" onerowcollector onerowcollector.dll www.microsoft.com