#!/bin/bash
rm *.nupkg
mono ~/lib/NuGet/NuGet.exe pack Rql.nuspec
mono ~/lib/NuGet/NuGet.exe pack Rql.MongoDB.nuspec
