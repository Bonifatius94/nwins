# Training Experiments

## How to Train
For training copy an existing training setup and modify some training settings.
Then start the training with docker-compose. This automatically creates docker 
containers running the specifified settings.

```sh
# move to the experiment to be run (e.g. experiment_01 of SimeplQL trainings)
cd experiments/train-simpleql/experiment_01

# launch the training sessions specified in the docker-compose file
docker-compose -f train-compose.yml up
```

## Training Folder Hierarchy
There may be a training folder for each learning algorithm.
Within those folders there may be numbered experiment folders containing the specific training setups.
