#!/bin/bash

BUILDCONFIG=$1

if [[ -z "$BUILDCONFIG" ]]; then
    BUILDCONFIG=Release
fi

SCRIPTDIR=$(cd $(dirname $0); pwd -P)
SCRIPTNAME=$(basename $0)
SLNPATH=$(${SCRIPTDIR}/upfind.sh *.sln)
SLNDIR=$(dirname $SLNPATH)
SLNFILE=$(basename $SLNPATH)
APPNAME="${SLNFILE%.*}"
NUGET="mono ${HOME}/lib/NuGet/NuGet.exe"

# Do a release build, and stop if it fails
bash -c "cd $SLNDIR; xbuild /property:Configuration=${BUILDCONFIG} ${APPNAME}.sln"
if [[ $? -ne 0 ]]; then exit 1; fi

# Create new packages
bash -c "cd $SLNDIR/Scratch; rm *.nupkg; ${NUGET} pack ${SLNDIR}/Rql.nuspec; ${NUGET} pack ${SLNDIR}/Rql.MongoDB.nuspec"

# Push them to NuGet
bash -c "cd $SLNDIR/Scratch; find . -name \*.nupkg -maxdepth 1 | xargs -n 1 ${NUGET} push"
