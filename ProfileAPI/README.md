# create container in process
sudo docker run --rm -it -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_Kestrel__Certificates__Default__Password=crypticpassword -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/ProfileAPI.pfx -v ${HOME}/.aspnet/https:/https/ profileapi

# create a contianer
sudo docker run -t -d -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_Kestrel__Certificates__Default__Password=crypticpassword -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/ProfileAPI.pfx -v ${HOME}/.aspnet/https:/https/ profileapi