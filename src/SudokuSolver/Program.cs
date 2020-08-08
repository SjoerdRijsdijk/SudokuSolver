using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver {
    class Program {
        private const int AmountOfRows = 9;
        private const int AmountOfNumbersPerRow = 9;
        private static readonly int[] _possibleValues = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        static void Main(string[] args) {
            while (true) {
                Console.WriteLine("Use prefilled test grid?");

                var grid = new List<List<int>>();

                if (YesOrNo()) {
                    grid = PrefilledGrid();
                }
                else {
                    Console.WriteLine("Enter the first row:");
                    Console.WriteLine("Enter a space separated line of numbers. Each row should have 9 numbers in it.");
                    Console.WriteLine("Example: 0 1 0 0 0 7 0 4 9");

                    var currentRow = 1;

                    while (true) {
                        var processedRow = ProcessRow(currentRow);

                        if (processedRow == null) {
                            continue;
                        }

                        grid.Add(processedRow);

                        if (currentRow == AmountOfRows) {
                            break;
                        }

                        currentRow++;
                    }
                }

                WriteEmpyNewLine(1);

                if (!ValidateGrid(grid)) {
                    Console.WriteLine("The following grid isn't valid:");
                    PrintGrid(grid);
                    WriteEmpyNewLine(1);
                    continue;
                }

                Console.WriteLine("Is the following grid okay?");
                PrintGrid(grid);
                WriteEmpyNewLine(1);

                if (!YesOrNo()) {
                    while (true) {
                        Console.WriteLine("Which row(s) do you want to change? Enter the number of the row(s) separated by a space.");
                        Console.WriteLine("Just press the 'Enter' key if you do want to continue with the previously shown grid.");
                        var rowNumbersInput = Console.ReadLine().Trim();

                        if (string.IsNullOrWhiteSpace(rowNumbersInput)) {
                            break;
                        }

                        var rowNumbers = SplitInput(rowNumbersInput);

                        var (ValidInput, InvalidNumberInput, DuplicateInput) = ProcessInput(rowNumbers, false);

                        if (ShowInvalidNumberInput(InvalidNumberInput) || ShowDuplicateInput(DuplicateInput)) {
                            continue;
                        }

                        foreach (var rowNumber in ValidInput) {
                            while (true) {
                                var processedRow = ProcessRow(rowNumber);

                                if (processedRow == null) {
                                    continue;
                                }

                                grid[rowNumber - 1] = processedRow;
                                break;
                            }
                        }

                        if (!ValidateGrid(grid)) {
                            Console.WriteLine("The following grid isn't valid:");
                            PrintGrid(grid);
                            WriteEmpyNewLine(1);
                            continue;
                        }

                        Console.WriteLine("Is the following grid okay?");
                        PrintGrid(grid);
                        WriteEmpyNewLine(1);

                        if (!YesOrNo()) {
                            continue;
                        }

                        break;
                    }
                }

                WriteEmpyNewLine(2);
                if (Solve(grid, 1, 1)) {
                    Console.WriteLine("Solved the sudoku");
                }
                else {
                    Console.WriteLine("Failed solving the sudoku");
                }

                PrintGrid(grid);
            }
        }

        private static IEnumerable<string> SplitInput(string input) {
            return input.Split(" ").Where(c => !string.IsNullOrWhiteSpace(c));
        }

        private static bool IsValidNumberInput(string input, out int number, bool zeroIsValid = true) {
            var isNumber = int.TryParse(input, out number);
            return isNumber && (zeroIsValid ? number >= 0 : number > 0) && number <= 9;
        }

        private static (List<int> ValidInput, List<string> InvalidNumberInput, List<int> DuplicateInput) ProcessInput(IEnumerable<string> input, bool zeroIsValid = true) {
            var validInput = new List<int>();
            var invalidNumberInput = new List<string>();
            var duplicateInput = new List<int>();
            var duplicateCheck = new HashSet<string>();

            foreach (var inputNumber in input) {
                if (!IsValidNumberInput(inputNumber, out int parsedInputNumber, zeroIsValid)) {
                    invalidNumberInput.Add(inputNumber);
                }
                else {
                    if (parsedInputNumber == 0 || duplicateCheck.Add(inputNumber)) {
                        validInput.Add(parsedInputNumber);
                    }
                    else {
                        validInput.Remove(parsedInputNumber);
                        duplicateInput.Add(parsedInputNumber);
                    }
                }
            }

            return (validInput, invalidNumberInput, duplicateInput);
        }

        /// <summary>
        /// Returns either a list of valid input or null when invalid.
        /// </summary>
        /// <param name="rowNumber">Number of the current row.</param>
        /// <returns></returns>
        public static List<int> ProcessRow(int rowNumber) {
            Console.Write($"Row {rowNumber}: ");
            var rowInput = Console.ReadLine().Trim();

            var inputNumbers = SplitInput(rowInput);
            var numbersEntered = inputNumbers.Count();

            if (numbersEntered != AmountOfNumbersPerRow) {
                Console.WriteLine($"Each row should contain {AmountOfNumbersPerRow} numbers. You have entered {numbersEntered}.");
                return null;
            }

            var (ValidInput, InvalidNumberInput, DuplicateInput) = ProcessInput(inputNumbers);
            var hasInvalidNumberInput = ShowInvalidNumberInput(InvalidNumberInput);
            var hasDuplicateInput = ShowDuplicateInput(DuplicateInput);

            return !hasInvalidNumberInput && !hasDuplicateInput ? ValidInput : null;
        }

        private static bool Solve(List<List<int>> grid, int x, int y) {
            var valid = false;
            var hasNumber = PositionContainsNumber(grid, x, y);
            foreach (var possibleValue in _possibleValues) {
                if (hasNumber || EnterNumber(grid, x, y, possibleValue)) {
                    var nextX = x;
                    var nextY = y;
                    if (x < 9 && y <= 9) {
                        nextX += 1;
                    }
                    else if (y < 9) {
                        nextY += 1;
                        nextX = 1;
                    }
                    else {
                        return true;
                    }

                    valid = Solve(grid, nextX, nextY);
                    if (!valid) {
                        if (!hasNumber) {
                            grid.ElementAt(y - 1)[x - 1] = 0;
                        }

                        if (hasNumber)
                            break;
                    }
                    else {
                        break;
                    }
                }
            }

            return valid;
        }

        private static List<List<int>> PrefilledGrid() {
            return new List<List<int>> {
                new List<int> { 0, 0, 0, 0, 0, 1, 8, 0, 5 },
                new List<int> { 8, 0, 0, 0, 0, 4, 0, 9, 6 },
                new List<int> { 0, 0, 6, 0, 0, 0, 0, 0, 1 },
                new List<int> { 0, 0, 8, 0, 0, 0, 0, 0, 0 },
                new List<int> { 0, 4, 3, 0, 8, 0, 9, 0, 0 },
                new List<int> { 5, 0, 0, 0, 9, 3, 0, 0, 0 },
                new List<int> { 7, 0, 0, 3, 0, 0, 2, 1, 8 },
                new List<int> { 0, 0, 0, 7, 1, 0, 0, 0, 0 },
                new List<int> { 0, 5, 0, 0, 4, 0, 0, 0, 7 },
            };
        }

        private static void PrintGrid(IEnumerable<IEnumerable<int>> values) {
            WriteEmpyNewLine(2);

            for (int i = 0; i < values.Count(); i++) {
                Console.WriteLine(string.Join("   ", values.ElementAt(i)));
                WriteEmpyNewLine(1);
            }
        }

        private static void WriteEmpyNewLine(int amount) {
            for (int i = 0; i < amount; i++) {
                Console.WriteLine();
            }
        }

        private static bool PositionContainsNumber(List<List<int>> grid, int x, int y) {
            var pos = grid.ElementAt(y - 1).ElementAt(x - 1);
            return pos > 0 && pos <= 9;
        }

        /// <summary>
        /// Checks if the entered number can be placed at the given position. Doesn't check if the position already has a number.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool EnterNumber(List<List<int>> grid, int x, int y, int value) {
            var indexY = y - 1;
            var indexX = x - 1;
            var originalValue = grid[y - 1][x - 1];

            // Necessary for validating the grid
            grid[indexY][indexX] = 0;

            if (grid[indexY].Contains(value) || grid.Any(c => c[indexX] == value)) {
                grid[indexY][indexX] = originalValue;
                return false;
            }

            var farthestX = (int)Math.Ceiling((decimal)x / 3) * 3;
            var farthestY = (int)Math.Ceiling((decimal)y / 3) * 3;

            for (int i = farthestY - 3; i < farthestY; i++) {
                var subSet = grid[i].Skip(farthestX - 3).Take(3);

                if (subSet.Contains(value)) {
                    grid[indexY][indexX] = originalValue;
                    return false;
                }
            }

            grid[indexY][indexX] = value;

            return true;
        }

        private static bool YesOrNo() {
            bool? isOkay = null;
            while (true) {
                Console.Write("Y/N: ");
                var answer = Console.ReadLine();

                switch (answer.ToLowerInvariant()) {
                    case "y":
                        isOkay = true;
                        break;
                    case "n":
                        isOkay = false;
                        break;
                    default:
                        break;
                }

                if (isOkay.HasValue) {
                    break;
                }
            }

            return isOkay.Value;
        }

        private static bool ShowInvalidNumberInput(IEnumerable<string> invalidNumberInput) {
            var hasInvalidNumberInput = invalidNumberInput.Any();
            if (hasInvalidNumberInput) {
                Console.WriteLine($"{string.Join(", ", invalidNumberInput)} is/are not (a) valid number(s).");
            }

            return hasInvalidNumberInput;
        }

        private static bool ShowDuplicateInput(IEnumerable<int> duplicateInput) {
            var hasDuplicateNumberInput = duplicateInput.Any();

            if (hasDuplicateNumberInput) {
                Console.WriteLine($"Too many of the following numbers: {string.Join(", ", duplicateInput)}.");
            }

            return hasDuplicateNumberInput;
        }

        private static bool ValidateGrid(List<List<int>> grid) {
            var validGrid = true;

            for (int i = 1; i <= grid.Count; i++) {
                for (int j = 1; j <= grid[i - 1].Count; j++) {
                    var value = grid[i - 1][j - 1];
                    if (value > 0) {
                        validGrid = EnterNumber(grid, j, i, value);
                        if (!validGrid) {
                            return validGrid;
                        }
                    }
                }
            }

            return validGrid;
        }
    }
}
