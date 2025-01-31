﻿using System;
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
        
        public int numberOfRuns = 0;
        public int winLossRatio = 0;

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

        // Calls functions to select a move
        public override int SelectMove(Cell[,] grid)
        {
            double epsilon = (float)Math.Pow(0.99985, numberOfRuns);
            int move = epsilonCalculation(grid, epsilon);
            return move;
        }

        // Calculates what move to make by using the epsilon value
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

        // Adds the current state if it does not exist in the AIs memory. If it exists it returns a move from the memory
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

        // Updates the AIs memory if a move was invalid, made the AI win or made the AI lose with appropriate values
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

        // The trainer for our Q Learning AI
        public void Trainer(int epochs, AI opponentAI)
        {
            TrainingGameEngine GameEngine = new TrainingGameEngine();
            int sessionWinLossRatio = 0;
            double alpha = 0.75;
            double gamma = 0.5;

            for (int i = 0; i < epochs; i++)
            {
                GameEngine.Reset();
                Cell[,] grid = GameEngine.Board.Grid;
                CellColor player = GameEngine.Player;
                CellColor playerTurn = CellColor.Red;

                int move = 0;
                int previousMove = move;
                while (true)
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

                    if (GameEngine.IsWin(playerTurn, move))
                    {
                        updateMemory(grid, playerTurn == player ? move : previousMove, playerTurn == player ? WinningMove : LosingMove);
                        winLossRatio += playerTurn == player ? 1 : -1;
                        sessionWinLossRatio += playerTurn == player ? 1 : -1;
                        break;
                    }
                    if (GameEngine.IsDraw())
                    {
                        break;
                    }
                    double currentStateAction = findInMemory(grid, move);
                    int updatedQValue = epsilonCalculation(grid, 5);
                    double nextStateAction = findInMemory(grid, updatedQValue);
                    // Q(a,s) <- Q(a,s) + a(reward + gamma * Q(a’,s’) - Q(a,s))
                    updateMemory(grid, move, currentStateAction + alpha * (0 + gamma * nextStateAction - currentStateAction));

                    playerTurn = GameEngine.SwapPlayer(playerTurn);
                }
                numberOfRuns++;
            }
        }
    }
}
