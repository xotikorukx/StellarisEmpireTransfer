# StellarisEmpireTransfer
Share your hand-made custom empires with your squad!

## Overview
This is a little C# shitscript I build to allow you to share empires with your friends. It uses the Server to Client model on TCP, meaning a PC needs to be running the server.

## Dependendices
.NET 8.0 https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- You'll want either the .NET Desktop Runtime or the .NET Runtime. This is a DOS-style console app-nothing pretty, just functional, so the barebones runtime will work fine.

## Get Started
- Download the latest release
- Run the server
- Either..
  - Hit "Allow" (if you're running windows)
  - Do something like `sudo ufw allow 35741` (if you're running unix. You can get fancy if you want! The app only uses TCP.)
  - Run the client on a machine that has stellaris installed
- Port forward on your router.
 
## How it Works
Once the server is running and forwarded, clients should just be able to connect. It will check first to see if a localhost server is available before trying to connect to remote.

The server runs in least-destructive mode.
- Upload to server overwrites the server version
  - Autofetch NEVER overwrites existing empires
- Delete from server NEVER deletes from the clients, even on force download with overwrite
- Force download with overwrite from server WILL overwrite server managed empires, but requires user input to initiate.
  - Make sure to UPLOAD your changes before doing a force overwrite to get your friends changes. Nothing is removed or changed on the client without user input - only added.

## For Developers (or code critique-ers)
It's a shitscript. I might update it to properly seperate netcode from empire management code, but, eh.

Also, I ran it through copilot in final testing because there was a wierd edge case I couldn't figure out (it fixed it, but didn't explain how 🤦), so if any of the code looks AI-ified, that's why.

## Building
Command line: 🤷
- It's "cross platform" (in theory, like, it might build, but might not run due to the `Environment.SpecialFolder` line), so the `dotnet` command line should work on poweshell or linux. I haven't tried it though.

Visual Studio
- At the top, hit "build" then "build solution".

## Known Issues
- Lots of error messages in the server console - netcode is a bit finicky. These can just be ignored unless it's on about file permissions.
- Lots of cases where it can hard crash. Not great error handling. It's a shitscript lol. As long as the files aren't locked and it can connect to the server, it'll be fine.

## License
Eh. It's not worth anyone selling, so :shrug:

Do whatever you want with it. I wrote it first, and I know I wrote it first, and nothing will take that from me.

## Credits
- Me! (xotikorukx)
- Also, copilot; for fixing a wierd socket closing case after a packet was sent. I think. I also fixed a few things after it did, so 🤷

## Disclaimers
I haven't tested it on linux, so I can't promise it will work on linux. The only possibly windows-only code I can think of is `Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);`. If you try to run it on linux, and it just won't, feel free to PR a fix. I will check and if possible, test it myself.

Also, I don't know if the directories are different if stellaris is running on say, steam deck, via proton, wine, or whatever. Like I said, untested, but feel free to post a PR. Only the client should break because the server creates and manages a *.txt in the executable's working directory.

## Project Status
Good enough for the guys I date! (But feel free to contribute. I check PR's and might update it more as I run into the need!)

## Legal Disclaimer
**I am NOT responsible for any damages or lost time/work. This software is provided AS IS and WITHOUT warranty.**
