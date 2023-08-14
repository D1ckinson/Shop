using System;
using System.Collections.Generic;
using System.Linq;

namespace Магазин
{
    class Program
    {
        static void Main()
        {
            int productsQuantity = 20;

            var seller = new Seller(productsQuantity);
        }
    }

    abstract class Human
    {
        protected int _money;

        protected List<Product> _products = new List<Product>();

        public void ShowProducts() => Renderer.DrawProductsInfo(Logic.GetArrayProductsInfo(_products));
    }

    class Player : Human
    {
        public Player()
        {
            _money = 2000;
        }

        public int GiveMoney(int moneyToGive)
        {
            _money -= moneyToGive;

            return moneyToGive;
        }
    }

    class Seller : Human
    {
        private Dictionary<ProductNames, int> _productsPrice = new Dictionary<ProductNames, int>()
        {
            { ProductNames.Cheese, 150 },
            { ProductNames.Sausage, 200 },
            { ProductNames.Bread, 70 },
            { ProductNames.Rice, 60 }
        };

        public Seller(int productsQuantity)
        {
            _money = 0;

            _products = Logic.CreateProductsList(_productsPrice, productsQuantity);
        }

        public void TakeMoney(int money) => _money += money;

        public void GiveProduct(Product product) => _products.Remove(product);
    }

    enum ProductNames
    {
        Cheese,
        Sausage,
        Bread,
        Rice
    }

    class Product
    {
        public Product(int price, string name)
        {
            Price = price;
            Name = name;
        }

        public int Price { get; protected set; }
        public string Name { get; protected set; }

        public string GiveInfo() => $"{Name}, стоит {Price}.";
    }

    class BoxFactory
    {
        public Box CreateTomato(int quantity, string name)
        {
            Product product = new Product(70, name);

            return new Box(product, quantity);
        }
    }

    class Box
    {
        public Box(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        public int Quantity { get; private set; }
        public Product Product { get; }


        public bool TryGiveOne(out Product product)
        {
            if (Quantity < 0)
            {
                product = null;
                return false;
            }

            Quantity--;

            product = new Product(Product.Price, Product.Name);

            return true;
        }
    }

    class Menu
    {
        private const ConsoleKey UpArrow = ConsoleKey.UpArrow;
        private const ConsoleKey DownArrow = ConsoleKey.DownArrow;
        private const ConsoleKey Enter = ConsoleKey.Enter;

        private Dictionary<string, Action> _actions;

        private bool _isWork;

        public Menu(Dictionary<string, Action> actions, string exitButtonName)
        {
            _actions = actions;

            _actions.Add(exitButtonName, Exit);
        }

        public void Work()
        {
            _isWork = true;

            int menuIndex = 0;
            int lastIndex = _actions.Count - 1;

            do
            {
                Renderer.DrawMenuActions(_actions.Keys.ToArray(), menuIndex);

                Renderer.EraseText(Renderer.RequestCursorPositionY);

                switch (Console.ReadKey().Key)
                {
                    case DownArrow:
                        menuIndex++;
                        break;

                    case UpArrow:
                        menuIndex--;
                        break;

                    case Enter:
                        StartAction(menuIndex);
                        break;
                }

                menuIndex = CheckIndexBorder(menuIndex, lastIndex);

            } while (_isWork);
        }

        private void Exit() => _isWork = false;

        private int CheckIndexBorder(int index, int lastIndex)
        {
            if (index > lastIndex)
                index = lastIndex;
            else if (index < 0)
                index = 0;

            return index;
        }

        private void StartAction(int index)
        {
            Renderer.EraseText(Renderer.ResponseCursorPositionY);

            _actions[_actions.Keys.ToArray()[index]].Invoke();
        }
    }

    static class Logic
    {
        public static Random Random = new Random();

