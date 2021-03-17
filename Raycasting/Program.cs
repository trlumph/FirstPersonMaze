using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    class Program
    {
        private const int screenWidth = 150;
        private const int screenHeight = 90;

        private const int mapWidth = 32;
        private const int mapHeight = 32;

        private const double fov = Math.PI / 4;
        private const double depth = 32;

        private static double playerX = 5.0;
        private static double playerY = 5.0;
        private static double playerA = 0;

        private static readonly StringBuilder Map = new StringBuilder();

        private static readonly char[] Screen = new char[screenWidth * screenHeight];

        static async Task Main(string[] args)
        {
            Console.SetWindowSize(screenWidth, screenHeight + 1);
            Console.SetBufferSize(screenWidth, screenHeight + 1);

            InitMap();

            var dateTimeFrom = DateTime.Now;

            while (true)
            {
                var dateTimeTo = DateTime.Now;
                double elapsedTime = (dateTimeTo - dateTimeFrom).TotalSeconds;
                dateTimeFrom = DateTime.Now;

                if (Console.KeyAvailable)
                {
                    InitMap();

                    var consoleKey = Console.ReadKey(true).Key;
                    double movementMultiplier = 10.0;
                    double rotationMultiplier = 2.0;

                    switch (consoleKey)
                    {
                        case ConsoleKey.A:
                            playerA += rotationMultiplier * elapsedTime;
                            break;
                        case ConsoleKey.D:
                            playerA -= rotationMultiplier * elapsedTime;
                            break;
                        case ConsoleKey.W:
                        {
                            playerX += Math.Sin(playerA) * movementMultiplier * elapsedTime;
                            playerY += Math.Cos(playerA) * movementMultiplier * elapsedTime;

                            char s = Map[(int)playerY * mapWidth + (int)playerX];
                            if (s == '#' || s == 'B')
                            {
                                playerX -= Math.Sin(playerA) * movementMultiplier * elapsedTime;
                                playerY -= Math.Cos(playerA) * movementMultiplier * elapsedTime;
                            }
                            else if (s == 'F')
                                return;
                            break;
                        }
                        case ConsoleKey.S:
                        {
                            playerX -= Math.Sin(playerA) * movementMultiplier * elapsedTime;
                            playerY -= Math.Cos(playerA) * movementMultiplier * elapsedTime;

                            char s = Map[(int)playerY * mapWidth + (int)playerX];
                            if (s == '#' || s == 'B')
                            {
                                playerX += Math.Sin(playerA) * movementMultiplier * elapsedTime;
                                playerY += Math.Cos(playerA) * movementMultiplier * elapsedTime;
                            }
                            else if (s == 'F')
                                return;
                            break;
                        }
                        case ConsoleKey.Escape:
                            return;
                    }
                }

                //Ray casting
                var rayCastingTasks = new List<Task<Dictionary<int, char>>>();

                for (int x = 0; x < screenWidth; x++)
                {
                    int x1 = x;
                    rayCastingTasks.Add(Task.Run(() => CastRay(x1)));
                }

                var rays = await Task.WhenAll(rayCastingTasks);

                foreach (var dictionary in rays)
                {
                    foreach(int key in dictionary.Keys)
                    {
                        Screen[key] = dictionary[key];
                    }
                }

                //stats
                char[] stats = $"X: {playerX}, Y: {playerY}, A: {playerA}, FPS: {(int)(1 / elapsedTime)}".ToCharArray();
                stats.CopyTo(Screen, 0);

                //map
                for (int x = 0; x < mapWidth; x++)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        Screen[(y + 1) * screenWidth + x] = Map[y * mapWidth + x];
                    }
                }

                //player
                Screen[(int)(playerY + 1) * screenWidth + (int)playerX] = 'P';

                Console.CursorVisible = false;
                Console.SetCursorPosition(0, 0);
                Console.Write(Screen);
            }
        }

        private static Dictionary<int, char> CastRay(int x)
        {
            var result = new Dictionary<int, char>();

            double rayAngle = playerA + fov / 2 - x * fov / screenWidth;

            double rayX = Math.Sin(rayAngle);
            double rayY = Math.Cos(rayAngle);

            double distanceToWall = 0;
            GameObject hitObject = GameObject.None;
            bool isBound = false;
            double wallSize = 1;

            while (hitObject == GameObject.None && distanceToWall < depth)
            {
                distanceToWall += 0.1;

                int testX = (int)(playerX + rayX * distanceToWall);
                int testY = (int)(playerY + rayY * distanceToWall);

                if (testX < 0 || testX >= depth + playerX || testY < 0 || testY >= depth + playerY)
                {
                    hitObject = GameObject.Wall;
                    distanceToWall = depth;
                }
                else
                {
                    char testCell = Map[testY * mapWidth + testX];
                    if (testCell == '#' || testCell == 'B' || testCell == 'F')
                    {
                        wallSize = GetWallSize(testCell);
                        hitObject = GetHitObject(testCell);
                        isBound = IsBound(rayX, rayY, distanceToWall, testX, testY);
                    }
                    else
                    {
                        Map[testY * mapWidth + testX] = '*';
                    }
                }
            }

            int ceiling = (int)(screenHeight / 2d - screenHeight * fov / distanceToWall);
            int floor = screenHeight - ceiling;

            ceiling += (int)(screenHeight - screenHeight * wallSize);

            char wallShade;

            if (isBound)
                wallShade = '|';
            else if (hitObject == GameObject.Finish)
                wallShade = 'F';
            else if (distanceToWall < depth / 4d)
                wallShade = '\u2588';
            else if (distanceToWall < depth / 3d)
                wallShade = '\u2593';
            else if (distanceToWall < depth / 2d)
                wallShade = '\u2592';
            else if (distanceToWall < depth)
                wallShade = '\u2591';
            else
                wallShade = ' ';

            for (int y = 0; y < screenHeight; y++)
            {
                if (y <= ceiling)
                {
                    result[y * screenWidth + x] = ' ';
                }
                else if (y > ceiling && y <= floor)
                {
                    result[y * screenWidth + x] = wallShade;
                }
                else
                {
                    char floorShade;

                    double b = 1 - (y - screenHeight / 2d) / (screenHeight / 2d);

                    if (b < 0.25)
                        floorShade = '#';
                    else if (b < 0.5)
                        floorShade = 'x';
                    else if (b < 0.75)
                        floorShade = '-';
                    else if (b < 0.9)
                        floorShade = '.';
                    else
                        floorShade = ' ';
                    result[y * screenWidth + x] = floorShade;
                }
            }
            return result;
        }

        private static GameObject GetHitObject(char testCell)
        {
            GameObject gameObject = GameObject.None;
            switch (testCell)
            {
                case '#':
                    gameObject = GameObject.Wall;
                    break;
                case 'B':
                    gameObject = GameObject.BigWall;
                    break;
                case 'F':
                    gameObject = GameObject.Finish;
                    break;
            }
            return gameObject;
        }

        private static double GetWallSize(char testCell)
        {
            if (testCell == '#') return 1.0;
            if (testCell == 'B') return 1.2;
            if (testCell == 'F') return 1.0;
            throw new NotImplementedException();
        }

        private static bool IsBound(double rayX, double rayY, double distanceToWall, int testX, int testY)
        {
            var boundsVectorList = new List<(double module, double cos)>();

            for (int tx = 0; tx < 2; tx++)
            {
                for (int ty = 0; ty < 2; ty++)
                {
                    double vx = (double)testX + tx - playerX;
                    double vy = (double)testY + ty - playerY;

                    double vectorModule = Math.Sqrt(vx * vx + vy * vy);
                    double cosAngle = rayX * vx / vectorModule + rayY * vy / vectorModule;

                    boundsVectorList.Add((vectorModule, cosAngle));
                }
            }

            boundsVectorList = boundsVectorList.OrderBy(v => v.module).ToList();

            double boundAngle = 0.03d / distanceToWall;

            if (Math.Acos(boundsVectorList[0].cos) < boundAngle ||
                Math.Acos(boundsVectorList[1].cos) < boundAngle)
                return true;
            return false;
        }

        private static void InitMap()
        {
            Map.Clear();
            Map.Append("################################");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#.......................##.....#");
            Map.Append("#.....................#####....#");
            Map.Append("##.....................##......#");
            Map.Append("###.............BBB#...........#");
            Map.Append("#########........##............#");
            Map.Append("#....##........................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#.....#########................#");
            Map.Append("#.....#####....................#");
            Map.Append("#.....###......................#");
            Map.Append("#.....#........................#");
            Map.Append("#...................#..........#");
            Map.Append("#...................#..........#");
            Map.Append("#...................#..........#");
            Map.Append("#......##############..........#");
            Map.Append("#...................#..........#");
            Map.Append("#...................#......F...#");
            Map.Append("#...................#..........#");
            Map.Append("#...................#..........#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("################################");
        }
    }
}