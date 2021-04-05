# Final Exam - n Wins Game

## About
This project contains a reinforcement learning approach for playing an adaptation of the very popular game *Connect Four*, called *Connect N*.
This includes game logic, UI and AI components.

## Game Rules
The original game consists of 6 rows and 7 columns. Both players can put one of their stones into a column alternatingly. 
When putting a stone into a column, the stone drops to the bottom of the column, so the columns get filled up with stones
while the game progresses. The main goal for both players is to connect 4 of their own stones, either as row, column or diagonal.
As soon as one player manages to do so, the game is over.

For better adaptability there have been made some modifications to the original game such as parameterizing 
the rows and columns of the board and allowing to define the amount of stones to connect. 
Therefore the new game is named *Connect N* (or *n-Gewinnt* as we say in Germany).

## Quickstart

### How to play the game
This section is about getting the Godot game to work (using an Archlinux Manjaro distro).

1) Set up your machine using a script from the setups directory (see the README for further information).
2) Build the Godot game app (see the README from src/nWis.Game/)
3) Launch the Godot game app you just built

### How to train the AI
This section shows how to run some dockerized AI trainings. Some examples are covered in the experiments/ directory.

1) Set up your machine to run Docker, Docker-Compose and Git (and VSCode as text editor).
```sh
# install docker and git (Ubuntu 20.04)
sudo apt-get update && sudo apt-get install -y docker.io docker-compose git
sudo usermod -aG docker $USER && reboot

# install VSCode text editor (optional)
sudo snap install code --classic
```

2) Download the source code from GitHub.
```sh
# clone the git repository
git clone https://hcm-lab.de/git/course/rl/2020/g02
cd g02
```

3) Build the Docker image and start a training session with a sample configuration.
```sh
# build the Dockerfile and tag the image nwins:latest
docker build . -t "nwins"

# start a sample training with two random agents playing against each other
docker run --name training_001 nwins 00_rand_vs_rand.json
```

4) Create your own experiments with Docker-Compose (see the experiments directory for further information).

5) Extract the training results and convert it from CSV to a database model (SQLite).

6) Copy your trained model into the model/ directory of your binary Godot app output.

### Useful Docker commands for manual configuration
For working with dockerized training sessions following commands may be useful:

```sh
# show all running containers
docker ps

# create a new training session with training settings file 'rand_vs_rand.json'
# specify the container name 'training_001' using --name option
docker run --name training_001 nwins 00_rand_vs_rand.json

# start / stop explicit containers (can also be a list of containers)
docker start training_001
docker stop training_001

# show logs from a container
docker logs training_001

# extract log files and trained models from a container and copy them to the host OS
docker cp training_001:/app/train .

# attach to the container via bash console
docker exec -it training_001 bash
```
