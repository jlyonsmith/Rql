#!/bin/bash
find . -name \*.nupkg -maxdepth 1 | xargs -n 1 mono ~/lib/NuGet/NuGet.exe push
