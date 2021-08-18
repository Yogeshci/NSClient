using System;
using NSClient.com.netsuite.webservices;

namespace NSClient
{
    /// <summary>
    /// SuiteTalk Items handling examples
    /// </summary>
    class NSItems : NSBase 
    {
        private const String PENDING_APPROVAL_STRING = "_salesOrderPendingApproval";

        private const String PENDING_FULFILLMENT_STRING = "_salesOrderPendingFulfillment";

        private const String PRICE_CURRENCY_INTERNAL_ID = "1";

        private const String BASE_PRICE_LEVEL_INTERNAL_ID = "1";

        private const String ITEM_TYPE = "_inventoryItem";

        private const String TRANSACTION_TYPE = "_salesOrder";


        public static void AddInventoryItem()
        {
            InventoryItem item = new InventoryItem();

            //String itemName =NSUtility.ReadSimpleString("Please enter the Item Name: ");
            String itemName = "CiTestItem";
            item.itemId = itemName;

            bool needValidInput = true;
            while (needValidInput)
            {
                //Client.Out.WriteLn("\nEnter the costing method (optional). ");
                //int intCostingMethod = NSUtility.ReadIntWithDefault("\nEnter 1 for AVERAGE, 2 for FIFO, 3 for LIFO:\n", 1);
                int intCostingMethod = 1;
                switch (intCostingMethod)
                {

                    case 1:
                        item.costingMethod = ItemCostingMethod._average;
                        item.costingMethodSpecified = true;
                        needValidInput = false;
                        break;


                    case 2:

                        item.costingMethod = ItemCostingMethod._fifo;
                        item.costingMethodSpecified = true;
                        needValidInput = false;
                        break;


                    case 3:
                        item.costingMethod = ItemCostingMethod._lifo;
                        item.costingMethodSpecified = true;
                        needValidInput = false;
                        break;
                }
            }

            CreatePricingMatrix(item);

            RecordRef taxScheduleRef = new RecordRef();
            taxScheduleRef.internalId = "1";
            item.taxSchedule = taxScheduleRef;

            WriteResponse writeRes = Client.Service.add(item);
            if (writeRes.status.isSuccess)
            {
                Client.Out.WriteLn("\nThe item " + itemName + " has been added successfully\nItem Internal ID=" + ((RecordRef)writeRes.baseRef).internalId);
            }
            else
            {
                Client.Out.Error(Client.GetStatusDetails(writeRes.status));
            }
        }

        /// <summary> This method takes an InventoryItem and sets its pricing matrix.
        /// It only sets the base price at quantity 0.  If an invalid price
        /// was entered (a non-numeric value), it does not set the pricing
        /// matrix.
        /// </summary>		
        private static void CreatePricingMatrix(InventoryItem item)
        {
            //double price = NSUtility.ReadSimpleDouble("\nPlease enter the base price, e.g. 25: ");
            double price = (double)20.0;
            Price[] prices = new Price[1];
            prices[0] = new Price();
            try
            {
                prices[0].value = price;
                prices[0].valueSpecified = true;
                //prices[0].quantity = NSUtility.ReadSimpleDouble("\nPlease enter the quantity, e.g. 1: ");
                prices[0].quantity = 1;
                prices[0].quantitySpecified = true;

                RecordRef currencyRef = new RecordRef();
                currencyRef.internalId = PRICE_CURRENCY_INTERNAL_ID;
                Pricing[] pricing = new Pricing[1];
                pricing[0] = new Pricing();
                pricing[0].currency = currencyRef;
                pricing[0].priceList = prices;
                pricing[0].discount = 0;
                pricing[0].discountSpecified = true;
                RecordRef priceLevel = new RecordRef();
                priceLevel.internalId = BASE_PRICE_LEVEL_INTERNAL_ID;
                priceLevel.type = RecordType.priceLevel;
                priceLevel.typeSpecified = true;
                pricing[0].priceLevel = priceLevel;

                PricingMatrix pricingMatrix = new PricingMatrix();
                pricingMatrix.pricing = pricing;
                pricingMatrix.replaceAll = false;

                item.pricingMatrix = pricingMatrix;
            }
            catch (System.FormatException)
            {
                Client.Out.Error("\nInvalid base price entered: " + price + ".  Proceed creating item without setting pricing matrix.");
            }
        }
    }
}
