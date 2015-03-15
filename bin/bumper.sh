#!/bin/bash
function evil_git_dirty {
  [[ $(git diff --shortstat 2> /dev/null | tail -n1) != "" ]] && echo "*"
}

SCRIPTDIR=$(cd $(dirname $0); pwd -P)
SLNPATH=$(${SCRIPTDIR}/upfind.sh *.sln)
SLNDIR=$(dirname $SLNPATH)
SLNFILE=$(basename $SLNPATH)
APPNAME="${SLNFILE%.*}"
pushd $SLNDIR
if [[ $(evil_git_dirty) == "*" ]]; then {
    echo "error: You have uncommitted changes! Please stash or commit them first."
    exit 1
}; fi
vamper -u
if [[ $? -ne 0 ]]; then 
	git checkout -- .
	exit 1
fi
VERSIONFILE=Scratch/${APPNAME}.version.txt
if [[ ! -e $VERSIONFILE ]]; then
	git checkout -- .
	echo "error: Version file" $VERSIONFILE "does not exist"
	exit 1
fi
git add . 
mkdir scratch 2> /dev/null
git commit -F $VERSIONFILE
popd