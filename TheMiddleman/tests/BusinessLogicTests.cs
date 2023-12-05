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
}