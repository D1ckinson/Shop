using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

namespace Магазин
{
    class Program
    {
        static void Main()
        {
            Random random = new Random();

            List<Product> products = new List<Product>();

            string[] productsNames = { "Сыр", "Морковь", "Хлеб", "Помидор" };

            int minProductsPrice = 10;
            int maxProductsPrice = 100;

            int playerMoney = 2000;
            int productsQuantity = 5;

            foreach (string name in productsNames)
            {
                int price = random.Next(minProductsPrice, maxProductsPrice + 1);

                for (int i = 0; i < productsQuantity; i++)
                {
                    products.Add(new Product(price, name));
                }
            }

            Seller seller = new Seller(products);

            Player player = new Player(playerMoney);

            ActionBuilder actionBuilder = new ActionBuilder(seller, player);

            Menu menu = new Menu(actionBuilder.MenuActions, "Выход");

            Console.CursorVisible = false;

            menu.Work();
        }
    }

    abstract class Human
    {
        protected int _money;

        protected List<Product> _products = new List<Product>();

        public Product[] ShowProducts() => _products.ToArray();
    }

    class Player : Human
    {
        public Player(int money)
        {
            _money = money;
        }

        public bool IsMoneyEnough(int moneyToGive)
        {
            if (moneyToGive > _money)
                return false;

            return true;
        }

        public void Buy(Product product)
        {
            _money -= product.Price;

            _products.Add(product);
        }
    }

    class Seller : Human
    {
        public Seller(List<Product> products)
        {
            _products = products;
            _money = 0;
        }

        public bool TryGetProduct(string productName, out Product product)
        {
            product = _products.FirstOrDefault(searchProduct => searchProduct.Name.ToLower() == productName.ToLower());

            if (product == null)
                return false;

            return true;
        }

        public void Sell(Product product)
        {
            _money += product.Price;

            _products.Remove(product);
        }
    }

    class Product
    {
        public Product(int price, string name)
        {
            Price = price;
            Name = name;
        }

        public int Price { get; private set; }
        public string Name { get; private set; }

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
    class ActionBuilder
    {
        public Dictionary<string, Action> MenuActions = new Dictionary<string, Action>();

        private Seller _seller;
        private Player _player;

        public ActionBuilder(Seller seller, Player player)
        {
            _seller = seller;
            _player = player;

            MenuActions.Add("Показать продукты продавца", ShowSellerProducts);
            MenuActions.Add("Посмотреть свои продукты", ShowPlayerProducts);
            MenuActions.Add("Купить продукт", ByuProduct);
        }

        private void ShowSellerProducts()
        {
            Console.Clear();

            string[] productsInfo = GetProductsInfo(_seller.ShowProducts());

            Renderer.DrawProductsInfo(productsInfo);
        }

        private void ShowPlayerProducts()
        {
            Console.Clear();

            string[] productsInfo = GetProductsInfo(_player.ShowProducts());

            Renderer.DrawProductsInfo(productsInfo);
        }

        private string[] GetProductsInfo(Product[] products)
        {
            string[] productsInfo = new string[products.Length];

            for (int i = 0; i < products.Length; i++)
                productsInfo[i] = products[i].GiveInfo();

            return productsInfo;
        }

        private void ByuProduct()
        {
            Console.CursorVisible = true;

            Renderer.DrawText("Введите название товара для покупки: ", Renderer.RequestCursorPositionY);

            Console.CursorVisible = false;

            string productName = Console.ReadLine();

            if (_seller.TryGetProduct(productName, out Product product) == false)
            {
                Renderer.DrawText("Такого продукта нет.", Renderer.ResponseCursorPositionY);

                return;
            }

            if (_player.IsMoneyEnough(product.Price) == false)
            {
                Renderer.DrawText($"У вас недостаточно денег.", Renderer.ResponseCursorPositionY);

                return;
            }

            _player.Buy(product);

            _seller.Sell(product);

            Renderer.DrawText($"Вы купили {product.Name}.", Renderer.ResponseCursorPositionY);
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
            Console.SetCursorPosition(0, ProductsCursorPositionY);

            foreach (string product in text)
                Console.WriteLine(product);
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