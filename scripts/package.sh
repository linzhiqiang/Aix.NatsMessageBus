set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder

dotnet restore ./Aix.NatsMessageBus.sln
dotnet build ./Aix.NatsMessageBus.sln -c Release


dotnet pack ./src/Aix.NatsMessageBus/Aix.NatsMessageBus.csproj -c Release -o $artifactsFolder
