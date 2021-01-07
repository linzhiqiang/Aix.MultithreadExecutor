set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder

dotnet restore ./Aix.MultithreadExecutor.sln
dotnet build ./Aix.MultithreadExecutor.sln -c Release


dotnet pack ./src/Aix.MultithreadExecutor/Aix.MultithreadExecutor.csproj -c Release -o $artifactsFolder
