#!/bin/bash
#Find $1 in current or parent directory
while [[ "$PWD" != "/" ]]; do
    find "$PWD" -maxdepth 1 -type f -name "$1"
    if [[ $? != 0 ]]; then
    	exit
    fi
    cd ..
done