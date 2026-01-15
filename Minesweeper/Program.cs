using Minesweeper;

var minsesweeperEngine = new MinesweeperEngine();

var startDate = DateTime.UtcNow;

var preset = 2;

while (true)
{
    Console.WriteLine($"{new string('-', (minsesweeperEngine.GridX / 2) - 3)} САПЁР {new string('-', (minsesweeperEngine.GridX / 2) - 3)}");
    Console.WriteLine();
    minsesweeperEngine.PrintGrid();

    Console.WriteLine();
    Console.WriteLine($"Управление:");
    Console.WriteLine($"{ConsoleKey.E} - копать");
    Console.WriteLine($"{ConsoleKey.Q} - флаг");
    Console.WriteLine($"{ConsoleKey.W} - наверх");
    Console.WriteLine($"{ConsoleKey.S} - вниз");
    Console.WriteLine($"{ConsoleKey.D} - вправо");
    Console.WriteLine($"{ConsoleKey.A} - влево");

    Console.WriteLine();
    Console.WriteLine($"{ConsoleKey.D1} - Легкая сложность");
    Console.WriteLine($"{ConsoleKey.D2} - Средняя сложность");
    Console.WriteLine($"{ConsoleKey.D3} - Сложная сложность");
    Console.WriteLine($"{ConsoleKey.Escape}/{ConsoleKey.X} - выход");
    Console.WriteLine();

    Console.Write("Ввод: ");
    var input = Console
        .ReadKey()
        .Key;

    switch (input)
    {
        case ConsoleKey.E:
            var diff = DateTime.UtcNow.Subtract(startDate);

            if (minsesweeperEngine.Dig())
            {
                if (!minsesweeperEngine.AllWihoutMinesHasDigged())
                    break;

                Console.Clear();
                Console.WriteLine($"Победа! Время: {diff}");
                Console.WriteLine();

                minsesweeperEngine.ShowGrid();
            }
            else
            {
                Console.Clear();
                Console.WriteLine($"Взрыв! Время: {diff}");
                Console.WriteLine();

                minsesweeperEngine.ShowGrid();
            }

            Console.WriteLine();
            Console.Write("Нажмите Enter чтобы продолжить, или введите R чтобы начать заново: ");
            var strInput = Console
                .ReadLine()?
                .ToUpper();

            startDate = DateTime.UtcNow;

            if (strInput == "R")
            {
                switch (preset)
                {
                    case 1:
                        minsesweeperEngine.StartEasyGame();
                        break;

                    case 2:
                        minsesweeperEngine.StartMediumGame();
                        break;

                    case 3:
                        minsesweeperEngine.StartHardGame();
                        break;
                }
                break;
            }
            return;

        case ConsoleKey.Q: 
            minsesweeperEngine.SetFlag();
            break;

        case ConsoleKey.W: 
            minsesweeperEngine.MoveUp();
            break;

        case ConsoleKey.S:
            minsesweeperEngine.MoveDown();
            break;

        case ConsoleKey.D: 
            minsesweeperEngine.MoveRight();
            break;

        case ConsoleKey.A:
            minsesweeperEngine.MoveLeft();
            break;


        case ConsoleKey.D1:
            minsesweeperEngine.StartEasyGame();
            startDate = DateTime.UtcNow;
            preset = 1;
            break;

        case ConsoleKey.D2:
            minsesweeperEngine.StartMediumGame();
            startDate = DateTime.UtcNow;
            preset = 2;
            break;

        case ConsoleKey.D3:
            minsesweeperEngine.StartHardGame();
            startDate = DateTime.UtcNow;
            preset = 3;
            break;

        case ConsoleKey.Escape:
        case ConsoleKey.X:
            return;
    }

    Console.Clear();
}