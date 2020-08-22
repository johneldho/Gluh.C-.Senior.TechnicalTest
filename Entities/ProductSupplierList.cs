using System.Collections.Generic;

namespace Gluh.TechnicalTest.Entities
{
    public class ProductSupplierList
    {
        public List<ProductSupplierCombination> ProductSupplierCombination { get; set; }
        
        public decimal TotalCost { get; set; }
    }
}