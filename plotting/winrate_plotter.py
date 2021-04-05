import pandas as pd
import matplotlib.pyplot as plt
import errno
import os
from pathlib import Path

import data_helper as dh

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

            # loop through win rates directories in logs
            for content_path in logs_path.rglob("*"):
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

                # Create new plot
                plt.figure()

                # Plot Win Rate A
                plt.plot(winrate_df[dh.EPISODE], winrate_df[dh.WINRATE_A], label=dh.WINRATE_A)

                # Plot Win Rate B
                plt.plot(winrate_df[dh.EPISODE], winrate_df[dh.WINRATE_B], label=dh.WINRATE_B)

                # Plot Tie Rate
                plt.plot(winrate_df[dh.EPISODE], winrate_df[dh.TIE_RATE], label=dh.TIE_RATE)

                # Set ylim between 0 and 1
                plt.ylim(0.0, 1.0)

                # Set xlim left 0
                plt.xlim(left=0)

                # Set the x and y axis label
                plt.xlabel(dh.EPISODE)
                plt.ylabel('win/tie rate')

                # Show legend
                plt.legend()

                # Create path to plots directory
                plots_path = experiment_directory_path /'plots'

                # Check plot directory is available
                if not plots_path.exists():
                    os.mkdir(plots_path)

                # Create path to winrate directory
                win_rates_path = Path(plots_path / 'winrate')

                # Check winrate directory is available
                if not win_rates_path.exists():
                    os.mkdir(win_rates_path)

                # Get image name from source directroy
                image_name = content_path.name + '.png'

                # Create image path
                image_path = win_rates_path / image_name

                # Save plot as image
                plt.savefig(image_path)

