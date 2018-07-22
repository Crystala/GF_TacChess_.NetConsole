using System;
using System.Collections.Generic;

namespace TacChess {
#region Program
    class Program {
        static void Main(string[] args) {
            // Initialize Console
            Console.CursorVisible = false;
            Console.SetWindowSize(80, 25);

            // Set Game
            Game.Instance.board = new Board(5, 7);
            Faction grf = new Faction(ConsoleColor.Green, "Griffon", 0);
            Faction sv = new Faction(ConsoleColor.Red, "Sangvis Ferri");
            Game.Instance.factions.Add(grf);
            Game.Instance.factions.Add(sv);
            Game.Instance.board[2, 0].type = Grid.GridType.HQ;
            Game.Instance.board[2, 0].faction = sv;
            Game.Instance.board[2, 6].type = Grid.GridType.HQ;
            Game.Instance.board[2, 6].faction = grf;
            Game.Instance.board[0, 0].supply = 1;
            Game.Instance.board[4, 0].supply = 1;
            Game.Instance.board[1, 2].supply = 1;
            Game.Instance.board[3, 2].supply = 1;
            Game.Instance.board[0, 3].supply = 1;
            Game.Instance.board[2, 3].supply = 1;
            Game.Instance.board[4, 3].supply = 1;
            Game.Instance.board[1, 4].supply = 1;
            Game.Instance.board[3, 4].supply = 1;
            Game.Instance.board[0, 6].supply = 1;
            Game.Instance.board[4, 6].supply = 1;
            Game.Instance.board[1, 0].doll = new Doll(Doll.DollType.Scout, sv);
            Game.Instance.board[3, 0].doll = new Doll(Doll.DollType.Assault, sv);
            Game.Instance.board[1, 6].doll = new Doll(Doll.DollType.Assault, grf);
            Game.Instance.board[3, 6].doll = new Doll(Doll.DollType.Scout, grf);
            Game.Instance.inputHandler = IH_1.Instance;

            Game.Instance.Draw();
            while (Game.Instance.victorious < 0) {
                Game.Instance.inputHandler.HandleInput(Console.ReadKey(true).Key);
                Game.Instance.Draw();
            }

            Game.Instance.OnGameEnd();
        }
    }
    #endregion

#region Data
    public class Board {
        public Board(int sizeX, int sizeY) {
            grids = new Grid[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++) {
                for (int j = 0; j < sizeY; j++) {
                    grids[i, j] = new Grid() { type = Grid.GridType.Normal, doll = null, supply = 0 };
                }
            }
        }

        public Grid this[int posX, int posY] {
            get {
                if (0 <= posX && posX < grids.GetLength(0) && 0 <= posY && posY < grids.GetLength(1)) {
                    return grids[posX, posY];
                }
                else {
                    return null;
                }
            }
        }

        public Grid[,] grids;

        public void Draw() {
            for (int i = 0; i < grids.GetLength(0); i++) {
                for (int j = 0; j < grids.GetLength(1); j++) {
                    Utils.WriteAt("+---+", (Game.GRID_CONSOLE_WIDTH - 1) * i, (Game.GRID_CONSOLE_HEIGHT - 1) * j);
                    Utils.WriteAt("|", (Game.GRID_CONSOLE_WIDTH - 1) * i, (Game.GRID_CONSOLE_HEIGHT - 1) * j + 1);
                    Utils.WriteAt("|", (Game.GRID_CONSOLE_WIDTH - 1) * (i + 1), (Game.GRID_CONSOLE_HEIGHT - 1) * j + 1);
                    Utils.WriteAt("|", (Game.GRID_CONSOLE_WIDTH - 1) * i, (Game.GRID_CONSOLE_HEIGHT - 1) * j + 2);
                    Utils.WriteAt("|", (Game.GRID_CONSOLE_WIDTH - 1) * (i + 1), (Game.GRID_CONSOLE_HEIGHT - 1) * j + 2);
                    Utils.WriteAt("+---+", (Game.GRID_CONSOLE_WIDTH - 1) * i, (Game.GRID_CONSOLE_HEIGHT - 1) * (j + 1));
                    grids[i, j].DrawContent((Game.GRID_CONSOLE_WIDTH - 1) * i + 1, (Game.GRID_CONSOLE_HEIGHT - 1) * j + 1);
                }
            }
        }
    }

