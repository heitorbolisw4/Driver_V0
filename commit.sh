#!/bin/sh
set -e

cd "$(dirname "$0")"

if [ -z "$1" ]; then
  echo "Usage: ./commit.sh \"commit message\""
  exit 1
fi

git add -A
git commit -m "$1"
