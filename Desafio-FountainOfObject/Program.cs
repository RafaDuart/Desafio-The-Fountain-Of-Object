using System.Dynamic;


namespace FountainOfObjects
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameFactory.MakeGame().GameRun();
        }



        public static class GameFactory
        {
            public static FountainOFObjects MakeGame()
            {
                while (true)
                {
                    Console.WriteLine("Qual o tamanho da caverna pequeno, medio, grande?");
                    string? input = Console.ReadLine();
                    if (input == "pequeno") return MakeSmallGame();
                    if (input == "medio") return MakeMediumGame();
                    if (input == "grande") return MakeLargeGame();
                    Console.WriteLine("Eu não conheço esse comando");
                }
            }

            public static FountainOFObjects MakeSmallGame()
            {
                Map map = new Map(4, 4);
                Maelstrom maelstrom = new Maelstrom();

                map.SetRoom(0, 0, new Entrance());
                map.SetRoom(0, 2, new Fountain());
                map.SetRoom(2, 1, new Pit());

                return new FountainOFObjects(map);
            }

            public static FountainOFObjects MakeMediumGame()
            {
                Map map = new Map(6, 6);
                map.SetRoom(2, 2, new Entrance());
                map.SetRoom(4, 5, new Fountain());
                return new FountainOFObjects(map);
            }

            public static FountainOFObjects MakeLargeGame()
            {
                Map map = new Map(8, 8);
                map.SetRoom(6, 6, new Entrance());
                map.SetRoom(0, 5, new Fountain());

                return new FountainOFObjects(map);
            }
        }




        public class FountainOFObjects
        {
            public Map Map { get; }
            public Player Player { get; }
            public Maelstrom Maelstrom { get; }


            public FountainOFObjects(Map map)
            {
                Map = map;
                Player = new Player();
                Maelstrom = new Maelstrom();
                SetPlayerLocationAtEntrance(Map, Player);
            }

            void SetPlayerLocationAtEntrance(Map map, Player player)
            {
                for (int row = 0; row < Map.Rows; row++)
                    for (int column = 0; column < Map.Columns; column++)
                        if (Map.GetRoom(row, column) is Entrance)
                        {
                            Player.PlayerLocation = new Location(row, column);
                        }
            }

            void SetMaelstromLocation(int row, int column, Maelstrom maelstrom)
            {
                for (int i = 0; i < Map.Rows; i++)
                    for (int j = 0; column < Map.Columns; j++)
                        if (Map.GetRoom(row, column) is EmptyRoom)
                        {
                            maelstrom.MaelstromLocation = new Location(row, column);
                        }
            }


            public void GameRun()
            {
                PlayerInput playerInput = new PlayerInput();

                while (!HasWon() && !PitDeath())
                {


                    ShowMap();
                    new MaelstromSense().SenseStuff(this);
                    new PitSense().SenseStuff(this);
                    new FountainSense().SenseStuff(this);
                    new EntranceSense().SenseStuff(this);
                    IAction action = playerInput.ChooseAction();
                    action.ExecuteAction(this);


                }
                if (HasWon())
                {
                    Console.WriteLine("A Fountain of Objects foi ativada, você escapou com vida");
                    Console.WriteLine("Você Ganhou!");
                }
                if (PitDeath()) Console.WriteLine("Você morreu!");
            }

            public bool HasWon()
            {

                Room playerRoom = Map.GetRoomAtLocation(Player.PlayerLocation);
                if (playerRoom is not Entrance) return false;
                for (int row = 0; row < Map.Rows; row++)
                    for (int column = 0; column < Map.Columns; column++)
                    {
                        if (Map.GetRoom(row, column) is Fountain fountainRoom)
                            if (fountainRoom.IsOff) return false;

                    }

                return true;

            }

            public bool PitDeath()
            {
                Room playerRoom = Map.GetRoomAtLocation(Player.PlayerLocation);
                if (playerRoom is not Pit) return false;
                return true;
            }



            public void ShowMap()
            {
                for (int row = 0; row < Map.Rows; row++)
                {

                    for (int column = 0; column < Map.Columns; column++)

                        if (Map.GetRoomAtLocation(Player.PlayerLocation) == Map.GetRoom(row, column))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("■");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else if (Map.GetRoom(row, column) is Entrance)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("■");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else if (Map.GetRoom(row, column) is Fountain)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("■");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else if (Map.GetRoom(row, column) is Pit)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("■");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else if (Map.GetRoomAtLocation(Maelstrom.MaelstromLocation) == Map.GetRoom(row, column))
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("■");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }

                        else Console.Write("■");
                    Console.WriteLine();
                }


            }

        }


        public enum Move { Norte, Sul, Leste, Oeste }
        public enum Senses { Ouvir, Cheirar, Tocar }
        enum Action { AtivarFountain }



        internal class PlayerInput
        {
            public IAction ChooseAction()
            {
                do
                {
                    Console.WriteLine("Qual direção você deseja se mover ?");
                    string? input = Console.ReadLine();
                    IAction? chosenAction = input switch
                    {
                        "norte" => new MovementDirection(Move.Norte),
                        "sul" => new MovementDirection(Move.Sul),
                        "oeste" => new MovementDirection(Move.Oeste),
                        "leste" => new MovementDirection(Move.Leste),
                        "ativar" => new EnableFountain(),
                        _ => null
                    };
                    if (chosenAction != null) return chosenAction;

                    Console.WriteLine("Eu desconheço este comando");
                } while (true);

            }
        }

        public interface ISense
        {
            void SenseStuff(FountainOFObjects game);
        }

        public class MaelstromSense : ISense
        {
            public void SenseStuff(FountainOFObjects game)
            {
                if (IsNeighbor(game)) Console.WriteLine("Você ouve o rosnado e o grunido de um Maelstroms próximo.");
            }

            public bool IsNeighbor(FountainOFObjects game)
            {
                Location playerLocation = game.Player.PlayerLocation;
                Location maelstromLocation = game.Maelstrom.MaelstromLocation;
                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column - 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column + 0) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column + 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column - 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column + 0) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column + 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column - 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column + 0) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column + 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                return false;

            }
        }
        public class PitSense : ISense
        {

            public void SenseStuff(FountainOFObjects game)
            {
                if (IsNeighbor(game)) Console.WriteLine("Você sente uma corrente de ar. Há um buraco em uma sala próxima.");
            }

            public bool IsNeighbor(FountainOFObjects game)
            {
                Location playerLocation = game.Player.PlayerLocation;

                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column - 1) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column + 0) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column + 1) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column - 1) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column + 0) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column + 1) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column - 1) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column + 0) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column + 1) is Pit) return true;
                return false;

            }
        }

        public class FountainSense : ISense
        {

            public void SenseStuff(FountainOFObjects game)
            {
                Location playerLocation = game.Player.PlayerLocation;
                Room playerRoom = game.Map.GetRoomAtLocation(playerLocation);

                if (playerRoom is Fountain fountainRoom)
                {
                    if (fountainRoom.IsOn) Console.WriteLine("Você ouve as águas correntes da vindo da Fountain of Objects.Ela foi reativada");

                    else Console.WriteLine("Você ouve o respingar da água nessa sala. A Fountain Of Objects está aqui! Deseja ativa-la?");
                }



            }
        }

        public class EntranceSense : ISense
        {
            public void SenseStuff(FountainOFObjects game)
            {
                Room playerRoom = game.Map.GetRoomAtLocation(game.Player.PlayerLocation);
                if (playerRoom is Entrance) Console.WriteLine("Você está na entrada");
            }

        }

        public interface IAction
        {
            void ExecuteAction(FountainOFObjects game);
        }

        public class EnableFountain : IAction
        {
            public void ExecuteAction(FountainOFObjects game)
            {
                Location playerLocation = game.Player.PlayerLocation;
                Room playerRoom = game.Map.GetRoomAtLocation(playerLocation);
                Fountain? fountainRoom = playerRoom as Fountain;

                if (playerRoom != fountainRoom)
                {
                    Console.WriteLine("Você não pode fazer isso aqui");
                    return;
                }
                fountainRoom.IsOn = true;
            }
        }

        public class MovementDirection : IAction
        {

            private readonly Move _direction;

            public MovementDirection(Move direction)
            {
                _direction = direction;
            }

            public void ExecuteAction(FountainOFObjects game)
            {
                Location current = game.Player.PlayerLocation;
                Location nextLocation = GetDirection(current, _direction);
                if (game.Map.IsOnMap(nextLocation))
                    game.Player.PlayerLocation = nextLocation;
                else Console.WriteLine("Você não pode se mover para essa direção");
            }

            public Location GetDirection(Location PlayerLocation, Move moveDirection)
            {
                return moveDirection switch
                {
                    Move.Norte => new Location(PlayerLocation.Row - 1, PlayerLocation.Column),
                    Move.Sul => new Location(PlayerLocation.Row + 1, PlayerLocation.Column),
                    Move.Leste => new Location(PlayerLocation.Row, PlayerLocation.Column + 1),
                    Move.Oeste => new Location(PlayerLocation.Row, PlayerLocation.Column - 1),

                };


            }

        }


        public record Location(int Row, int Column);

        public class Map
        {
            private Room[,] _rooms = new Room[4, 4];

            public int Rows { get; }
            public int Columns { get; }

            public Map(int numberOfRows, int numberOfColumns)
            {

                Rows = numberOfRows;
                Columns = numberOfColumns;
                _rooms = new Room[Rows, Columns];


                for (int row = 0; row < numberOfRows; row++)
                {
                    for (int column = 0; column < numberOfColumns; column++)
                    {

                        _rooms[row, column] = new EmptyRoom();

                    }

                }

            }


            public bool IsOnMap(Location location)
            {
                if (location.Column < 0) return false;
                if (location.Column >= Columns) return false;
                if (location.Row < 0) return false;
                if (location.Row >= Rows) return false;
                return true;

            }



            public void SetRoom(int row, int column, Room room) => _rooms[row, column] = room;

            public Room GetRoom(int row, int column) => IsOnMap(new Location(row, column)) ? _rooms[row, column] : new OffTheMap();



            public Room GetRoomAtLocation(Location location) => IsOnMap(location) ? _rooms[location.Row, location.Column] : new OffTheMap();

        }


        public interface ICharacter { }

        public class Player : ICharacter
        {
            public Location PlayerLocation { get; set; } = new Location(0, 0);
        }

        public class Maelstrom : ICharacter
        {
            public Location MaelstromLocation { get; set; } = new Location(0, 0);
        }

        public abstract class Room { }
        public class EmptyRoom : Room { }
        public class Entrance : Room { }
        public class Fountain : Room
        {

            public bool IsOn { get; set; }
            public bool IsOff => !IsOn;
        }
        public class Pit : Room { }
        public class OffTheMap : Room { }

    }


}