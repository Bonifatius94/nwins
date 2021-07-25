
from pathlib import Path
import pandas as pd
import sqlite3, sys


# create AgentType dictionary
agentTypes = {
    "SimpleQL" : 1,
    "DoubleQL" : 2,
    "DynaQL" : 3,
    "SarsaLambda" : 4
}

SQL_CREATE_SCHEMA = '''CREATE TABLE NwinsQtable(
    AgentType  INTEGER NOT NULL,
    HashBefore CHAR(12) NOT NULL,
    Column     INTEGER  NOT NULL,
    QValue     DOUBLE   NOT NULL,

    PRIMARY KEY (AgentType, HashBefore, Column)
)'''


def create_db_from_models(db_filepath: str, csv_model_files: list):

    # create SQLite database file (or just connect to it if it already exists)
    conn = sqlite3.connect(db_filepath)

    # create a cursor object to apply transactional database changes
    cursor = conn.cursor()

    # drop WinsQtable.db table if it already exists
    cursor.execute("DROP TABLE IF EXISTS nWinsQtable")
    print("Database cleanup performed........")

    # create the database schema
    cursor.execute(SQL_CREATE_SCHEMA)
    print("Schema created successfully........")

    # commit all changes to the database file
    # now, the database should be ready for applying the trained model files
    conn.commit()

    # detect all CSV files 
    for model_csv in csv_model_files:

        # read dataframe from csv file
        header_list = ["HashBefore", "HashAfter", "ActingSide", "Column", "QValue"]
        qtable_df = pd.read_csv(model_csv, names=header_list)

        # drop HashAfter and ActingSide columns (not required, would just bloat the database)
        qtable_df = qtable_df.drop(columns=["HashAfter", "ActingSide"])
        print("Model read from csv")

        # add column with agent name
        qtable_df["AgentType"] = agentTypes[model_csv.name.split('_')[0]]

        # write only best actions in sql
        qtable_df = qtable_df.sort_values("QValue", ascending=False).drop_duplicates(["AgentType", "HashBefore"])
        qtable_df.to_sql('NwinsQtable', conn, if_exists='append', index=False)
        print("Model inserted into db")

    conn.close()


# define script args
ARG_OUTFILE = '--outfile'
ARG_CSV_MODELS = '--csv-models'
ARG_HELP = '--help'
ALL_ARG_SPECIFIERS = [ARG_OUTFILE, ARG_CSV_MODELS, ARG_HELP]
USAGE_MESSAGE =
'''
SCRIPT USAGE:
================
options:
  {}: the target SQLite database file
  {}: the trained CSV models to be written to the database (as list, separated by white spaces)
'''.format(ARG_OUTFILE, ARG_CSV_MODELS)


def main():

    # print usage info if --help option appears
    if ARG_HELP in sys.argv: print(USAGE_MESSAGE)

    # validate script arguments
    if len(sys.argv) < 5: raise ValueError('Invalid arguments! Insufficient script arguments specified! Use --help option for more information!')
    if ARG_OUTFILE not in sys.argv: raise ValueError('Invalid arguments! Script argument {} is missing!'.format(ARG_OUTFILE))
    if ARG_CSV_MODELS not in sys.argv: raise ValueError('Invalid arguments! Script argument {} is missing!'.format(ARG_CSV_MODELS))

    # parse script arguments
    sqlite_filepath = sys.argv[sys.argv.index(ARG_OUTFILE) + 1]
    csv_filepaths = []
    first_csv_model = sys.argv.index(ARG_CSV_MODELS) + 1
    for i in range(first_csv_model, len(sys.argv)):
        if sys.argv[i] not in ALL_ARG_SPECIFIERS: csv_filepaths.append(sys.argv[i])

    model_path = Path(Path().cwd().parents[0] / 'src' / 'nWins.Game' / 'model' / 'NwinsQtable.db')
    csv_models_pattern = Path('models')


if __name__=='__main__':
    main()
