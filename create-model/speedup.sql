
CREATE TABLE NwinsQtable_1 AS
SELECT * FROM NwinsQtable WHERE AgentType = 1;

CREATE TABLE NwinsQtable_2 AS
SELECT * FROM NwinsQtable WHERE AgentType = 2;

CREATE TABLE NwinsQtable_3 AS
SELECT * FROM NwinsQtable WHERE AgentType = 3;

CREATE TABLE NwinsQtable_4 AS
SELECT * FROM NwinsQtable WHERE AgentType = 4;


CREATE INDEX IX_Speedup_1 ON NwinsQtable_1 (HashBefore, Column);
CREATE INDEX IX_Speedup_2 ON NwinsQtable_2 (HashBefore, Column);
CREATE INDEX IX_Speedup_3 ON NwinsQtable_3 (HashBefore, Column);
CREATE INDEX IX_Speedup_4 ON NwinsQtable_4 (HashBefore, Column);
