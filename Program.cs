using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Test
{
    class Program
    {
        static string productsPath = Path.Combine(AppContext.BaseDirectory, "cosmetics.csv");
        static string usersPath = Path.Combine(AppContext.BaseDirectory, "users.csv");


        static bool AdminLog = false;
        const string AdminCode = "ADMIN";


        static void Main()
        {
            InitFiles();
            Hellow();
            ShowMainMenu();
        }
        static void InitFiles()
        {
            if (!File.Exists(productsPath))
                File.WriteAllText(productsPath, "Id,Name,Price\n");

            if (!File.Exists(usersPath))
                File.WriteAllText(usersPath, "Id,Email,PasswordHash\n");
        }
        public static void Hellow()
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("====== Ласкаво просимо до Cosmetics_store ======");
            Console.WriteLine("-------------------------------------------------");
            Console.ResetColor();
        }
        public static void Return()
        {
            Console.WriteLine("для продовження роботи натиснiть будь яку клавiшу");
            Console.ReadKey();
        }

        static void ShowMainMenu()
        {
            while (true)
            {
                Console.WriteLine("Введіть пункт меню");
                Console.WriteLine("\n1. Каталог");
                Console.WriteLine("2. Оформити замовлення");
                Console.WriteLine("3. Вхід адміністратора");
                Console.WriteLine("4. Вихід");

                if (!int.TryParse(Console.ReadLine(), out int c)) continue;

                if (c == 1) ShowProuductsMenu();
                else if (c == 2) ShowOrderMenu();
                else if (c == 3) Admin();
                else if (c == 4) break;

                Return();
            }
        }
        static void Admin()
        {
            Console.WriteLine("\n1. Реєстрація\n2. Вхід");
            if (!int.TryParse(Console.ReadLine(), out int c)) return;

            if (c == 1) Register();
            else if (c == 2) Login();
        }

        static void Register()
        {
            Console.WriteLine("Email:");
            string email = Console.ReadLine().Trim();

            if (ReadUsers().Any(u => u.Email == email))
            {
                Console.WriteLine("Email вже існує");
                return;
            }

            Console.WriteLine("Пароль:");
            string pass = Console.ReadLine();

            int id = GenerateId(usersPath);
            string hash = Hash(pass);

            File.AppendAllText(usersPath, $"{id},{email},{hash}\n");
            Console.WriteLine("Користувача зареєстровано");
        }
         static void Login()
        {
            Console.WriteLine("Email:");
            string email = Console.ReadLine().Trim();

            Console.WriteLine("Пароль:");
            string pass = Console.ReadLine();

            string hash = Hash(pass);

            foreach (var u in ReadUsers())
            {
                if (u.Email == email && u.PasswordHash == hash)
                {
                    Console.WriteLine("Введіть код адміністратора:");
                    string code = Console.ReadLine();

                    if (code != AdminCode)
                    {
                        Console.WriteLine("Невірний код доступу адміністратора");
                        return;
                    }

                    AdminLog = true;
                    Console.WriteLine("Вхід виконано успішно");
                    ShowAdminMenu();
                    return;
                }
            }

            Console.WriteLine("Невірний email або пароль");
        }
        static void ShowAdminMenu()
        {
            while (AdminLog)
            {
                Console.WriteLine("\n=== АДМІН МЕНЮ ===");
                Console.WriteLine("1. Додати товар");
                Console.WriteLine("2. Видалити товар");
                Console.WriteLine("3. Редагувати товар");
                Console.WriteLine("4. Статистика");
                Console.WriteLine("5. Вихід");

                if (!int.TryParse(Console.ReadLine(), out int c)) continue;

                if (c == 1) AddProduct();
                else if (c == 2) DeleteProduct();
                else if (c == 3) EditProduct();
                else if (c == 4) ShowStatistics();
                else if (c == 5) AdminLog = false;

                Return();
            }
        }
        static List<Product> ReadProducts()
        {
            var list = new List<Product>();
            var lines = File.ReadAllLines(productsPath);

            if (lines.Length == 0 || lines[0] != "Id,Name,Price")
                return list;

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var p = line.Split(',');
                if (p.Length != 3) continue;

                if (int.TryParse(p[0], out int id) &&
                    double.TryParse(p[2], out double price))
                {
                    list.Add(new Product { Id = id, Name = p[1], Price = price });
                }
            }
            return list;
        }

        static void AddProduct()
        {
            Console.WriteLine("Назва:");
            string name = Console.ReadLine();

            Console.WriteLine("Ціна:");
            if (!double.TryParse(Console.ReadLine(), out double price)) return;

            int id = GenerateId(productsPath);
            File.AppendAllText(productsPath, $"{id},{name},{price}\n");
        }

        static void DeleteProduct()
        {
            Console.WriteLine("ID:");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;

            var result = new List<string> { "Id,Name,Price" };

            foreach (var p in ReadProducts())
                if (p.Id != id)
                    result.Add($"{p.Id},{p.Name},{p.Price}");

            File.WriteAllLines(productsPath, result);
        }

        static void EditProduct()
        {
            Console.WriteLine("ID:");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;

            Console.WriteLine("Нова назва:");
            string name = Console.ReadLine();

            Console.WriteLine("Нова ціна:");
            if (!double.TryParse(Console.ReadLine(), out double price)) return;

            var result = new List<string> { "Id,Name,Price" };

            foreach (var p in ReadProducts())
            {
                if (p.Id == id)
                    result.Add($"{id},{name},{price}");
                else
                    result.Add($"{p.Id},{p.Name},{p.Price}");
            }

            File.WriteAllLines(productsPath, result);
        }
        static void ShowStatistics()
        {
            var products = ReadProducts();
            if (products.Count == 0) return;

            Console.WriteLine($"Кількість: {products.Count}");
            Console.WriteLine($"Мінімальна ціна: {products.Min(p => p.Price)}");
            Console.WriteLine($"Максимальна ціна: {products.Max(p => p.Price)}");
            Console.WriteLine($"Сума: {products.Sum(p => p.Price)}");
            Console.WriteLine($"Середнє: {products.Average(p => p.Price)}");
        }
        static void ShowProuductsMenu()
        {
            PrintTable(ReadProducts());
        }

        static void ShowOrderMenu()
        {
            var products = ReadProducts();
            PrintTable(products);

            double total = 0;

            while (true)
            {
                Console.WriteLine("ID (0 — кінець):");
                if (!int.TryParse(Console.ReadLine(), out int id)) continue;
                if (id == 0) break;

                var p = products.FirstOrDefault(x => x.Id == id);
                if (p.Id == 0) continue;

                Console.WriteLine("Кількість:");
                if (!int.TryParse(Console.ReadLine(), out int q)) continue;

                total += p.Price * q;
            }

            Console.WriteLine($"Сума: {total}");
        }
        static int GenerateId(string path)
        {
            int max = 0;
            foreach (var line in File.ReadAllLines(path).Skip(1))
            {
                var p = line.Split(',');
                if (p.Length > 0 && int.TryParse(p[0], out int id))
                    if (id > max) max = id;
            }
            return max + 1;
        }

        static string Hash(string input)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        static List<User> ReadUsers()
        {
            var list = new List<User>();
            foreach (var l in File.ReadAllLines(usersPath).Skip(1))
            {
                var p = l.Split(',');
                if (p.Length == 3)
                    list.Add(new User { Id = int.Parse(p[0]), Email = p[1], PasswordHash = p[2] });
            }
            return list;
        }

        static void PrintTable(List<Product> products)
        {
            Console.WriteLine("{0,-5} {1,-20} {2,10}", "ID", "Name", "Price");
            Console.WriteLine(new string('-', 40));
            foreach (var p in products)
                Console.WriteLine("{0,-5} {1,-20} {2,10:F2}", p.Id, p.Name, p.Price);
        }
        struct Product { public int Id; public string Name; public double Price; }
        struct User { public int Id; public string Email; public string PasswordHash; }
    }
} 