from pathlib import Path
import pandas as pd
import sqlite3

if __name__=='__main__':
    model_path = Path(Path().cwd().parents[0] / 'src' / 'nWins.Game' / 'model' / 'NwinsQtable.db')
    conn = sqlite3.connect(model_path)

    #Create a cursor object using the cursor() method
    cursor = conn.cursor()

    #Drop WinsQtable.db table if already exists.
    cursor.execute("DROP TABLE IF EXISTS nWinsQtable")

    #Create sql statement
    sql ='''CREATE TABLE NwinsQtable(
    AgentType  INTEGER NOT NULL,
    HashBefore CHAR(12) NOT NULL,
    Column     INTEGER  NOT NULL,
    QValue     DOUBLE   NOT NULL,

    PRIMARY KEY (AgentType, HashBefore, Column)
    )'''
    cursor.execute(sql)
    print("Table created successfully........")

    # Commit changes in the database
    conn.commit()

    # Load agent models from csvs into sqlite table
    models_csv_path = Path('models')

    # Create AgentType dictionary
    agentTypes = {  "SimpleQL" : 1,
                    "DoubleQL" : 2,
                    "DynaQL" : 3,
                    "SarsaLambda" : 4}

    for model_csv in models_csv_path.rglob("*"):
        # Read dataframe from csv file
        header_list = ["HashBefore", "HashAfter", "ActingSide", "Column", "QValue"]
        qtable_df = pd.read_csv(model_csv, names=header_list)

        # Drop Hash After and ActingSide
        qtable_df = qtable_df.drop(columns=["HashAfter", "ActingSide"])
        print("Model read from csv")

        # Add Column with agent name
        qtable_df["AgentType"] = agentTypes[model_csv.name.split('_')[0]]

        # Write only best actions in sql
        qtable_df = qtable_df.sort_values("QValue", ascending=False).drop_duplicates(["AgentType", "HashBefore"])

        qtable_df.to_sql('NwinsQtable', conn, if_exists='append', index=False)

        print("Model inserted into db")

    conn.close()


