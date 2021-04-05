import pandas as pd
import matplotlib.pyplot as plt
import errno
import os
import numpy as np
from pathlib import Path

import data_helper as dh


# Counts the number of learning agents in one log path
def count_learning_agents_in_logs_path(logs_path):
    count = 0

    # loop through win rates directories in logs
    for content_path in logs_path.rglob("*"):
        # check if content is directory
        if not content_path.is_dir():
            continue

        # Create file path to win_rates.csv
        win_rates_path = content_path / 'win_rates.csv'

        # Check if win rate csv exists
        if not win_rates_path.exists():
            continue

        # Get directory name
        directory_name = content_path.name
        # Split directory name into agent names
        agent_names = directory_name.split('_vs_')

        # Iterate over both agents
        for agent in agent_names:
            # Plot only winrates for not-random agents
            if 'Random' in agent:
                continue

            count = count + 1

    return count


if __name__=='__main__':
    # Create path to experiments directory
    experiments_path = Path(Path().cwd().parents[0] / 'experiments')

    # Loop through all training directories
    for train_directory_path in experiments_path.rglob("*"):
        # check if train_directory_path is directory
        if not train_directory_path.is_dir():
            continue
        
        # Loop through experiment directories
        for experiment_directory_path in train_directory_path.rglob("*"):
            # check if experiment_directory_path is directory
            if not experiment_directory_path.is_dir():
                continue

            # Create path to train/logs
            logs_path = experiment_directory_path / 'train/logs'

            # Check path to train/logs is available
            if not logs_path.exists():
                    continue
            
            # Create new plot for each experiment
            plt.figure()

            # Current agent number
            i = 0

            # Number of learning agents in this experiment
            n = count_learning_agents_in_logs_path(logs_path)

            # loop through win rates directories in logs
            for content_path in sorted(logs_path.rglob("*")):
                # check if content is directory
                if not content_path.is_dir():
                    continue

                # Create file path to win_rates.csv
                win_rates_path = content_path / 'win_rates.csv'

                # Check if win rate csv exists
                if not win_rates_path.exists():
                    raise FileNotFoundError(errno.ENOENT, os.strerror(errno.ENOENT), win_rates_path)

                # Read dataframe from csv file
                winrate_df = dh.get_winrate_df_from_csv(win_rates_path)

                # Get directory name
                directory_name = content_path.name
                # Split directory name into agent names
                agent_names = directory_name.split('_vs_')

                # Iterate over both agents
                for agent in agent_names:
                    # Plot only winrates for not-random agents
                    if 'Random' in agent:
                        continue

                    # Get side of agent
                    side = dh.WINRATE_A if agent.split('_')[-1] == 'SideA' else dh.WINRATE_B

                    # Create list with winrates to investigate
                    winrate_bars = [0.60, 0.70, 0.80, 0.85, 0.90, 0.95, 0.97, 0.99]

                    # Create empty list for episode marks
                    episodes_for_winrate = []

                    # Get episodes, when the winrates are reached
                    for winrate in winrate_bars:
                        found_episode = False
                        for index, row in winrate_df.iterrows():               
                            if row[side] > winrate and not found_episode:
                                episodes_for_winrate.append(row[dh.EPISODE])
                                found_episode = True
                                break

                        if not found_episode:
                            episodes_for_winrate.append(0)

                    width = 0.175

                    # the label locations
                    label_locations = np.arange(len(winrate_bars))  

                    # Plot winrate of agent
                    plt.bar(label_locations - ((n - 1)/2 * width) + i * width, episodes_for_winrate, width, label=agent)

                    i = i + 1

            
            # Set tick labels
            plt.xticks(label_locations, winrate_bars)
  
            # Set the x and y axis label
            plt.xlabel('win rate')
            plt.ylabel(dh.EPISODE)         

            # Show legend
            plt.legend()

            # Create path to plots directory
            plots_path = experiment_directory_path /'plots'

            # Check plot directory is available
            if not plots_path.exists():
                    os.mkdir(plots_path)

            # Create path to convergence_speed directory
            convergence_speed_path = Path(plots_path / 'convergence_speed')

            # Check convergence_speed directory is available
            if not convergence_speed_path.exists():
                    os.mkdir(convergence_speed_path)

            # Get image name from source directroy
            image_name = 'convergence_speed.png'

            # Create image path
            image_path = convergence_speed_path / image_name

            # Save plot as image
            plt.savefig(image_path)
