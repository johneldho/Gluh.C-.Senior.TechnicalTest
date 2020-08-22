using System;
using Gluh.TechnicalTest.Database;
using Gluh.TechnicalTest.Models;
using System.Collections.Generic;
using System.Linq;
using Gluh.TechnicalTest.Entities;

namespace Gluh.TechnicalTest
{
    public class PurchaseOptimizer
    {
        /// <summary>
        /// Calculates the optimal set of supplier to purchase products from.
        /// ### Complete this method
        /// </summary>
        public void Optimize(List<PurchaseRequirement> purchaseRequirements)
        {
            //List of all the suppliers with sufficient stocks
            var productSupplierStock = GetStockDetailsForEachProduct(purchaseRequirements);

            //Get the product wise total price for each supplier with out shipping charges
            var productSupplierCombinationList = GetAllCombinationOfProductSupplier(productSupplierStock);
            
            //Now calculates the total cost for all the combinations possible 
            var supplierCombinationList = productSupplierCombinationList.ToList();
            CalculateTotalCostForEachCombination(supplierCombinationList);

            //find the lowest order value from the combination list and Generate the purchase Order
            GeneratePurchaseOrder(supplierCombinationList);
        }

        /// <summary>
        /// Get all the list of suppliers for which stocks are available for each of the products in the requirement
        /// </summary>
        /// <param name="purchaseRequirements"></param>
        private IEnumerable<ProductStockQantity> GetStockDetailsForEachProduct(IEnumerable<PurchaseRequirement> purchaseRequirements)
        {
            var productStockWithQuantity = new List<ProductStockQantity>();

            // iterate the list of products 
            foreach (var productItems in purchaseRequirements)
            {
                productStockWithQuantity.AddRange(productItems.Product.Stock
                    .Where(a => a.Product == productItems.Product && a.StockOnHand >= productItems.Quantity)
                    .Select(o => new ProductStockQantity()
                    {
                        Cost = o.Cost,
                        ID = o.ID,
                        Product = o.Product,
                        StockOnHand = o.StockOnHand,
                        Supplier = o.Supplier,
                        RequiredQuantity = productItems.Quantity
                    }).ToList());
            }

            return productStockWithQuantity;
        }

        /// <summary>
        /// This ,method will extract all the possible combination of product supplier list 
        /// </summary>
        /// <param name="productStock"></param>
        /// <returns></returns>
        private IEnumerable<ProductSupplierList> GetAllCombinationOfProductSupplier(IEnumerable<ProductStockQantity> productStock)
        {
            var productSupplierCombinationListNew = productStock.Select(stock => new ProductSupplierCombination()
            {
                Product = stock.Product,
                Supplier = stock.Supplier,
                RequiredQuantity = stock.RequiredQuantity,
                CostWithoutShipping = 0
            }).ToList();

            return productSupplierCombinationListNew.GroupBy(x => x.Product)
                .Aggregate(
                    Enumerable.Repeat(Enumerable.Empty<ProductSupplierCombination>(), 1),
                    (acc, seq) =>
                        from accseq in acc
                        from item in seq
                        select accseq.Concat(new List<ProductSupplierCombination>() { item }))
                .Select(o => new ProductSupplierList()
                {
                    ProductSupplierCombination = o.ToList()
                }).ToList();
        }


        /// <summary>
        /// This method will find all total cost of item without shipping costs for each of the combination in the list and also calls the method to get the supplierShipping costs
        /// </summary>
        /// <param name="productSupplierCombinationList"></param>
        private void CalculateTotalCostForEachCombination(IEnumerable<ProductSupplierList> productSupplierCombinationList)
        {
            foreach (var productList in productSupplierCombinationList)
            {

                
                productList.ProductSupplierCombination
                    .ForEach(psc => psc.CostWithoutShipping = psc.Product.Stock.Find(we => we.Supplier == psc.Supplier).Cost * psc.RequiredQuantity);

                //3762.24 + 1363.64 + 10
                //if above total  > max shipping cost value then add shipping cost 


                ////Group by supplier to find supplier to find Total Cost Without Shipping
                var groupedSupplierRecord = productList.ProductSupplierCombination.GroupBy(psc => psc.Supplier)
                    .Select(psc => new { Supplier = psc.Key, TotalCostWithoutShipping = psc.Sum(ps => ps.CostWithoutShipping) }).ToList();

                //Finds sum of total Costwithout shipping for this combination
                var totalWithoutShippingCost = groupedSupplierRecord.Sum(tws=> tws.TotalCostWithoutShipping);


                //Total Cost including Shipping
                productList.TotalCost = totalWithoutShippingCost + groupedSupplierRecord.
                                                    Where(tc => tc.TotalCostWithoutShipping < tc.Supplier.ShippingCostMinOrderValue || tc.TotalCostWithoutShipping > tc.Supplier.ShippingCostMaxOrderValue)
                                                    .Sum(tc => tc.Supplier.ShippingCost);
                 
                 
            }
            
        } 

        /// <summary>
        /// Generates the lowest order value from the combination and display the product and quantity to be send to each supplier
        /// </summary>
        /// <param name="productSupplierCombinationList"></param>
        private void GeneratePurchaseOrder(IEnumerable<ProductSupplierList> productSupplierCombinationList)
        {
            var lowerOrderList = productSupplierCombinationList.OrderBy(a => a.TotalCost)?.FirstOrDefault();
            
            //TODO: add the items which are not in stock with any of the supplier to the list while generating the Purchase order

            var groupedSupplier = lowerOrderList?.ProductSupplierCombination.GroupBy(w => w.Supplier);

            if (groupedSupplier != null)
                foreach (var orderItem in groupedSupplier)
                {
                    Console.WriteLine("  Supplier Name: " + orderItem.Key.Name);

                    foreach (var product in orderItem)
                    {
                        Console.WriteLine("       Product ID: " + product.Product.ID + ", Name: " +
                                          product.Product.Name +
                                          ", Quantity: " + product.RequiredQuantity);
                    }

                    Console.WriteLine();
                }
        }

    }
}
