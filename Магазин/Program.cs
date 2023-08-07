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
            _money = 100;
        }

        public int GiveMoney(int moneyToGive)
        {
            //сделать проверку

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
    }

    enum ProductNames
    {
        Cheese,
        Sausage,
        Bread,
        Rice,
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
        private static Dictionary<ProductNames, string> _russiansNames = new Dictionary<ProductNames, string>()
        {
            { ProductNames.Cheese, "Сыр" },
            { ProductNames.Sausage, "Колбаса" },
            { ProductNames.Bread, "Хлеб" },
            { ProductNames.Rice, "Рис" },
        };

        public static Random Random = new Random();

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

            return number;
        }

        public static string[] GetArrayProductsInfo(List<Product> products)
        {
            string[] foodInfo = new string[products.Count];

            for (int i = 0; i < products.Count; i++)
                foodInfo[i] = products[i].GiveInfo();

            return foodInfo;
        }

        public static List<Product> CreateProductsList(Dictionary<ProductNames, int> productsPrice, int productsCount)
        {
            List<Product> products = new List<Product>();

            //string[] enumArray = Enum.GetNames(typeof(ProductNames));
            //int enumQuantity = enumArray.Length;
            //int randomEnum = Random.Next(enumQuantity);
            //ProductNames productName = (ProductNames)randomEnum;

            //ProductNames productName = (ProductNames)Random.Next(Enum.GetNames(typeof(ProductNames)).Length);

            int enumLength = Enum.GetNames(typeof(ProductNames)).Length;

            for (int i = 0; i < productsCount; i++)
            {
                ProductNames productName = (ProductNames)Random.Next(enumLength);

                products.Add(new Product(CheckPrice(productsPrice, productName), CheckRussianName(productName)));
            }

            return products;
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

        public static void DrawProductsInfo(string[] text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                Console.SetCursorPosition(0, ProductsCursorPositionY + i);
                Console.Write(text[i]);
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