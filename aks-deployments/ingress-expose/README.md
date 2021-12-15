# run docker engine
sudo service docker start

# create container in process
sudo docker run --rm -it -p 5263:5263 -p 7263:7263 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=7263 -e ASPNETCORE_Kestrel__Certificates__Default__Password=crypticpassword -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/ProfileAPI.pfx -v ${HOME}/.aspnet/https:/https/ profileapi

# create a contianer
sudo docker run -t -d -p 5263:5263 -p 7263:7263 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=7263 -e ASPNETCORE_Kestrel__Certificates__Default__Password=crypticpassword -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/ProfileAPI.pfx -v ${HOME}/.aspnet/https:/https/ profileapi

# create AKS

export RESOURCE_GROUP=ABastiuchenkoRG
export CLUSTER_NAME=ABastiuchenkoAKSCluster
export AZURE_CONTAINER_REGISTRY=ABastiuchenkoAzContainerRegistry
export KEY_VAULT_NAME=ABastiuchenkoK8sKeyVault
export LOCATION=eastus

az login

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create an ACR instance
az acr create --name $AZURE_CONTAINER_REGISTRY --resource-group $RESOURCE_GROUP --sku basic --admin-enabled


# create AKS cluster
az aks create \
    --resource-group $RESOURCE_GROUP \
    --name $CLUSTER_NAME \
    --node-count 1 \
    --enable-addons http_application_routing \
    --generate-ssh-keys \
    --node-vm-size Standard_B2s \
    --enable-managed-identity \
    --network-plugin azure \
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






## Attach Azure KeyVault
# Create AzKeyVault
az keyvault create --name $KEY_VAULT_NAME --resource-group $RESOURCE_GROUP --location $LOCATION

# Add secrets to Az Key vault
az keyvault secret set --vault-name $KEY_VAULT_NAME --name "ConnectionStrings--ProfileContext" --value "blablabla"


## Open access to key-vault through the managed identity
az aks enable-addons --addons azure-keyvault-secrets-provider --name $CLUSTER_NAME --resource-group $RESOURCE_GROUP

# AKS cluster must be created (or updated existing) with --enable-addons azure-keyvault-secrets-provider parameter like an example below
# az aks create -n $CLUSTER_NAME -g $RESOURCE_GROUP --enable-addons azure-keyvault-secrets-provider --enable-managed-identity
# A user-assigned managed identity, named azurekeyvaultsecretsprovider-*, will be created. All futher configuration to open access to KeyVault will be applied to this user-assigned managed identity
# In most cases there is one more a user-assigned managed identity named *agentpool that is usualy used for access to AzContainerRegistry.

# set policy to access secrets in your key vault using KV name and client id of the user-assigned managed identity, named azurekeyvaultsecretsprovider-*
az keyvault set-policy -n $KEY_VAULT_NAME --secret-permissions get list --spn <azurekeyvaultsecretsprovider-managed-identity-identity-client-id>

# Create the YAML script with a SecretProviderClass, using your own values for 
# -'userAssignedManagedIdentityID', named azurekeyvaultsecretsprovider-*
# -'keyvaultName'
# -'tenantId'
# and 'objects' to retrieve from your key vault


# Apply deployment and service to cluster
# kubectl delete pods -l app=azure-user-profile
replace manage-identity-client-id
kubectl apply -f aks-deployment-profileapi.yml --force
kubectl apply -f aks-secretproviderclass-profileapi.yml --force
kubectl apply -f aks-service-profileapi.yml --force
kubectl apply -f aks-ingress-profileapi.yml --force

kubectl get all

kubectl describe ing

# If the Address does not appear in prev command result - try apply yml below again
kubectl apply -f aks-service-profileapi.yml --force
kubectl apply -f aks-ingress-profileapi.yml --force
