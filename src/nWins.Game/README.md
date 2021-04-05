# nWins Game

## How to Build
```sh
# prepare .mono directory (do this only once when setting up the git repo)
# note: this only work for the bash console, so make sure to run godot-mono inside a bash console context
godot-mono project.godot

# build the source code
msbuild nwins-ui.sln

# load the trained AI models into models directory
mkdir model
# download the file from https://megastore.uni-augsburg.de/get/ettzrfKWFf/ and unzip it

# start the game ui
godot-mono
```