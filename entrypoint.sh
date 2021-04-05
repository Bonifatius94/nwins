#!/usr/bin/env bash

# ===================================================
#             TRAINING TASK LAUNCHER
# ===================================================

# make sure that there is exactly one script argument
if [ $# -ne 1 ]; then
    echo "Invalid entrypoint arguments! Expecting exactly one argument!"
    exit 1
fi

# when the help option is used, print the USAGE to console and exit
if [ "--help" == "$1" ]; then
    echo "USAGE"
    echo "================================"
    echo "docker run nwins <settings_file>"
    echo "settings_file: relative path to the settings file in $SETTINGS_ROOT location"
    exit 0
fi

# write environment settings to docker logs
echo "environment settings:"
echo "==========================================="
echo "  src root:      $SRC_ROOT"
echo "  settings root: $SETTINGS_ROOT"
echo "  logs root:     $LOGS_ROOT"
echo "  models root:   $MODELS_ROOT"
echo "==========================================="
echo ""

# determine the settings file to be used
SETTINGS=$1

# determine the logfile path
LOGFILE=$LOGS_ROOT/log_$(echo $SETTINGS | cut -f 1 -d '.')_$(date "+%Y-%m-%d_%H-%M-%S").txt

# create dirs to store logs and trained models
mkdir -p $LOGS_ROOT
mkdir -p $MODELS_ROOT

# write training task settings to docker logs
echo "training settings:"
echo "==========================================="
echo "  settings file: $SETTINGS_ROOT/$SETTINGS"
echo "  log file:      $LOGFILE"
echo "==========================================="
echo ""

# start the training task
dotnet nWins.Training.dll --settings $SETTINGS 2>&1 | tee $LOGFILE &

# add handler for SIGTERM ('docker stop') and INT (Ctrl+C) signal
# on signal -> gracefully exit training loop
trap 'kill -15 $(pgrep dotnet) && sleep 8' SIGTERM INT
wait

# ===================================================
#                    2021-01-01
# ===================================================
