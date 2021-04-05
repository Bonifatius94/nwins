using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nWins.Lib.Factory;

namespace nWins.Lib.Core
{
    /// <summary>
    /// Representing an immutable n-wins game state containing all information where the players stones were put.
    /// </summary>
    public readonly struct SimpleGameState : IGameState
    {
        /// <summary>
        /// Create a new game state instance of the given size (rows / columns).
        /// </summary>
        /// <param name="maxRows">The amount of rows to be assigned.</param>
        /// <param name="maxColumns">The amount of columns to be assigned.</param>
        internal SimpleGameState(int maxRows, int maxColumns)
        {
            // make sure the arguments are valid
            if (maxRows <= 0) { throw new ArgumentException("Invalid argument! Max rows needs to be greater than 0."); }
            if (maxColumns <= 0) { throw new ArgumentException("Invalid argument! Max columns needs to be greater than 0."); }

            _fields = new GameSide[maxRows * maxColumns];
            _columnStoneSums = new int[maxColumns];
            _maxColumns = maxColumns;
        }

        /// <summary>
        /// Creating a new game state using the given attributes (clone constructor, should only be used in GameStateFactory).
        /// </summary>
        /// <param name="fields">The fields array to be assigned.</param>
        /// <param name="columnStoneSums">The column stone sums cache to be assigned.</param>
        /// <param name="maxColumns">The amount of columns of the fields array.</param>
        internal SimpleGameState(GameSide[] fields, int[] columnStoneSums, int maxColumns)
        {
            _fields = fields;
            _columnStoneSums = columnStoneSums;
            _maxColumns = maxColumns;
        }

        private readonly GameSide[] _fields;
        private readonly int[] _columnStoneSums;
        private readonly int _maxColumns;

        public GameSide[] Fields => _fields;
        public int[] ColumnStoneSums => _columnStoneSums;
        public int MaxColumns => _maxColumns;
        public int MaxRows => Fields.Length / MaxColumns;

        public IGameState ApplyAction(GameAction action)
        {
            // make sure the action is valid
            if (action.ActingSide == GameSide.None) { throw new ArgumentException("Invalid arguments! Side must either be StoneA or StoneB!"); }
            if (action.ColumnIndex >= MaxColumns) { throw new IndexOutOfRangeException("Invalid arguments! Column index is out of range!"); }
            if (action.ActingSide != (ColumnStoneSums.Sum() % 2 == 0 ? GameSide.SideA : GameSide.SideB)) { throw new ArgumentException("Invalid arguments! Opposing side is supposed to draw!"); }
            if (ColumnStoneSums[action.ColumnIndex] == MaxRows) { throw new ArgumentException("Invalid arguments! Column is already entirely occupied! Please try another one."); }

            // make a deep copy of the fields
            var newFields = (GameSide[])Fields.Clone();

            // apply the action's stone to the fields array
            int insertionRow = ColumnStoneSums[action.ColumnIndex];
            newFields[action.ColumnIndex + MaxColumns * insertionRow] = action.ActingSide;

            // update the column stone sum for the column affected
            var newColumnStoneSums = (int[])ColumnStoneSums.Clone();
            newColumnStoneSums[action.ColumnIndex] = ColumnStoneSums[action.ColumnIndex] + 1;

            // create an immutable game status instance with the new attributes
            return new SimpleGameState(newFields, newColumnStoneSums, MaxColumns);
        }

        public IEnumerable<GameAction> GetPossibleActions()
        {
            var actingSide = ColumnStoneSums.Sum() % 2 == 0 ? GameSide.SideA : GameSide.SideB;

            int maxRows = MaxRows;
            var columnStoneSums = ColumnStoneSums;
            var availableColumns = Enumerable.Range(0, MaxColumns).Where(col => columnStoneSums[col] < maxRows);
            var actions = availableColumns.Select(col => new GameAction((uint)col, actingSide));
            
            return actions;
        }

        public bool IsConnectN(int n)
        {
            return isRowConnectN(n) 
                || isColumnConnectN(n)
                || isDiagonalConnectN(n);
        }

        private bool isRowConnectN(int n)
        {
            // loop through all row
            for (int rowOffset = 0; rowOffset < MaxRows; rowOffset++)
            {
                // loop through all column offsets
                for (int colOffset = 0; colOffset < MaxColumns - n; colOffset++)
                {
                    // check for connect-n row
                    if (isSingleConnectN(n, rowOffset, colOffset, 0, 1)) { return true; }
                }
            }

            return false;
        }

        private bool isColumnConnectN(int n)
        {
            // loop through all columns
            for (int colOffset = 0; colOffset < MaxColumns; colOffset++)
            {
                // loop through all row offsets
                for (int rowOffset = 0; rowOffset < MaxRows - n; rowOffset++)
                {
                    // check for connect-n column
                    if (isSingleConnectN(n, rowOffset, colOffset, 1, 0)) { return true; }
                }
            }

            return false;
        }

        private bool isDiagonalConnectN(int n)
        {
            // make sure that any diagonal fits into the board (otherwise abort)
            if (n > Math.Min(MaxColumns, MaxRows)) { return false; }

            // loop through all row offsets
            for (int rowOffset = 0; rowOffset < MaxRows - n; rowOffset++)
            {
                // loop through all column offsets
                for (int colOffset = 0; colOffset < MaxColumns - n; colOffset++)
                {
                    // check for left-to-right connect-n diagonal
                    if (isSingleConnectN(n, rowOffset, colOffset, 1, 1)) { return true; }

                    // check for right-to-left connect-n diagonal (reverse)
                    if (isSingleConnectN(n, rowOffset, MaxColumns - colOffset - 1, 1, -1)) { return true; }
                }
            }

            return false;
        }

        private bool isSingleConnectN(int n, int rowOffset, int colOffset, int rowUpdate, int colUpdate)
        {
            // initialize side with the sequence's first field's side
            var side = Fields[colOffset + rowOffset * MaxColumns];

            // process all n-1 other fields of the potential connect-n sequence
            for (int i = 1; i < n; i++)
            {
                // get the next field of the sequence
                var field = Fields[(colOffset + i * colUpdate) + MaxColumns * (rowOffset + i * rowUpdate)];

                // invalidate the field -> abort with false if the connect-i invariant is not met
                if (side == GameSide.None || side != field) { return false; }
            }

            // all partial sequences met the connect-i condition -> connect-n sequence found!
            return true;
        }

        #region Override

        public override string ToString()
        {
            string rowSeparator = "+";
            for (int i = 0; i < MaxColumns; i++) { rowSeparator += "---+"; }

            var builder = new StringBuilder();
            builder.AppendLine(rowSeparator);

            for (int row = MaxRows - 1; row >= 0; row--)
            {
                for (int column = 0; column < MaxColumns; column++)
                {
                    var field = Fields[column + MaxColumns * row];
                    builder.Append("|");
                    builder.Append($" { (field == GameSide.SideA ? "A" : (field == GameSide.SideB ? "B" : " ")) } ");
                }

                builder.AppendLine("|");
                builder.AppendLine(rowSeparator);
            }

            return builder.ToString();
        }

        public override bool Equals(object obj)
        {
            var state = obj as IGameState;
            return state != null && state.Equals(this);
        }

        public override int GetHashCode()
        {
            int hash = 0;

            for (int i = 0; i < 16 && i < MaxRows * MaxColumns; i++)
            {
                hash |= (int)Fields[i] << (i * 2);
            }

            return hash;
        }

        public bool Equals(IGameState other)
        {
            // compare fields, column stone sums and max columns / rows
            // perform cost efficient comparisons first to save computation power
            return this.MaxColumns == other.MaxColumns && this.MaxRows == other.MaxRows
                && this.ColumnStoneSums.SequenceEqual(other.ColumnStoneSums)
                && this.Fields.SequenceEqual(other.Fields);
        }

        public bool Equals(string base64Hash)
        {
            try
            {
                // try to interpret the string as Base64 hash
                var other = GameStateHashFactory.FromBase64Hash(base64Hash);

                // use the standard equator for comparing two GameState instances
                return this.Equals(other);
            }
            // instances are assumed to be not equal if the conversion fails -> return false
            catch (Exception) { return false; }
        }

        #endregion Override
    }
}