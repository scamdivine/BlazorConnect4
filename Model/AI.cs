using System;
using System.IO;
using BlazorConnect4.Model;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BlazorConnect4.AIModels
{
    [Serializable]
    public abstract class AI
    {
        // Funktion för att bestämma vilken handling som ska genomföras.
        public abstract int SelectMove(Cell[,] grid);

        // Funktion för att skriva till fil.
        public virtual void ToFile(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, this);
            }
        }

        // Funktion för att att läsa från fil.
        protected static AI FromFile(string fileName)
        {
            AI returnAI;
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                returnAI = (AI)bformatter.Deserialize(stream);
            }
            return returnAI;

        }

    }

     
    [Serializable]
    public class RandomAI : AI
    {
        [NonSerialized] Random generator;

        public RandomAI()
        {
            generator = new Random();
        }
        
        public override int SelectMove(Cell[,] grid)
        {
            return generator.Next(7);
        }

        public static RandomAI ConstructFromFile(string fileName)
        {
            RandomAI temp = (RandomAI)(AI.FromFile(fileName));
            // Eftersom generatorn inte var serialiserad.
            temp.generator = new Random();
            return temp;
        }
    }

    [Serializable]
    public class QAgent : AI
    {
        private CellColor player;

        float WinningMove = 1.0F;
        float LosingMove = -1.0F;
        float InvalidMove = -0.1F;
        
        int numberOfRuns = 0;

        Dictionary<String, double[]> memoryDict = new Dictionary<String, double[]>();

        public QAgent(CellColor player)
        {
            this.player = player;
        }

        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            return temp;
        }

        public override int SelectMove(Cell[,] grid)
        {
            double epsilon = (float)Math.Pow(0.99985, numberOfRuns);

            int move = epsilonCalculation(grid, epsilon);


            return move;
        }

        public int epsilonCalculation(Cell[,] grid, double epsilon)
        {
            Random rand = new Random();
            if (rand.NextDouble() < epsilon)
            {
                int currentState = rand.Next(7);
                while (!GameEngine.IsValid(grid, currentState))
                {
                    currentState = rand.Next(7);
                }
                return currentState;
            }
            else
            {
                int col = 0;
                double colValue = findInMemory(grid, col);
                for (int i = 1; i < 7; i++)
                {
                    double nextColValue = findInMemory(grid, i);
                    if (colValue < nextColValue)
                    {
                        col = i;
                        colValue = nextColValue;
                    }
                }
                return col;
            }
        }

        public double findInMemory(Cell[,] grid, int col)
        {
            String gridKey = GameBoard.GetHashStringCode(grid);
            Random rand = new Random();
            if (!memoryDict.ContainsKey(gridKey))
            {
                double[] moves = { rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), rand.NextDouble() };
                memoryDict.Add(gridKey, moves);
                return moves[0];
            }
            return memoryDict[gridKey][col];
        }

        public void updateMemory(Cell[,] grid, int col, double reward)
        {
            String gridKey = GameBoard.GetHashStringCode(grid);
            Random rand = new Random();
            if (!memoryDict.ContainsKey(gridKey))
            {
                double[] moves = { rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), rand.NextDouble() };
                memoryDict.Add(gridKey, moves);
            }
            memoryDict[gridKey][col] = reward;
        }

        public int getMove(Cell[,] grid, int col)
        {
            Random rand = new Random();

            for(int epoch = 0; epoch < 100; epoch++)
            {
                int currentState = rand.Next(7);

                while (true)
                {
                    if (GameEngine.IsValid(grid, col))
                    {
                        break;
                    }
                }
            }
            int move = 0;
            float epsilon = (float)Math.Pow(0.99985, numberOfRuns);
            int test = numberOfRuns == 1 ? 1 : 0;
            if (test == 1)
                move = rand.Next(7);
            else
                move = 0;

            return move;
        }

        public void Trainer(int epochs, QAgent opponentAI)
        {
            TrainingGameEngine GameEngine = new TrainingGameEngine();

            for (int i = 0; i < epochs; i++)
            {
                GameEngine.Reset();
                Cell[,] grid = GameEngine.Board.Grid;
                CellColor player = GameEngine.Player;
                CellColor opponent = opponentAI.player;
                CellColor playerTurn = CellColor.Red;

                int move = 0;
                int previousMove = move;
                bool gameInProgress = true;
                Console.WriteLine("Game started");

                while (gameInProgress)
                {
                    if (playerTurn == player)
                    {
                        move = SelectMove(grid);
                        previousMove = move;
                    }
                    else
                    {
                        move = opponentAI.SelectMove(grid);
                    }
                    if (!GameEngine.IsValid(grid, move))
                    {
                        if (playerTurn == player)
                        {
                            updateMemory(grid, move, InvalidMove);
                        }
                        continue;
                    }

                    GameEngine.Play(move, playerTurn);

                    for (int j = 0; j < 6; j++)
                    {
                        for (int k = 0; k < 7; k++)
                        {
                            Console.Write(grid[k, j].Color + "\t");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();

                    if (GameEngine.IsWin(playerTurn, move))
                    {
                        updateMemory(grid, playerTurn == player ? move : previousMove, playerTurn == player ? WinningMove : LosingMove);
                        break;
                    }
                    if (GameEngine.IsDraw())
                    {
                        break;
                    }

                    playerTurn = GameEngine.SwapPlayer(playerTurn);
                    //if (GameEngine.IsDraw())
                    //{
                    //    gameInProgress = false;
                    //    Console.WriteLine("IsDraw");
                    //}
                    //else if (GameEngine.IsWin(thisPlayer, colMove))
                    //{
                    //    updateMemory(grid, thisPlayer == player ? colMove : previousMove, thisPlayer == player ? WinningMove : LosingMove);
                    //    gameInProgress = false;
                    //    Console.WriteLine("IsWin");
                    //}
                    //else if (!GameEngine.IsValid(grid, colMove))
                    //{
                    //    updateMemory(grid, colMove, InvalidMove);
                    //    colMove = SelectMove(grid);
                    //    //Console.WriteLine("IsValid");
                    //}
                    //else {
                    //    thisPlayer = GameEngine.SwapPlayer(thisPlayer);
                    //    previousMove = colMove;
                    //    colMove = SelectMove(grid);
                    //    GameEngine.Play(colMove, thisPlayer);
                    //}
                    //Console.WriteLine("Move: " + colMove + " Runs: " + numberOfRuns);
                }
                Console.WriteLine("Game done");
                numberOfRuns++;
            }
        }
    }
}
