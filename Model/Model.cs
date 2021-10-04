using System;
using System.IO;
using BlazorConnect4.AIModels;

namespace BlazorConnect4.Model
{
    public enum CellColor
    {
        Red,
        Yellow,
        Blank
    }
    

    public class Cell
    {
        public CellColor Color {get; set;}

        public Cell(CellColor color)
        {
            Color = color;
        }
         
    }

    public class GameBoard
    {
        public Cell[,] Grid { get; set; }

        public GameBoard()
        {
            Grid = new Cell[7, 6];

            //Populate the Board with blank pieces
            for (int i = 0; i <= 6; i++)
            {
                for (int j = 0; j <= 5; j++)
                {
                    Grid[i, j] = new Cell(CellColor.Blank);
                }
            }
        }

        // Creates a string key for the current grid state
        public static String GetHashStringCode(Cell[,] grid)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    stringBuilder.Append(grid[col, row].Color);
                }
            }
            return stringBuilder.ToString();
        }
    }


    public class GameEngine
    {
        public GameBoard Board { get; set; }
        public CellColor Player { get; set;}
        public bool active;
        public String message;
        private AI ai;


        public GameEngine()
        {
            Reset("Human");
        }



        // Reset the game and creats the opponent.
        // TODO change the code so new RL agents are created.
        public void Reset(String playAgainst)
        {
            Board = new GameBoard();
            Player = CellColor.Red;
            active = true;
            message = "Starting new game";

            if (playAgainst == "Human")
            {
                ai = null;
            }
            else if (playAgainst == "Random")
            {
                if (File.Exists("Data/Random.bin"))
                {
                    ai = RandomAI.ConstructFromFile("Data/Random.bin");
                }
                else
                {
                    ai = new RandomAI();
                    ai.ToFile("Data/Random.bin");
                }
                
            }
            else if (playAgainst == "Q1")
            {
                if (File.Exists("Data/Q1Yellow.bin"))
                {
                    ai = QAgent.ConstructFromFile("Data/Q1Yellow.bin");
                }
                else
                {
                    ai = new QAgent(Player);
                    ai.ToFile("Data/Q1Yellow.bin");
                }
            }
            else if (playAgainst == "Q2")
            {
                if (File.Exists("Data/Q2.bin"))
                {
                    ai = QAgent.ConstructFromFile("Data/Q2.bin");
                }
                else
                {
                    ai = new QAgent(Player);
                    ai.ToFile("Data/Q2.bin");
                }
            }
            else if (playAgainst == "Q3")
            {
                if (File.Exists("Data/Q3.bin"))
                {
                    ai = QAgent.ConstructFromFile("Data/Q3.bin");
                }
                else
                {
                    ai = new QAgent(Player);
                    ai.ToFile("Data/Q3.bin");
                }
            }

        }




        private bool IsValid(int col)
        {
            return Board.Grid[col, 0].Color == CellColor.Blank;
        }

        public static bool IsValid(Cell[,] board, int col)
        {
            return board[col, 0].Color == CellColor.Blank;
        }


        public bool IsDraw()
        {
            for (int i = 0; i < 7; i++)
            {
                if (Board.Grid[i,0].Color == CellColor.Blank)
                {
                    return false;
                }
            }
            return true;
        }


        public bool IsWin(int col, int row)
        {
            bool win = false;
            int score = 0;
            

            // Check down
            if (row < 3)
            {
                for (int i = row; i <= row + 3; i++)
                {
                    if (Board.Grid[col,i].Color == Player)
                    {
                        score++;
                    }
                }
                win = score == 4;
                score = 0;
            }

            // Check horizontal

            int left = Math.Max(col - 3, 0);

            for (int i = left; i <= col; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i+j <= 6 && Board.Grid[i+j,row].Color == Player)
                    {
                        score++;
                    }
                }
                win = win || score == 4;
                score = 0;
            }

            // Check left down diagonal

            int colpos;
            int rowpos;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = col - i + j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        Board.Grid[colpos,rowpos].Color == Player)
                    {
                        score++;
                    }
                }

                win = win || score == 4;
                score = 0;
            }

            // Check left up diagonal

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = col + i - j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        Board.Grid[colpos, rowpos].Color == Player)
                    {
                        score++;
                    }
                }
                
                win = win || score == 4;
                score = 0;
            }

            return win;
        }

        public static bool IsWin(Cell[,] grid, CellColor player, int col, int row)
        {
            bool win = false;
            int score = 0;


            // Check down
            if (row < 3)
            {
                for (int i = row; i <= row + 3; i++)
                {
                    if (grid[col, i].Color == player)
                    {
                        score++;
                    }
                }
                win = score == 4;
                score = 0;
            }

            // Check horizontal

            int left = Math.Max(col - 3, 0);

            for (int i = left; i <= col; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i + j <= 6 && grid[i + j, row].Color == player)
                    {
                        score++;
                    }
                }
                win = win || score == 4;
                score = 0;
            }

            // Check left down diagonal

            int colpos;
            int rowpos;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = col - i + j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        grid[colpos, rowpos].Color == player)
                    {
                        score++;
                    }
                }

                win = win || score == 4;
                score = 0;
            }

            // Check left up diagonal

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = col + i - j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        grid[colpos, rowpos].Color == player)
                    {
                        score++;
                    }
                }

                win = win || score == 4;
                score = 0;
            }

            return win;
        }


        public bool Play(int col)
        {
            if (IsValid(col) && active){

                for (int i = 5; i >= 0; i--)
                {
                    if (Board.Grid[col, i].Color == CellColor.Blank)
                    {
                        Board.Grid[col, i].Color = Player;

                        if (IsWin(col,i))
                        {
                            message = Player.ToString() + " Wins";
                            active = false;
                            return true;
                        }

                        if (IsDraw())
                        {
                            message = "Draw";
                            active = false;
                            return true;
                        }
                        break;
                    }
                }
                return PlayNext();
            }

            return false;
        }


        private bool PlayNext()
        {

            if (Player == CellColor.Red)
            {
                Player = CellColor.Yellow;
            }
            else
            {
                Player = CellColor.Red;
            }

            if (ai != null && Player == CellColor.Yellow)
            {
                int move = ai.SelectMove(Board.Grid);

                while (! IsValid(move))
                {
                    move = ai.SelectMove(Board.Grid);
                }

                return Play(move);
            }

            return false;
        }
    }

    public class TrainingGameEngine
    {
        public GameBoard Board { get; set; }
        public CellColor Player { get; set; }

        public TrainingGameEngine()
        {
            Board = new GameBoard();
            Player = CellColor.Red;
        }



        // Reset the game and creats the opponent.
        public void Reset()
        {
            Board = new GameBoard();
            Player = CellColor.Red;
        }

        public bool IsValid(Cell[,] board, int col)
        {
            return board[col, 0].Color == CellColor.Blank;
        }


        public bool IsDraw()
        {
            for (int i = 0; i < 7; i++)
            {
                if (Board.Grid[i, 0].Color == CellColor.Blank)
                {
                    return false;
                }
            }
            return true;
        }

        public CellColor SwapPlayer(CellColor player)
        {
            if (player == CellColor.Red)
            {
                return CellColor.Yellow;
            }
            else
            {
                return CellColor.Red;
            }
        }

        public void Play(int col, CellColor player)
        {
            for (int i = 5; i >= 0; i--)
            {
                if (Board.Grid[col, i].Color == CellColor.Blank)
                {
                    Board.Grid[col, i].Color = player;

                    break;
                }
            }
        }

        public bool IsWin(CellColor player, int col)
        {
            bool win = false;
            int score = 0;
            int row = 5;

            while (row >= 0)
            {
                if (Board.Grid[col, row].Color == CellColor.Blank)
                {
                    row += 1;
                    break;
                }
                if (row == 0)
                {
                    break;
                }
                row--;
            }

            // Check down
            if (row < 3)
            {
                for (int i = row; i <= row + 3; i++)
                {
                    if (Board.Grid[col, i].Color == player)
                    {
                        score++;
                    }
                }
                win = score == 4;
                score = 0;
            }
                
            // Check horizontal

            int left = Math.Max(col - 3, 0);

            for (int i = left; i <= col; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i + j <= 6 && Board.Grid[i + j, row].Color == player)
                    {
                        score++;
                    }
                }
                win = win || score == 4;
                score = 0;
            }

            // Check left down diagonal

            int colpos;
            int rowpos;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = col - i + j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        Board.Grid[colpos, rowpos].Color == player)
                    {
                        score++;
                    }
                }

                win = win || score == 4;
                score = 0;
            }

            // Check left up diagonal

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = col + i - j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        Board.Grid[colpos, rowpos].Color == player)
                    {
                        score++;
                    }
                }

                win = win || score == 4;
                score = 0;
            }
            return win;
        }
    }
}
