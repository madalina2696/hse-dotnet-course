using NUnit.Framework;
using NUnit.Framework.Legacy;
using TheMiddleman.Entity;

[TestFixture]
public class BusinessLogicTests
{
    private BusinessLogic? businessLogic;
    private Product? product;
    private Trader? trader;

    [SetUp]
    public void Setup()
    {
        businessLogic = new BusinessLogic();
        product = new Product { Name = "Neuer Produkt" };
        trader = new Trader("Max Mustermann", "Mustermann GmbH", 10000);
        businessLogic.GetProducts().Add(product);
    }

    [Test]
    public void Purchase_Successful_ReduceBalanceAndUpdateInventory()
    {
        product!.Availability = 50;
        product.BuyingPrice = 100;
        businessLogic?.Purchase(trader!, product, 10);
        ClassicAssert.AreEqual(9000, trader?.AccountBalance);
        ClassicAssert.IsTrue(trader?.OwnedProducts.ContainsKey(product));
        ClassicAssert.AreEqual(10, trader?.OwnedProducts[product]);
    }

    [Test]
    public void Purchase_InsufficientFunds_ThrowsBalanceException()
    {
        product!.Availability = 50;
        product!.BuyingPrice = 10000;
        Assert.Throws<BalanceException>(() => businessLogic!.Purchase(trader!, product!, 10));
    }

    [Test]
    public void Purchase_InsufficientProductAvailability_ThrowsProductException()
    {
        if (product != null)
        {
            product.Availability = 5;
        }
        Assert.Throws<ProductException>(() => businessLogic!.Purchase(trader!, product!, 10));
    }

    [Test]
    public void Purchase_Successful_UpdateMarketAvailability()
    {
        product!.Availability = 50;
        product.BuyingPrice = 100;
        businessLogic?.Purchase(trader!, product, 10);
        ClassicAssert.AreEqual(40, product.Availability);
    }

    [Test]
    public void Purchase_Successful_UpdateTodaysReport()
    {
        product!.Availability = 50;
        product.BuyingPrice = 100;
        businessLogic?.Purchase(trader!, product, 10);
        ClassicAssert.AreEqual(9000, trader!.AccountBalance);
    }

    [Test]
    public void TakeLoan_Successful_UpdateBalance()
    {
        businessLogic?.TakeLoan(trader!, new Trader.Loan { Amount = 5000, RepaymentAmount = 5150, DueDay = 8 });
        ClassicAssert.AreEqual(15000, trader!.AccountBalance);
    }

    [Test]
    public void RepayLoan_Successful_UpdateBalance()
    {
        businessLogic!.TakeLoan(trader!, new Trader.Loan { Amount = 5000, RepaymentAmount = 5150, DueDay = 6 });
        businessLogic.RepayLoan(trader!);
        ClassicAssert.AreEqual(9850, trader!.AccountBalance);
    }

    [Test]
    public void UpgradeStorageCapacity_Successful()
    {
        var trader = new Trader("Madalina", "Madalina AG.", 10000);
        int initialCapacity = trader.StorageCapacity;
        double initialBalance = trader.AccountBalance;

        businessLogic!.UpgradeStorageCapacity(trader, 10);

        ClassicAssert.AreEqual(initialCapacity + 10, trader.StorageCapacity, "Lagerkapazität sollte erhöht werden.");
        ClassicAssert.AreEqual(initialBalance - (10 * 50), trader.AccountBalance, "Kontostand sollte um Kosten des Upgrades verringert werden.");
    }

    [Test]
    public void UpgradeStorageCapacity_InsufficientFunds()
    {
        var trader = new Trader("Madalina", "Madalina AG.", 200);
        int initialCapacity = trader.StorageCapacity;
        double initialBalance = trader.AccountBalance;

        Assert.Throws<BalanceException>(() => businessLogic!.UpgradeStorageCapacity(trader, 10), "Sollte eine BalanceException werfen, wenn nicht genügend Geld vorhanden ist.");

        ClassicAssert.AreEqual(initialCapacity, trader.StorageCapacity, "Lagerkapazität sollte nicht erhöht werden.");
        ClassicAssert.AreEqual(initialBalance, trader.AccountBalance, "Kontostand sollte unverändert bleiben.");
    }

    [Test]
    public void Sell_Successful()
    {
        var trader = new Trader("Max", "Max Co.", 10000);
        var product = new Product { Name = "Produkt", BasePrice = 125 };
        trader.OwnedProducts.Add(product, 20);

        businessLogic!.Sell(trader, product, 10);

        ClassicAssert.AreEqual(11000, trader.AccountBalance, "Kontostand sollte um den Verkaufserlös erhöht werden.");
        ClassicAssert.AreEqual(10, trader.OwnedProducts[product], "Bestand des verkauften Produkts sollte reduziert werden.");
    }

    [Test]
    public void Sell_Successful_UpdateDailyReport()
    {
        var trader = new Trader("Madalina", "Madalina AG.", 10000);
        var product = new Product { Name = "Produkt", BasePrice = 100, Availability = 50 };
        trader.OwnedProducts.Add(product, 30);

        double initialRevenue = trader.Revenue;
        int quantityToSell = 10;

        businessLogic!.Sell(trader, product, quantityToSell);

        ClassicAssert.AreEqual(initialRevenue + (quantityToSell * product.SellingPrice), trader.Revenue, "Tagesumsatz sollte aktualisiert werden.");
    }

    [Test]
    public void Sell_ProductNotInInventory()
    {
        var trader = new Trader("Max", "Max Co.", 10000);
        var product = new Product { Name = "Produkt", BasePrice = 100, Availability = 50 };

        Assert.Throws<ProductException>(() => businessLogic!.Sell(trader, product, 10), "Sollte eine ProductException werfen, wenn das Produkt nicht im Bestand ist.");
    }
}