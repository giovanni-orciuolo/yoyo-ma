#! /bin/bash

rm -rf YoYo-Ma
git clone git@github.com:DoubleHub/YoYo-Ma.git
docker build --build-arg token=YOUR_TOKEN_HERE -t yoyo-discord-image YoYo-M$docker run -d --name yoyo-discord-app yoyo-discord-image
docker run -d --name yoyo-discord-app yoyo-discord-image
