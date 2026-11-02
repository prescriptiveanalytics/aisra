#!/bin/bash
set -e

docker compose down
docker compose build
./run-docker.sh
