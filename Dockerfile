# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["CouponsFrontend.csproj", "./"]
RUN dotnet restore "CouponsFrontend.csproj"

COPY . .
RUN dotnet build "CouponsFrontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CouponsFrontend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CouponsFrontend.dll"]
