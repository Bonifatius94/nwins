using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using nWins.Lib.Core;
using nWins.Lib.Factory;

// get all recursive transitions for the initial game state of a 4x5 board
var state = GameStateFactory.CreateState(4, 5);

// initialize empty adjacency lookup tree (leafs have value 'empty list')
var adjacency = new Dictionary<IGameState, IGameState[]>();

// visit all child states and write them to the adjacency
visitChildStates(state, adjacency);

// select nodes with adjacency (no leafs)
var startNodes = adjacency.Where(pair => pair.Value.Length > 0).Select(pair => pair.Key).ToArray();

// write adjacency to file
using (var writer = new StreamWriter("transitions.csv", false, Encoding.ASCII, 8192))
foreach (var startNode in startNodes)
{
    string startHash = GameStateHashFactory.ToBase64Hash(startNode);
    var destHashes = adjacency[startNode].Select(x => GameStateHashFactory.ToBase64Hash(x));
    var lines = destHashes.Select(destHash => startHash + "," + destHash);
    foreach (string line in lines) { writer.WriteLine(line); }
}

static void visitChildStates(IGameState root, Dictionary<IGameState, IGameState[]> adjacency)
{
    // abort if the adjacency already knows the root state
    if (adjacency.ContainsKey(root)) { return; }

    // apply all possible actions to get the children of the current state
    var actions = root.GetPossibleActions();
    var childStates = actions.Select(x => root.ApplyAction(x)).ToArray();

    // write the transitions to adjacency
    adjacency.Add(root, childStates);

    // visit all children their descendants recursively (until there are no more possible actions)
    childStates.Where(x => !x.IsConnectN(4)).ToList().ForEach(x => visitChildStates(x, adjacency));
}