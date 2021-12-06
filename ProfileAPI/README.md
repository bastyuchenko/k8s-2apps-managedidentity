# create container in process
sudo docker run --rm -it -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_Kestrel__Certificates__Default__Password=crypticpassword -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/ProfileAPI.pfx -v ${HOME}/.aspnet/https:/https/ profileapi

# create a contianer
sudo docker run -t -d -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_Kestrel__Certificates__Default__Password=crypticpassword -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/ProfileAPI.pfx -v ${HOME}/.aspnet/https:/https/ profileapi

# create AKS

export RESOURCE_GROUP=ABastiuchenkoRG
export CLUSTER_NAME=ABastiuchenkoAKSforK8s
export AZURE_CONTAINER_REGISTRY=ABastiuchenkoAzContainerRegistry

# Create an ACR instance
az acr create --name $AZURE_CONTAINER_REGISTRY --resource-group $RESOURCE_GROUP --sku basic --admin-enabled

# Create images localy ad tag ones
docker-compose build
docker tag profileapi:latest abastiuchenkoazcontainerregistry.azurecr.io/profileapi:v1

# Push local images to Azure Container Registry
az login
az acr login --name $AZURE_CONTAINER_REGISTRY
docker push abastiuchenkoazcontainerregistry.azurecr.io/profileapi:v1

# create AKS cluster
## Option 1 (Recommended)
az aks create \
    --resource-group $RESOURCE_GROUP \
    --name $CLUSTER_NAME \
    --node-count 1 \
    --enable-addons http_application_routing \
    --generate-ssh-keys \
    --node-vm-size Standard_B2s \
    --network-plugin azure

## OR Option 2 
az aks create \
    --resource-group $RESOURCE_GROUP \ 
    --name $CLUSTER_NAME \
    --node-count 1 \
    --generate-ssh-keys \
    --node-vm-size Standard_B2s \
    --location westeurope