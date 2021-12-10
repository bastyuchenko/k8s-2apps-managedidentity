# create container in process
sudo docker run --rm -it -p 5263:5263 -p 7263:7263 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=7263 -e ASPNETCORE_Kestrel__Certificates__Default__Password=crypticpassword -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/ProfileAPI.pfx -v ${HOME}/.aspnet/https:/https/ profileapi

# create a contianer
sudo docker run -t -d -p 5263:5263 -p 7263:7263 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=7263 -e ASPNETCORE_Kestrel__Certificates__Default__Password=crypticpassword -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/ProfileAPI.pfx -v ${HOME}/.aspnet/https:/https/ profileapi

# create AKS

export RESOURCE_GROUP=ABastiuchenkoRG
export CLUSTER_NAME=ABastiuchenkoAKSCluster
export AZURE_CONTAINER_REGISTRY=ABastiuchenkoAzContainerRegistry
export LOCATION=eastus

az login

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create an ACR instance
az acr create --name $AZURE_CONTAINER_REGISTRY --resource-group $RESOURCE_GROUP --sku basic --admin-enabled


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
    --enable-addons azure-keyvault-secrets-provider \
    --enable-managed-identity \
    --location $LOCATION


# Allow the AKS cluster to pull images from the ACR
az aks update --name $CLUSTER_NAME --resource-group $RESOURCE_GROUP --attach-acr $AZURE_CONTAINER_REGISTRY






# Create images localy ad tag ones
docker-compose build
docker tag profileapi:latest abastiuchenkoazcontainerregistry.azurecr.io/profileapi

# Push local images to Azure Container Registry
# az login
az acr login --name $AZURE_CONTAINER_REGISTRY
docker push abastiuchenkoazcontainerregistry.azurecr.io/profileapi

# Get credentials from the AKS Cluster
az aks get-credentials --resource-group $RESOURCE_GROUP --name $CLUSTER_NAME

# Apply deployment and service to cluster
# kubectl delete pods -l name=azure-user-profile
kubectl apply -f aks-deploy-profileapi.yml --force

kubectl get all





## Attach Azure KeyVault
export KEY_VAULT_NAME=ABastiuchenkoK8sKeyVault

# Create AzKeyVault
az keyvault create --name $KEY_VAULT_NAME --resource-group $RESOURCE_GROUP --location $LOCATION

# Add secrets to Az Key vault
az keyvault secret set --vault-name $KEY_VAULT_NAME --name "" --value ""