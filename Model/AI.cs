﻿using System;
using System.IO;
using BlazorConnect4.Model;
using System.Threading.Tasks;

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
        float WinningMove = 1.0F;
        float LosingMove = -1.0F;
        float InvalidMove = -0.1F;

        float Memory = 0.0F;

        public QAgent()
        {

        }

        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            return temp;
        }

        public override int SelectMove(Cell[,] grid)
        {
            return 0;
        }          
 
        public bool isValid(int col, Cell[,] grid)
        {
            // Checks top row with input col to see if cell is Blank, which means a player can place there.
            bool isValid = grid[col, 0].Color == CellColor.Blank;
            return isValid;
        }
    }
}