    public class Grid {
        public int supply = 0;
        public Doll doll = null;
        public GridType type = GridType.Normal;
        public Faction faction;

        public enum GridType {
            Normal = 0,
            HQ = 1
        }

        public void DrawContent(int posX, int posY) {
            if (doll != null) {
                Utils.WriteAt(Doll.displayName[(int)doll.type], posX, posY, doll.faction.color);
            }
            else {
                Utils.WriteAt("  ", posX, posY);
            }

            if (supply > 0) {
                Utils.WriteAt("s", posX + Game.GRID_CONSOLE_WIDTH - 3, posY);
            }
            else {
                Utils.WriteAt(" ", posX + Game.GRID_CONSOLE_WIDTH - 3, posY);
            }

            if (type == GridType.HQ) {
                Utils.WriteAt("HQ", posX, posY + 1);
            }
            else {
                Utils.WriteAt("  ", posX, posY + 1);
            }
        }
    }

    public class Doll {
        public Doll(DollType type, Faction faction) {
            this.type = type;
            this.faction = faction;
        }

        public DollType type;
        public Faction faction;

        public enum DollType {
            Scout = 0,
            Assault = 1,
            Elite = 2,
            Bomb = 3
        }

        public static string[] displayName = new string[4]
            //{ "S", "A", "E", "B" };
            { "侦", "突", "锐", "砰" };
        public static int[] deployCost = new int[4] { 1, 1, 2, 1 };
    }

    public class Faction {
        public Faction(ConsoleColor color, string name, int supply = 0) {
            this.color = color;
            this.name = name;
            this.supply = supply;
        }

        public ConsoleColor color;
        public string name;
        public int supply;

        public static bool operator ==(Faction a, Faction b) {
            if (a as object == null || b as object == null) {
                return false;
            }
            return a.name == b.name;
        }
        public static bool operator !=(Faction a, Faction b) {
            if (a as object == null || b as object == null) {
                return false;
            }
            return a.name != b.name;
        }

        public override bool Equals(object obj) {
            var faction = obj as Faction;
            return faction != null &&
                   color == faction.color &&
                   name == faction.name &&
                   supply == faction.supply;
        }

        public override int GetHashCode() {
            var hashCode = -1388644900;
            hashCode = hashCode * -1521134295 + color.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + supply.GetHashCode();
            return hashCode;
        }
    }
    #endregion

    #region Game Management & Logic
    public class Game {
        public const int GRID_CONSOLE_WIDTH = 5;
        public const int GRID_CONSOLE_HEIGHT = 4;
        public const int EXIT_VICTORIOUS = 999;


        public ConsoleColor CURSOR_COLOR {
            get {
                if (victorious < 0) {
                    return factions[currentFactionIndex].color;
                }
                else {
                    return ConsoleColor.White;
                }
            }
        }

        private static Game instance;
        public static Game Instance {
            get {
                if (instance == null) {
                    instance = new Game();
                }
                return instance;
            }
        }

        public List<Faction> factions = new List<Faction>();
        public int victorious = -1;
        public Board board;
        public int currentFactionIndex = 0;
        public IInputHandler inputHandler;
        private int cursorPosX = 0, cursurPosY = 0;

        public Faction CurrentFaction {
            get {
                return factions[currentFactionIndex];
            }
        }

        public int GetFactionIndex(Faction faction) {
            for (int i = 0; i < factions.Count; i++) {
                if (factions[i] == faction) {
                    return i;
                }
            }
            return -1;
        }

        public bool DeployDoll(Doll doll) {
            return DeployDoll(doll, factions[currentFactionIndex]);
        }

