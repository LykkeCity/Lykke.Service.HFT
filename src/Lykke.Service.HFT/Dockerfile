FROM microsoft/aspnetcore:2.0
WORKDIR /app
EXPOSE 80
COPY . .
COPY /runtimes/debian-x64/native/libuv.so /app
ENTRYPOINT ["dotnet", "Lykke.Service.HFT.dll"]
