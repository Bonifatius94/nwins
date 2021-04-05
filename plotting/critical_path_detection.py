
import pandas as pd
import numpy as np
import sys, os, glob


class GameTree(object):

    def __init__(self, path_transitions: str):

        # load the transitions csv file
        self.trans_headers = ['state_before', 'state_after']
        self.transitions = pd.read_csv(path_transitions, names=self.trans_headers)

        print('created game tree from {}'.format(path_transitions))


    def next_states(self, state: str):

        # get all following states from transitions
        next_states = self.transitions[self.transitions['state_before'] == state]
        next_states = next_states[['state_after']].values.tolist()
        next_states = [a[0] for a in next_states]

        return next_states


    def is_terminal(self, state: str):
        return len(self.next_states(state)) == 0


class CsvAgent(object):

    def __init__(self, path_qtable: str, game_tree: GameTree):

        # assign transitions dataframe
        self.game_tree = game_tree

        # load Q table from csv file
        self.qtable_headers = ['state_before', 'state_after', 'acting_side', 'affected_column', 'q_value']
        self.qtable = pd.read_csv(path_qtable, names=self.qtable_headers)

        print('created agent from {}'.format(path_qtable))


    def best_next_state(self, state: str):

        # get all Q values from the Q table
        next_states = self.game_tree.next_states(state)
        q_values = self.qtable[self.qtable['state_after'].isin(next_states)]

        q_values = q_values['q_value']

        # determine the action with the best Q value
        crit_state_index = q_values.idxmax()

        critical_state = self.qtable.iloc[crit_state_index][['state_after']]

        critical_column = self.qtable.iloc[crit_state_index][['affected_column']]

        critical_state = critical_state.values.tolist()[0]

        critical_column = critical_column.values.tolist()[0]

        return critical_state, critical_column


def get_critical_path(game_tree: GameTree, agent_a: CsvAgent, agent_b: CsvAgent):

    # initialize the critical path walk
    initial_state = 'BAUAAAAAAA=='
    crit_path = [initial_state]
    crit_columns_path = []
    is_player_a_acting = True

    # loop until a terminal game state is reached
    while not game_tree.is_terminal(crit_path[-1]):

        # choose the best consecutive state according to the Q table
        acting_player = agent_a if is_player_a_acting else agent_b
        critical_state, crit_column = acting_player.best_next_state(crit_path[-1])

        # update critical path and acting side
        crit_path.append(critical_state)
        crit_columns_path.append(crit_column)
        is_player_a_acting = not is_player_a_acting

    return crit_path, crit_columns_path


def main():

    # make sure that the Q tables for players A and B are specified
    if len(sys.argv) < 3: raise ValueError("Invalid arguments! Call this script with following arguments: \n"
        + "<path_qtable_a.csv>, <path_qtable_b.csv>, [<transitions.csv>]\n"
        + "The third argument is optional and defaults to './transitions.csv'.")

    # parse script args
    path_qtable_a = sys.argv[1]
    path_qtable_b = sys.argv[2]
    path_transitions = sys.argv[3]

    # make sure that the given files exist before trying to load them
    if not os.path.exists(path_qtable_a) or not os.path.isfile(path_qtable_a): raise ValueError("Q-Table A could not be found!")
    if not os.path.exists(path_qtable_b) or not os.path.isfile(path_qtable_b): raise ValueError("Q-Table B could not be found!")
    if not os.path.exists(path_transitions) or not os.path.isfile(path_transitions): raise ValueError("Transitions could not be found!")

    # create game tree and agents
    game_tree = GameTree(path_transitions)
    agent_a = CsvAgent(path_qtable_a, game_tree)
    agent_b = CsvAgent(path_qtable_b, game_tree)

    # walk the critical path
    crit_path, crit_columns_path = get_critical_path(game_tree, agent_a, agent_b)
    print('critical path:', crit_path)
    print('crit critical path:', crit_columns_path)


 # run the main script
if __name__ == '__main__':
    main()