        public bool DeployDoll(Doll doll, Faction faction) {
            Grid hq = null;
            for (int i = 0; i < board.grids.GetLength(0); i++) {
                for (int j = 0; j < board.grids.GetLength(1); j++) {
                    if (board[i, j].type == Grid.GridType.HQ && board[i, j].faction == factions[currentFactionIndex]) {
                        hq = board[i, j];
                    }
                }
            }
            if (hq == null || hq.doll != null || factions[currentFactionIndex].supply < Doll.deployCost[(int)doll.type]) {
                return false;
            }

            factions[currentFactionIndex].supply -= Doll.deployCost[(int)doll.type];
            hq.doll = doll;
            return true;
        }

        public bool MoveDoll(int deltaX, int deltaY) {
            return MoveDoll(cursorPosX, cursurPosY, deltaX, deltaY);
        }

        public bool MoveDoll(int posX, int posY, int deltaX, int deltaY) {
            if (board[posX, posY] == null || board[posX, posY].doll == null || board[posX, posY].doll.faction != CurrentFaction) {
                return false;
            }
            int x = posX + deltaX, y = posY + deltaY;
            if (board[x, y] == null) {
                return false;
            }
            if (board[x, y].doll != null && board[x, y].doll.faction == board[posX, posY].faction) {
                return false;
            }

            if (board[x, y].doll != null) {
                board[x, y].doll = Battle(board[posX, posY].doll, board[x, y].doll);
            }
            else {
                board[x, y].doll = board[posX, posY].doll;
            }
            board[posX, posY].doll = null;

            if (board[x, y].doll.type == Doll.DollType.Scout) {
                board[x, y].doll.faction.supply += board[x, y].supply;
                board[x, y].supply = 0;
            }

            if (board[x, y].type == Grid.GridType.HQ && board[x, y].faction != board[x, y].doll.faction) {
                victorious = GetFactionIndex(board[x, y].doll.faction);
            }

            return true;
        }

        public Doll Battle(Doll d1, Doll d2) {
            if (d1.type == Doll.DollType.Bomb || d2.type == Doll.DollType.Bomb) {
                return null;
            }
            else {
                if ((int)d1.type > (int)d2.type) {
                    return d1;
                }
                else if ((int)d1.type < (int)d2.type) {
                    return d2;
                }
                else {
                    return null;
                }
            }
        }

        public void EndCurrentTurn() {
            currentFactionIndex++;
            if (currentFactionIndex >= factions.Count) {
                currentFactionIndex = 0;
            }
        }

        public void MoveCursor(int deltaX, int deltaY) {
            int x = cursorPosX + deltaX;
            int y = cursurPosY + deltaY;
            if (board[x, y] != null) {
                cursorPosX = x;
                cursurPosY = y;
            }
        }

        private void DrawCursor(ConsoleColor color) {
            Utils.WriteAt("+---+", (Game.GRID_CONSOLE_WIDTH - 1) * cursorPosX, (Game.GRID_CONSOLE_HEIGHT - 1) * cursurPosY, color);
            Utils.WriteAt("|", (Game.GRID_CONSOLE_WIDTH - 1) * cursorPosX, (Game.GRID_CONSOLE_HEIGHT - 1) * cursurPosY + 1, color);
            Utils.WriteAt("|", (Game.GRID_CONSOLE_WIDTH - 1) * (cursorPosX + 1), (Game.GRID_CONSOLE_HEIGHT - 1) * cursurPosY + 1, color);
            Utils.WriteAt("|", (Game.GRID_CONSOLE_WIDTH - 1) * cursorPosX, (Game.GRID_CONSOLE_HEIGHT - 1) * cursurPosY + 2, color);
            Utils.WriteAt("|", (Game.GRID_CONSOLE_WIDTH - 1) * (cursorPosX + 1), (Game.GRID_CONSOLE_HEIGHT - 1) * cursurPosY + 2, color);
            Utils.WriteAt("+---+", (Game.GRID_CONSOLE_WIDTH - 1) * cursorPosX, (Game.GRID_CONSOLE_HEIGHT - 1) * (cursurPosY + 1), color);
        }
        