        private static Dictionary<ProductNames, string> _russiansNames = new Dictionary<ProductNames, string>()
        {
            { ProductNames.Cheese, "Сыр" },
            { ProductNames.Sausage, "Колбаса" },
            { ProductNames.Bread, "Хлеб" },
            { ProductNames.Rice, "Рис" },
        };

        public static string CheckRussianName(ProductNames name)
        {
            if (_russiansNames.ContainsKey(name))
                return _russiansNames[name];
            else
                return name.ToString();
        }

        public static int CheckPrice(Dictionary<ProductNames, int> productsPrice, ProductNames name)
        {
            if (productsPrice.ContainsKey(name))
            {
                return productsPrice[name];
            }
            else
            {
                int price = ReadIntInput($"За товар {name} цена не указана, укажите цену: ");

                productsPrice.Add(name, price);

                return price;
            }
        }

        public static string[] GetArrayProductsInfo(List<Product> products)
        {
            string[] productsNames = Enum.GetNames(typeof(ProductNames));

            Dictionary<string, int> productsQuantity = new Dictionary<string, int>();

            foreach (string name in productsNames)
                productsQuantity.Add(name, 0);

            foreach (Product product in products)
                productsQuantity[product.Name] = productsQuantity[product.Name]++;

            string[] productsInfo = new string[products.Count];

            for (int i = 0; i < products.Count; i++)
                productsInfo[i] = products[i].GiveInfo();

            return productsInfo;
        }

        public static List<Product> CreateProductsList(Dictionary<ProductNames, int> productsPrice, int productsCount)
        {
            List<Product> products = new List<Product>();

            int enumLength = Enum.GetNames(typeof(ProductNames)).Length;

            for (int i = 0; i < productsCount; i++)
            {
                ProductNames productName = (ProductNames)Random.Next(enumLength);

                products.Add(new Product(CheckPrice(productsPrice, productName), CheckRussianName(productName)));
            }

            return products;
        }

        private static string ReadStringInput(string text)
        {
            Renderer.DrawText(text, Renderer.RequestCursorPositionY);

            return Console.ReadLine();
        }

        private static int ReadIntInput(string text)
        {
            int number;

            string userInput = ReadStringInput(text);

            while (int.TryParse(userInput, out number) == false)
            {
                Renderer.DrawText("Введите число.", Renderer.ResponseCursorPositionY);

                userInput = ReadStringInput(text);
            }

            Renderer.EraseText(Renderer.ResponseCursorPositionY);
            Renderer.EraseText(Renderer.RequestCursorPositionY);

            return number;
        }
    }

    static class Renderer
    {
        public const int ResponseCursorPositionY = 7;
        public const int RequestCursorPositionY = 5;
        public const int ProductsCursorPositionY = 9;

        private const int SpaceLineSize = 100;
        private const char SpaceChar = ' ';

        public static void DrawMenuActions(string[] text, int selectTextPosition)
        {
            for (int i = 0; i < text.Length; i++)
            {
                Console.SetCursorPosition(0, i);

                if (i == selectTextPosition)
                    DrawSelectText(text[i]);
                else
                    Console.Write(text[i]);
            }
        }

        public static void DrawProductsInfo(IEnumerable<string> text)
        {
            int counter = 0;

            foreach (string product in text)
            {
                Console.SetCursorPosition(0, ProductsCursorPositionY + counter);
                Console.Write(product[counter]);

                counter++;
            }
        }

        public static void EraseColumnText(int counter, int cursorPositionY)
        {
            for (int i = 0; i < counter; i++)
                EraseText(i + cursorPositionY);
        }

        public static void DrawText(string text, int cursorPositionY)
        {
            EraseText(cursorPositionY);

            Console.Write(text);
        }

        public static void EraseText(int cursorPositionY)
        {
            Console.SetCursorPosition(0, cursorPositionY);

            Console.Write(new string(SpaceChar, SpaceLineSize));

            Console.CursorLeft = 0;
        }

        private static void DrawSelectText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;

            Console.Write(text);

            Console.ResetColor();
        }
    }
}