# ������� ����� � ASP.NET Core ��� ����������
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# ������� ����� ��� ������
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DIplomServer/DIplomServer.csproj", "DIplomServer/"]
RUN dotnet restore "./DIplomServer/DIplomServer.csproj"
COPY . .
WORKDIR "/src/DIplomServer"
RUN dotnet build "./DIplomServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ���������� ������
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DIplomServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish

# ��������� ����� � ������� �����������
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DIplomServer.dll"]
