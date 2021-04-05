import pandas as pd

# Constants for dataframe columns
LEARNING_STEPS = 'learningSteps'
WINRATE_A = 'winRateA'
WINRATE_B = 'winRateB'
TIE_RATE = 'tieRate'
EPISODE = 'episode'

def get_winrate_df_from_csv(winrates_path):
    # Read dataframe from csv file
    header_list = [LEARNING_STEPS, WINRATE_A, WINRATE_B, TIE_RATE]
    winrate_df = pd.read_csv(winrates_path, names=header_list)

    # Add Column with summed episodes
    winrate_df[EPISODE] = 0
    episode_sum = 0
    for index, row in winrate_df.iterrows():
        episode_sum += row[LEARNING_STEPS]
        winrate_df.at[index, EPISODE] = int(episode_sum)

    return winrate_df
