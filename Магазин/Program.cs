using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Магазин
{
    class Program
    {
        static void Main()
        {
            Random random = new Random();

            List<Product> products = new List<Product>();

            string[] productsNames = { "Сыр", "Морковь", "Хлеб", "Помидор" };

            int minProductsPrice = 50;
            int maxProductsPrice = 200;

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

            Menu menu = new Menu(actionBuilder.GiveMenuActions(), "Выход");

            Console.CursorVisible = false;

            menu.Work();
        }
    }

    abstract class Human
    {
        protected int Money;

        protected List<Product> Products = new List<Product>();

        public Product[] GiveProductsArray() => Products.ToArray();
    }

    class Player : Human
    {
        public Player(int money)
        {
            Money = money;
        }

        public bool IsMoneyEnough(int moneyToGive) => moneyToGive <= Money;

        public void Buy(Product product)
        {
            Money -= product.Price;

            Products.Add(product);
        }
    }

    class Seller : Human
    {
        public Seller(List<Product> products)
        {
            Products = products;
            Money = 0;
        }

        public bool TryGetProduct(string productName, out Product product)
        {
            product = Products.FirstOrDefault(searchProduct => searchProduct.Name.ToLower() == productName.ToLower());

            return product != null;
        }

        public void Sell(Product product)
        {
            Money += product.Price;

            Products.Remove(product);
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

        private readonly Dictionary<string, Action> _actions;

        private readonly Renderer _renderer = new Renderer();

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
                _renderer.DrawMenuActions(_actions.Keys.ToArray(), menuIndex);

                _renderer.EraseText(_renderer.RequestCursorPositionY);

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

                if (menuIndex > lastIndex)
                    menuIndex = lastIndex;
                else if (menuIndex < 0)
                    menuIndex = 0;

            } while (_isWork);
        }

        private void Exit() => _isWork = false;

        private void StartAction(int index)
        {
            _renderer.EraseText(_renderer.ResponseCursorPositionY);

            _actions[_actions.Keys.ToArray()[index]].Invoke();
        }
    }

    class ActionBuilder
    {
        private readonly Seller _seller;
        private readonly Player _player;
        private readonly Renderer _renderer = new Renderer();

        public ActionBuilder(Seller seller, Player player)
        {
            _seller = seller;
            _player = player;
        }

        public Dictionary<string, Action> GiveMenuActions()
        {
            Dictionary<string, Action> menuActions = new Dictionary<string, Action>()
            {
                {"Показать продукты продавца", ShowSellerProducts },
                {"Посмотреть свои продукты", ShowPlayerProducts },
                {"Купить продукт", ByuProduct },
            };

            return menuActions;
        }

        private void ShowSellerProducts()
        {
            Console.Clear();

            string[] productsInfo = GetProductsInfo(_seller.GiveProductsArray());

            _renderer.DrawProductsInfo(productsInfo);
        }

        private void ShowPlayerProducts()
        {
            Console.Clear();

            string[] productsInfo = GetProductsInfo(_player.GiveProductsArray());

            _renderer.DrawProductsInfo(productsInfo);
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

            _renderer.DrawText("Введите название товара для покупки: ", _renderer.RequestCursorPositionY);

            Console.CursorVisible = false;

            string productName = Console.ReadLine();

            if (_seller.TryGetProduct(productName, out Product product) == false)
            {
                _renderer.DrawText("Такого продукта нет.", _renderer.ResponseCursorPositionY);

                return;
            }

            if (_player.IsMoneyEnough(product.Price) == false)
            {
                _renderer.DrawText($"У вас недостаточно денег.", _renderer.ResponseCursorPositionY);

                return;
            }

            _player.Buy(product);

            _seller.Sell(product);

            _renderer.DrawText($"Вы купили {product.Name}.", _renderer.ResponseCursorPositionY);
        }
    }

    class Renderer
    {
        public readonly int ResponseCursorPositionY = 7;
        public readonly int RequestCursorPositionY = 5;
        public readonly int ProductsCursorPositionY = 9;

        private readonly int _spaceLineSize = 100;
        private readonly char _spaceChar = ' ';

        public void DrawMenuActions(string[] text, int selectTextPosition)
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

        public void DrawProductsInfo(IEnumerable<string> text)
        {
            Console.SetCursorPosition(0, ProductsCursorPositionY);

            foreach (string product in text)
                Console.WriteLine(product);
        }

        public void DrawText(string text, int cursorPositionY)
        {
            EraseText(cursorPositionY);

            Console.Write(text);
        }

        public void EraseText(int cursorPositionY)
        {
            Console.SetCursorPosition(0, cursorPositionY);

            Console.Write(new string(_spaceChar, _spaceLineSize));

            Console.CursorLeft = 0;
        }

        private void DrawSelectText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;

            Console.Write(text);

            Console.ResetColor();
        }
    }
}