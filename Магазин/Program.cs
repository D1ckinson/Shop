using System;
using System.Collections.Generic;
using System.Linq;

namespace Магазин
{
    class Program
    {
        static void Main()
        {
            int buyersMoney = 5000;

            Buyer buyer = new Buyer(buyersMoney);
            Shop shop = new Shop();

            shop.Work(buyer);
        }
    }

    class Shop
    {
        private Seller _seller;

        public Shop() =>
            _seller = new Seller(GiveProducts());

        public void Work(Buyer buyer)
        {
            const string BuyCommand = "1";
            const string ShowSellerProductsCommand = "2";
            const string ShowBuyerProductsCommand = "3";
            const string ShowBuyerMoneyCommand = "4";
            const string ExitCommand = "5";

            bool isWork = true;

            while (isWork)
            {
                Console.WriteLine(
                    $"\nКоманды меню:\n" +
                    $"{BuyCommand} - Купить продукты\n" +
                    $"{ShowSellerProductsCommand} - Показать продукты продавца\n" +
                    $"{ShowBuyerProductsCommand} - Показать продукты покупателя\n" +
                    $"{ShowBuyerMoneyCommand} - Показать количество денег покупателя\n" +
                    $"{ExitCommand} - Выйти\n");

                string input = UserUtils.ReadString("Введите команду: ");
                Console.Clear();

                switch (input)
                {
                    case BuyCommand:
                        Sell(buyer);
                        break;

                    case ShowSellerProductsCommand:
                        _seller.ShowProducts();
                        break;

                    case ShowBuyerProductsCommand:
                        buyer.ShowProducts();
                        break;

                    case ShowBuyerMoneyCommand:
                        buyer.ShowMoney();
                        break;

                    case ExitCommand:
                        isWork = false;
                        break;

                    default:
                        Console.WriteLine("Такой команды нет");
                        break;
                }
            }
        }

        private void Sell(Buyer buyer)
        {
            string productName = UserUtils.ReadString("Введите название товара для покупки: ");

            if (_seller.TryGetProduct(productName, out Product product) == false)
            {
                Console.WriteLine("Такого продукта нет.");

                return;
            }

            if (buyer.IsMoneyEnough(product.Price) == false)
            {
                Console.WriteLine($"У вас недостаточно денег.");

                return;
            }

            buyer.Buy(product);
            _seller.Sell(product);
            Console.WriteLine($"Вы купили {product.Name}.");
        }

        private List<Product> GiveProducts()
        {
            List<Product> products = new List<Product>();
            List<string> productsNames = new List<string> { "Сыр", "Морковь", "Хлеб", "Помидор", "Картошка" };

            int minPrice = 50;
            int maxPrice = 400;
            int minQuantity = 2;
            int maxQuantity = 10;

            foreach (string name in productsNames)
            {
                int quantity = UserUtils.GenerateRandomValue(minQuantity, maxQuantity + 1);
                int price = UserUtils.GenerateRandomValue(minPrice, maxPrice + 1);
                Product product = new Product(name, price);

                for (int i = 0; i < quantity; i++)
                {
                    products.Add(product);
                }
            }

            return products;
        }
    }

    abstract class Human
    {
        protected int Money;
        protected List<Product> Products = new List<Product>();

        public void ShowProducts()
        {
            if (Products.Count == 0)
            {
                Console.WriteLine("У меня нет продуктов");

                return;
            }

            IEnumerable<Product> uniqueProducts = Products.Distinct();

            for (int i = 0; i < uniqueProducts.Count(); i++)
            {
                Product product = uniqueProducts.ElementAt(i);
                int quantity = Products.Count(productToCount => product == productToCount);

                Console.WriteLine($"{product.Name}, стоит {product.Price}. Количество - {quantity}");
            }
        }
    }

    class Buyer : Human
    {
        public Buyer(int money) =>
            Money = money;

        public bool IsMoneyEnough(int moneyToGive) =>
            moneyToGive <= Money;

        public void Buy(Product product)
        {
            Money -= product.Price;
            Products.Add(product);
        }

        public void ShowMoney() =>
            Console.WriteLine("В моем кошельке:" + Money);
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
            product = Products.FirstOrDefault(productToSearch => productToSearch.Name.ToLower() == productName.ToLower());

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
        public Product(string name, int price)
        {
            Name = name;
            Price = price;
        }

        public int Price { get; private set; }
        public string Name { get; private set; }
    }

    static class UserUtils
    {
        private static Random s_random = new Random();

        public static string ReadString(string text)
        {
            Console.Write(text);

            return Console.ReadLine();
        }

        public static int GenerateRandomValue(int min, int max) =>
            s_random.Next(min, max);
    }
}