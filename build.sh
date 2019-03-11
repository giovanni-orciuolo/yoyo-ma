#! /bin/bash

docker build --build-arg token=YOUR_TOKEN_HERE -t yoyo-discord-image YoYo-M$docker run -d --name yoyo-discord-app yoyo-discord-image
docker run -d --name yoyo-discord-app yoyo-discord-image
