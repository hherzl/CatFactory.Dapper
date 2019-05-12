namespace CatFactory.Dapper.Tests.Models
{
    public class Product
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }
    }

    public class Shipper
    {
        public int ShipperID { get; set; }

        public string CompanyName { get; set; }

        public string Phone { get; set; }
    }
}