        public void Draw() {
            board.Draw();

            // Draw Info
            for (int i = 0; i < factions.Count; i++) {
                Utils.WriteAt(factions[i].name, board.grids.GetLength(0) * (GRID_CONSOLE_WIDTH - 1) + 5, 1 + 3 * (factions.Count - 1 - i), factions[i].color);
                Utils.WriteAt("Supply: " + factions[i].supply, board.grids.GetLength(0) * (GRID_CONSOLE_WIDTH - 1) + 5, 1 + 3 * (factions.Count - 1 - i) + 1, factions[i].color);
            }

            // Draw Cursor
            DrawCursor(CURSOR_COLOR);
        }

        public void OnGameEnd() {
            if (victorious == EXIT_VICTORIOUS) {
                return;
            }
            
            int y = Console.WindowHeight / 2;
            Utils.WriteAtMiddle(factions[victorious].name + " Wins the Game!", y, factions[victorious].color);
            Utils.WriteAtMiddle("Press Any Key to Exit...", y + 1, factions[victorious].color);
            Console.ReadKey(true);
        }
    }
    #endregion

#region Input Handlers
    public interface IInputHandler {
        void HandleInput(ConsoleKey key);
    }

    public class IH_1 : IInputHandler {
        private static IH_1 instance;
        public static IH_1 Instance {
            get {
                if (instance == null) {
                    instance = new IH_1();
                }
                return instance;
            }
        }

        public void HandleInput(ConsoleKey key) {
            switch (key) {
                case ConsoleKey.UpArrow:
                    Game.Instance.MoveCursor(0, -1);
                    break;

                case ConsoleKey.DownArrow:
                    Game.Instance.MoveCursor(0, 1);
                    break;

                case ConsoleKey.LeftArrow:
                    Game.Instance.MoveCursor(-1, 0);
                    break;

                case ConsoleKey.RightArrow:
                    Game.Instance.MoveCursor(1, 0);
                    break;

                case ConsoleKey.W:
                    if (Game.Instance.MoveDoll(0, -1)) {
                        Game.Instance.EndCurrentTurn();
                    }
                    break;

                case ConsoleKey.A:
                    if (Game.Instance.MoveDoll(-1, 0)) {
                        Game.Instance.EndCurrentTurn();
                    }
                    break;

                case ConsoleKey.S:
                    if (Game.Instance.MoveDoll(0, 1)) {
                        Game.Instance.EndCurrentTurn();
                    }
                    break;

                case ConsoleKey.D:
                    if (Game.Instance.MoveDoll(1, 0)) {
                        Game.Instance.EndCurrentTurn();
                    }
                    break;

                case ConsoleKey.NumPad1:
                    Game.Instance.DeployDoll(new Doll(Doll.DollType.Scout, Game.Instance.CurrentFaction));
                    break;

                case ConsoleKey.NumPad2:
                    Game.Instance.DeployDoll(new Doll(Doll.DollType.Assault, Game.Instance.CurrentFaction));
                    break;

                case ConsoleKey.NumPad3:
                    Game.Instance.DeployDoll(new Doll(Doll.DollType.Elite, Game.Instance.CurrentFaction));
                    break;

                case ConsoleKey.NumPad4:
                    Game.Instance.DeployDoll(new Doll(Doll.DollType.Bomb, Game.Instance.CurrentFaction));
                    break;

                case ConsoleKey.Enter:
                    Game.Instance.EndCurrentTurn();
                    break;

                case ConsoleKey.Escape:
                    Game.Instance.victorious = Game.EXIT_VICTORIOUS;
                    break;
            }
        }
    }
    #endregion

    public class Utils {
        public static void WriteAt(string s, int x, int y, ConsoleColor foreColor = ConsoleColor.White, ConsoleColor backColor = ConsoleColor.Black) {
            try {
                Console.ForegroundColor = foreColor;
                Console.BackgroundColor = backColor;
                Console.SetCursorPosition(x, y);
                Console.Write(s);
            }
            catch (ArgumentOutOfRangeException e) {
                Console.Clear();
                Console.WriteLine(e.Message);
            }
        }

        public static void WriteAtMiddle(string s, int y, ConsoleColor foreColor = ConsoleColor.White, ConsoleColor backColor = ConsoleColor.Black) {
            WriteAt(s, (Console.WindowWidth - s.Length) / 2, y, foreColor, backColor);
        }
    }
}
