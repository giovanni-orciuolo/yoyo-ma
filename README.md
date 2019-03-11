# YoYo-Ma bot for Discord

YoYo-Ma is a bot that I made in C# as an exercise to practice my skillz. It is still under development, so it lacks a lot of cool features etc, which I plan to add later on.

### Hosting with your app token

Currently this bot is not public, meaning that you have to self-host it on your own machine or on a cloud VPS.
At the moment, the bot will try to read the token from the environment variables (```process.env.DISCORD_TOKEN```) and if not found will attempt to read the token from a file called "token.txt" placed under the project root directory. If not found, it will simply crash with a message.

### Building for development and usage

This bot needs DSharpPlus Nightly v4.0.0. You can install all the subpackages if you wish, but the bot makes use only of: CommandsNext, VoiceNext, Lavalink (in the near future).
Since this bot also features a music service, it needs some other external dependencies to run correctly.

Download opus.dll and sodium.dll (rename them from libopus.dll and libsodium.dll!) and place them under the build output directory. For more information about this procedure [click here](https://dsharpplus.emzi0767.com/articles/vnext_setup.html).
You also need to download ffmpeg. You can find it under the "Natives" page in DSharpPlus docs, but I will mirror the links here: 
[FFmpeg (x64)](https://dsharpplus.emzi0767.com/natives/ffmpeg_win32_x64.zip) - [FFmpeg (x86)](https://dsharpplus.emzi0767.com/natives/ffmpeg_win32_x86.zip)

### Running with Docker

The Dockerfile provided should do all the dirty work for you. You just need to build the image and run it!
Just follow these commands, for a completely clean installation of the bot on Docker: 

```sh
$ git clone git@github.com:DoubleHub/YoYo-Ma.git
$ docker build --build-arg token=YOUR_TOKEN_HERE -t yoyo-discord-image YoYo-Ma/yoyo-bot/
$ docker run -d --name yoyo-discord-app yoyo-discord-image
```

Made by Giovanni Orciuolo under the MIT license
