using Gluh.TechnicalTest.Database;

namespace Gluh.TechnicalTest.Entities
{
    public class ProductSupplierCombination
    {
        public Product Product { get; set; }
        public Supplier Supplier { get; set; }
        public int RequiredQuantity { get; set; }
        public decimal CostWithoutShipping { get; set; }

       
    }
}