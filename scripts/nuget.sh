set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder


dotnet build ./src/Aix.NatsMessageBus/Aix.NatsMessageBus.csproj -c Release

dotnet pack ./src/Aix.NatsMessageBus/Aix.NatsMessageBus.csproj -c Release -o $artifactsFolder

dotnet nuget push ./$artifactsFolder/Aix.NatsMessageBus.*.nupkg -k $PRIVATE_NUGET_KEY -s https://www.nuget.org
