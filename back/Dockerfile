# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# نسخ ملفات الـ csproj لجميع الطبقات (بما فيها الدومين) لتسريع الـ Restore
COPY ["HotelBooking.API/HotelBooking.API.csproj", "HotelBooking.API/"]
COPY ["HotelBooking.Application/HotelBooking.Application.csproj", "HotelBooking.Application/"]
COPY ["HotelBooking.Domain/HotelBooking.Domain.csproj", "HotelBooking.Domain/"]
COPY ["HotelBooking.Infrastructure/HotelBooking.Infrastructure.csproj", "HotelBooking.Infrastructure/"]

# عمل Restore للاعتمادات
RUN dotnet restore "HotelBooking.API/HotelBooking.API.csproj"

# نسخ بقية الكود بالكامل
COPY . .

WORKDIR /src/HotelBooking.API
RUN dotnet publish "HotelBooking.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HotelBooking.API.dll"]