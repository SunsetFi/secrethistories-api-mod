# Secret Histories API

A RESTful API server for allowing externally reading data from and interacting with games built with the Secret Histories engine.

It currently supports:

- [Cultist Simulator](https://store.steampowered.com/app/718670/Cultist_Simulator/)
- [Book of Hours](https://store.steampowered.com/app/1028310/BOOK_OF_HOURS/)

At the moment, this mod is not published to the steam workshop or provided as a download, it must be built manually.

## Requirements

### Book of Hours

The Book of Hours build requires BepInEx to be installed into Book of Hours. This is needed both for the build step, and to run the mod.

### Cultist Simulator

The Cultist Simulator build is standalone and uses the game's built in mod loader. Since it is a DLL mod, the [GHIRBI](https://steamcommunity.com/sharedfiles/filedetails/?id=2901287611) mod must also be installed.

## Building

To build this project, some environment variables must be specified:

- The `GAME` env var must be set to `BH` for Book of Hours, or `CS` for Cultist Simulator
- The `BOHDir` env var must be set to the steam installation folder for Book of Hours.
- The `CSDir` env var must be set to the steam installation folder for Cultist Simulator.

Once these are set, the project can be built with `dotnet publish`. The build output will automatically be copied into your game's mod folder.

## Usage

Once the mod is installed, it will open port 8081 from within the game engine and listen to requests there.

See the API docs for usage information.

If you are looking to build a webapp or nodejs application that interacts with the game, you may wish to see the companion [secrethistories-api](https://www.npmjs.com/package/secrethistories-api) npm library providing a typescript-typed API wrapper around the endpoints of this mod.

## API Docs

Documentation for the API can be found at:

https://sunsetfi.github.io/secrethistories-api-mod