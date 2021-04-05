using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using nWins.Lib.Factory;

namespace nWins.Lib.Core
{
    /// <summary>
    /// Representing an immutable n-wins game state containing all information where the players stones were put.
    /// This instance is optimized for small board sizes using very efficient bitwise operations.
    /// Therefore the maximum supported fields are 64 which also includes a standard sized board with 6 rows and 7 columns.
    /// </summary>
    public readonly struct BitwiseGameState : IGameState, IEquatable<BitwiseGameState>
    {
        #region Preparation

        // static pre-computed masks for parameters (columns, win-conn)
        private static readonly ulong[,] ROW_MASKS = initRowMasks();
        private static readonly ulong[,] COL_MASKS = initColumnMasks();
        private static readonly ulong[,] DIAG_MASKS = initDiagonalMasks();
        private static readonly ulong[,] REV_DIAG_MASKS = initReverseDiagonalMasks();

        private static ulong[,] initRowMasks()
        {
            // info: don't worry about performance, this is only executed once on app startup

            // theoretical maximum of columns and win-conn for a 64 fields board
            var masks = new ulong[64, 64];

            // loop through all possible amounts of columns
            for (int cols = 1; cols <= 64; cols++)
            {
                int maxRows = 64 / cols;
                ulong tempMask = 0ul;

                // increase n until it entirely fills up all columns of a row
                for (int n = 1; n <= 64 && n <= cols; n++)
                {
                    tempMask |= 1ul << (n - 1);
                    masks[cols - 1, n - 1] = tempMask;
                }
            }

            return masks;
        }

        private static ulong[,] initColumnMasks()
        {
            // info: don't worry about performance, this is only executed once on app startup

            // theoretical maximum of columns and win-conn for a 64 fields board
            var masks = new ulong[64, 64];

            // loop through all possible amounts of columns
            for (int cols = 1; cols <= 64; cols++)
            {
                int maxRows = 64 / cols;
                ulong tempMask = 0ul;

                // increase n until it entirely fills up all rows of a column
                for (int n = 1; n <= 64 && n <= maxRows; n++)
                {
                    tempMask |= 1ul << ((n - 1) * cols);
                    masks[cols - 1, n - 1] = tempMask;
                }
            }

            return masks;
        }

        private static ulong[,] initDiagonalMasks()
        {
            // info: don't worry about performance, this is only executed once on app startup

            // theoretical maximum of columns and win-conn for a 64 fields board
            var masks = new ulong[64, 64];

            // loop through all possible amounts of columns
            for (int cols = 1; cols <= 64; cols++)
            {
                int maxRows = 64 / cols;
                ulong tempMask = 0ul;

                // increase n until it entirely fills up all rows of a column
                for (int n = 1; n <= 64 && n <= maxRows && n <= cols; n++)
                {
                    tempMask |= 1ul << (((n - 1) * cols) + (n - 1));
                    masks[cols - 1, n - 1] = tempMask;
                }
            }

            return masks;
        }

        private static ulong[,] initReverseDiagonalMasks()
        {
            // info: don't worry about performance, this is only executed once on app startup

            // theoretical maximum of columns and win-conn for a 64 fields board
            var masks = new ulong[64, 64];

            // loop through all possible amounts of columns
            for (int cols = 1; cols <= 64; cols++)
            {
                int maxRows = 64 / cols;

                // increase n until it entirely fills up all rows of a column
                for (int n = 1; n <= 64 && n <= maxRows && n <= cols; n++)
                {
                    ulong tempMask = 0ul;
                    for (int i = 0; i < n; i++) { tempMask |= 1ul << ((i * cols) + (n - i - 1)); }
                    masks[cols - 1, n - 1] = tempMask;
                }
            }

            return masks;
        }

        // access pre-computed masks using getters
        private static ulong getRowMask(int cols, int n) => ROW_MASKS[cols - 1, n - 1];
        private static ulong getColMask(int cols, int n) => COL_MASKS[cols - 1, n - 1];
        private static ulong getDiagMask(int cols, int n) => DIAG_MASKS[cols - 1, n - 1];
        private static ulong getRevDiagMask(int cols, int n) => REV_DIAG_MASKS[cols - 1, n - 1];

        #endregion Preparation

        /// <summary>
        /// Create a new game state instance of the given size (rows / columns) with empty fields.
        /// </summary>
        /// <param name="maxRows">The amount of rows to be assigned.</param>
        /// <param name="maxColumns">The amount of columns to be assigned.</param>
        internal BitwiseGameState(int maxRows, int maxColumns)
        {
            // make sure the arguments are valid
            if (maxRows <= 0) { throw new ArgumentException("Invalid argument! Max rows needs to be greater than 0."); }
            if (maxColumns <= 0) { throw new ArgumentException("Invalid argument! Max columns needs to be greater than 0."); }
            if (maxColumns * maxRows > 64) { throw new ArgumentException("Invalid argument! Too many fields, rows multiplied by columns must not be greater than 64!"); }

            // initialize game state with an empty board
            _maxRows = (byte)maxRows;
            _maxColumns = (byte)maxColumns;
            _fieldsSideA = 0x0000000000000000ul;
            _fieldsSideB = 0x0000000000000000ul;
            _columnStoneSums = 0x00000000u;
        }

        /// <summary>
        /// Creating a new game state using the given state (clone constructor, should only be used in GameStateFactory).
        /// </summary>
        /// <param name="state">The fields array to be assigned.</param>
        internal BitwiseGameState(IGameState state)
        {
            // make sure the arguments are valid
            if (state.MaxRows <= 0) { throw new ArgumentException("Invalid argument! Max rows needs to be greater than 0."); }
            if (state.MaxColumns <= 0) { throw new ArgumentException("Invalid argument! Max columns needs to be greater than 0."); }
            if (state.MaxRows * state.MaxColumns > 64) { throw new ArgumentException("Invalid argument! Too many fields, rows multiplied by columns must not be greater than 64!"); }

            // convert fields and columns stone sums to the more efficient data format
            var convFields = convertFields(state.Fields);
            var convColumnStoneSums = convertColumnStoneSums(state.ColumnStoneSums);

            // initialize game state with the converted data
            _maxRows = (byte)state.MaxRows;
            _maxColumns = (byte)state.MaxColumns;
            _fieldsSideA = convFields.Item1;
            _fieldsSideB = convFields.Item2;
            _columnStoneSums = convColumnStoneSums;
        }

        internal BitwiseGameState(byte maxRows, byte maxColumns, ulong fieldsSideA, ulong fieldsSideB, ulong columnStoneSums)
        {
            // make sure the arguments are valid
            if (maxRows <= 0) { throw new ArgumentException("Invalid argument! Max rows needs to be greater than 0."); }
            if (maxColumns <= 0) { throw new ArgumentException("Invalid argument! Max columns needs to be greater than 0."); }
            if (maxColumns * maxRows > 64) { throw new ArgumentException("Invalid argument! Too many fields, rows multiplied by columns must not be greater than 64!"); }

            _maxRows = maxRows;
            _maxColumns = maxColumns;
            _fieldsSideA = fieldsSideA;
            _fieldsSideB = fieldsSideB;
            _columnStoneSums = columnStoneSums;
        }

        #region Helpers

        private static Tuple<ulong, ulong> convertFields(GameSide[] fields)
        {
            // initialize fields with zeros
            ulong fieldsSideA = 0x0000000000000000ul;
            ulong fieldsSideB = 0x0000000000000000ul;

            // loop through all fields
            for (int i = 0; i < fields.Length; i++)
            {
                // get field at pos
                var field = fields[i];

                // flip the bit at the given position according to the field's side
                fieldsSideA ^= ((ulong)(field == GameSide.SideA ? 1 : 0)) << i;
                fieldsSideB ^= ((ulong)(field == GameSide.SideB ? 1 : 0)) << i;
            }

            // return converted fields triple
            return new Tuple<ulong, ulong>(fieldsSideA, fieldsSideB);
        }

        private static ulong convertColumnStoneSums(int[] columnStoneSums)
        {
            // initialize sums with zeros
            ulong convSums = 0;

            for (int i = 0; i < columnStoneSums.Length; i++)
            {
                // shift existing content by 7 and apply the next sum
                ulong sum = (ulong)columnStoneSums[i];
                convSums |= (convSums << BITS_PER_COLUMN_SUM) | sum;
            }

            // return 7-bit sequences compressed as a single ulong
            return convSums;
        }

        #endregion Helpers

        private const byte BITS_PER_COLUMN_SUM = 7; // max sum is 64 for a board with 1 column and 64 rows
        private const ulong COLUMN_SUM_MASK = 0b_1111111; //0x7F

        private readonly byte _maxRows;
        private readonly byte _maxColumns;
        // private readonly ulong _fieldsNone;
        private readonly ulong _fieldsSideA;
        private readonly ulong _fieldsSideB;
        private readonly ulong _columnStoneSums;

        public GameSide[] Fields => getFields();
        public int[] ColumnStoneSums => getColumnStoneSums();
        public int MaxColumns => _maxColumns;
        public int MaxRows => _maxRows;

        public ulong FieldsSideA => _fieldsSideA;
        public ulong FieldsSideB => _fieldsSideB;

        public IGameState ApplyAction(GameAction action)
        {
            // make sure the action is valid
            if (action.ActingSide == GameSide.None) { throw new ArgumentException("Invalid arguments! Side must either be StoneA or StoneB!"); }
            if (action.ColumnIndex >= MaxColumns) { throw new IndexOutOfRangeException("Invalid arguments! Column index is out of range!"); }
            if (action.ActingSide != getActingSide()) { 
                throw new ArgumentException("Invalid arguments! Opposing side is supposed to draw!"); }
            if (getColumnStoneSumAt((int)action.ColumnIndex) >= MaxRows) { 
                throw new ArgumentException("Invalid arguments! Column is already entirely occupied! Please try another one."); }

            ulong fieldsSideA = _fieldsSideA;
            ulong fieldsSideB = _fieldsSideB;

            // if side A is drawing -> update side A
            if (action.ActingSide == GameSide.SideA) { fieldsSideA = applyBitwiseAction(fieldsSideA, (int)action.ColumnIndex); }
            // otherwise if side B is drawing -> update side B
            else { fieldsSideB = applyBitwiseAction(fieldsSideB, (int)action.ColumnIndex); }

            // update column stone sums for the affected column
            ulong columnStoneSums = incremetColumnStoneSum((int)action.ColumnIndex);

            // use the copy constructor to create a bitwise game state instance with the next state's features
            return new BitwiseGameState(_maxRows, _maxColumns, fieldsSideA, fieldsSideB, columnStoneSums);
        }

        public IEnumerable<GameAction> GetPossibleActions()
        {
            var actions = new List<GameAction>();
            var side = getActingSide();

            for (int column = 0; column < MaxColumns; column++)
            {
                // if the column is still available
                if (getColumnStoneSumAt(column) < MaxRows)
                {
                    actions.Add(new GameAction((uint)column, side));
                }
            }

            return actions;
        }

        public bool IsConnectN(int n)
        {
            // make sure that at least one player has put enough stones to achieve a win condition
            bool canSideAWin = setBitsCount(_fieldsSideA) >= n;
            bool canSideBWin = setBitsCount(_fieldsSideB) >= n;

            // check if any of the win conditions applies
            return (canSideAWin && (
                       isRowConnectN(n, _fieldsSideA)
                    || isColumnConnectN(n, _fieldsSideA)
                    || isDiagonalConnectN(n, _fieldsSideA)))
                || (canSideBWin && (
                       isRowConnectN(n, _fieldsSideB)
                    || isColumnConnectN(n, _fieldsSideB)
                    || isDiagonalConnectN(n, _fieldsSideB)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isRowConnectN(int n, ulong bitboard)
        {
            // make sure that any n-row fits into the board (otherwise abort)
            if (n > MaxColumns) { return false; }

            // load row mask from cache
            ulong mask = getRowMask(MaxColumns, n);

            // loop through all rows
            for (int rowOffset = 0; rowOffset < MaxRows; rowOffset++)
            {
                // loop through all column offsets
                for (int colOffset = 0; colOffset < MaxColumns - n + 1; colOffset++)
                {
                    // check for connect-n row
                    ulong shiftedBitboard = bitboard >> (rowOffset * MaxColumns + colOffset);
                    if ((shiftedBitboard & mask) == mask) { return true; }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isColumnConnectN(int n, ulong bitboard)
        {
            // make sure that any n-column fits into the board (otherwise abort)
            if (n > MaxRows) { return false; }

            // load column mask from cache
            ulong mask = getColMask(MaxColumns, n);

            // loop through all columns
            for (int colOffset = 0; colOffset < MaxColumns; colOffset++)
            {
                // loop through all row offsets
                for (int rowOffset = 0; rowOffset < MaxRows - n + 1; rowOffset++)
                {
                    // check for connect-n column
                    ulong shiftedBitboard = bitboard >> (rowOffset * MaxColumns + colOffset);
                    if ((shiftedBitboard & mask) == mask) { return true; }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isDiagonalConnectN(int n, ulong bitboard)
        {
            // make sure that any n-diagonal fits into the board (otherwise abort)
            if (n > Math.Min(MaxColumns, MaxRows)) { return false; }

            // load diagonal masks from cache
            ulong maskBltr = getDiagMask(MaxColumns, n);
            ulong maskTlbr = getRevDiagMask(MaxColumns, n);

            // loop through all row offsets
            for (int rowOffset = 0; rowOffset < MaxRows - n + 1; rowOffset++)
            {
                // loop through all column offsets
                for (int colOffset = 0; colOffset < MaxColumns - n + 1; colOffset++)
                {
                    // check for left-to-right connect-n diagonal
                    ulong shiftedBitboard = bitboard >> (rowOffset * MaxColumns + colOffset);
                    if ((shiftedBitboard & maskBltr) == maskBltr) { return true; }
                    if ((shiftedBitboard & maskTlbr) == maskTlbr) { return true; }
                }
            }

            return false;
        }

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GameSide[] getFields()
        {
            // initialize empty fields array
            var fields = new GameSide[_maxRows * _maxColumns];

            // apply the stones to the fields
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = getFieldAt(i);
            }

            return fields;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GameSide getFieldAt(int pos)
        {
            ulong mask = 1ul << pos;
            bool isSideA = (_fieldsSideA & mask) > 0;
            bool isSideB = (_fieldsSideB & mask) > 0;
            return isSideA ? GameSide.SideA : (isSideB ? GameSide.SideB : GameSide.None);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int[] getColumnStoneSums()
        {
            var columnsStoneSums = new int[MaxColumns];

            for (int column = 0; column < MaxColumns; column++)
            {
                columnsStoneSums[column] = calculateColumnStoneSumAt(column);
            }

            return columnsStoneSums;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int calculateColumnStoneSumAt(int column)
        {
            int sum = 0;

            for (int row = 0; row < MaxRows; row++)
            {
                var field = getFieldAt(row * MaxColumns + column);
                if (field == GameSide.None) { break; }
                sum++;
            }

            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong applyBitwiseAction(ulong bitboard, int column)
        {
            // determine the field's bit index
            int row = getColumnStoneSumAt(column);
            int pos = row * MaxColumns + column;

            // flip the bit at the field's position
            bitboard ^= (1ul << pos);
            return bitboard;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong incremetColumnStoneSum(int column)
        {
            int sum = getColumnStoneSumAt(column);
            return setColumnStoneSumAt(column, sum + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int getColumnStoneSumAt(int column)
        {
            int shiftBits = (MaxColumns - column - 1) * BITS_PER_COLUMN_SUM;
            int sum = (int)((_columnStoneSums >> shiftBits) & COLUMN_SUM_MASK);
            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong setColumnStoneSumAt(int column, int newSum)
        {
            int shiftBits = (MaxColumns - column - 1) * BITS_PER_COLUMN_SUM;
            ulong mask = COLUMN_SUM_MASK << shiftBits;
            ulong columnStoneSums = (ulong)((_columnStoneSums & ~mask) | ((ulong)newSum << shiftBits));
            return columnStoneSums;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GameSide getActingSide()
        {
            return (setBitsCount(_fieldsSideA) == setBitsCount(_fieldsSideB) ? GameSide.SideA : GameSide.SideB);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int setBitsCount(ulong n)
        { 
            // use efficient bitwise operation
            #if (NET5_0 || NETCOREAPP3_1 || NETCOREAPP3_0)
                return BitOperations.PopCount(n);

            // use fallback operation for unsupported .NET frameworks
            #else
                int count = 0;

                while (n > 0)
                {
                    count += (int)(n & 1);
                    n >>= 1;
                }

                return count;
            #endif
        }

        #endregion Helpers

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
            // use the lower 16 field bits of both players
            // encode the bits like in Base64 hashing but a lot more efficient
            return (int)(addZeroBitSpaces((uint)_fieldsSideA) | (addZeroBitSpaces((uint)_fieldsSideB) << 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint addZeroBitSpaces(uint input)
        {
            // snippet source: https://stackoverflow.com/a/4600182

            uint tmp = (input ^ (input >> 8)) & 0x0000ff00;
            input ^= (tmp ^ (tmp << 8));
            tmp = (input ^ (input >> 4)) & 0x00f000f0;
            input ^= (tmp ^ (tmp << 4));
            tmp = (input ^ (input >> 2)) & 0x0c0c0c0c;
            input ^= (tmp ^ (tmp << 2));
            tmp = (input ^ (input >> 1)) & 0x22222222;
            uint output = input ^ (tmp ^ (tmp << 1));

            return output;
        }

        public bool Equals(IGameState other)
        {
            // use more efficient comparison for bitwise game states whenever possible
            if (other.GetType() == typeof(BitwiseGameState)) { return this.Equals((BitwiseGameState)other); }

            // compare fields, column stone sums and max columns / rows
            // perform cost efficient comparisons first to save computation power
            return this.MaxColumns == other.MaxColumns && this.MaxRows == other.MaxRows
                && this.ColumnStoneSums.SequenceEqual(other.ColumnStoneSums)
                && this.Fields.SequenceEqual(other.Fields);
        }

        public bool Equals(BitwiseGameState other)
        {
            // very efficient comparison for bitwise states
            return other.MaxRows == this.MaxRows
                && other.MaxColumns == this.MaxColumns
                && other.FieldsSideA == this.FieldsSideA
                && other.FieldsSideB == this.FieldsSideB;
            // info: no comparison for column stone sums required as they 
            //       have to be the same when all stones are at the same positions
        }

        public bool Equals(string base64Hash)
        {
            try
            {
                // try to interpret the string as Base64 game state hash
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