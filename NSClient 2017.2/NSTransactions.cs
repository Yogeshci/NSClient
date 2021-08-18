/// <summary>
/// Copyright  NetSuite Inc. 1999. All rights reserved.
/// </summary>

using NSClient.com.netsuite.webservices;
using System;
using System.Collections.Generic;

namespace NSClient
{
    /// <summary>
    /// SuiteTalk Transactions handling examples
    /// </summary>
    class NSTrasactions : NSBase
    {

        private const String PENDING_APPROVAL_STRING = "_salesOrderPendingApproval";

        private const String PENDING_FULFILLMENT_STRING = "_salesOrderPendingFulfillment";

        private const String PRICE_CURRENCY_INTERNAL_ID = "1";

        private const String BASE_PRICE_LEVEL_INTERNAL_ID = "1";

        private const String ITEM_TYPE = "_inventoryItem";

        private const String TRANSACTION_TYPE = "_salesOrder";


        /// <summary> A helper method that takes an InventoryItem and prints out
        /// its Display Name, Item ID and Cost fields.
        /// </summary>		
        private void DisplayInventoryItemFields(InventoryItem item)
        {
            if (item != null)
            {
                if (item.displayName != null)
                {
                    Client.Out.WriteLn("     Display Name:                " + item.displayName);
                }
                else
                {
                    Client.Out.WriteLn("     Display Name:                none");
                }

                if (item.itemId != null)
                {
                    Client.Out.WriteLn("     Item ID:                     " + item.itemId);
                }
                else
                {
                    Client.Out.WriteLn("     Item ID:                     none");
                }

                if (item.purchaseDescription != null)
                {
                    Client.Out.WriteLn("     Purchase Description:        " + item.purchaseDescription);
                }
                else
                {
                    Client.Out.WriteLn("     Purchase Description:        none");
                }

                if ((System.Object)item.cost != null)
                {
                    Client.Out.WriteLn("     Cost:                        " + item.cost + "\n");
                }
                else
                {
                    Client.Out.WriteLn("     Cost:                        none\n");
                }
            }
        }

        /// <summary> Adds a SalesOrder record into NetSuite using the add() operation. Users
        /// are required to enter the entityID for the customer, as well as the
        /// internal IDs for the line items to be added to the sales order.
        /// 
        /// Note: do NOT add internal IDs for non-inventory items such as service items,
        /// discounts and subtotals.	
        /// </summary>
        public static void AddSalesOrder()
        {

            SalesOrder so = new SalesOrder();

            CustomerSearch custSearch = new CustomerSearch();
            SearchStringField customerEntityID = new SearchStringField();
            customerEntityID.@operator = SearchStringFieldOperator.@is;
            customerEntityID.operatorSpecified = true;
            // Set customer entity
            Client.Out.WriteLn("\nPlease enter the following customer Information. " + "Note that some fields have already been populated. ");
            customerEntityID.searchValue = NSUtility.ReadSimpleString("Customer entity name: ");

            CustomerSearchBasic custBasic = new CustomerSearchBasic();
            custBasic.entityId = customerEntityID;

            custSearch.basic = custBasic;

            // Search for the customer entity
            SearchResult res = Client.Service.search(custSearch);

            if (res.status.isSuccess)
            {
                if (res.recordList != null && res.recordList.Length == 1)
                {
                    RecordRef customer = new RecordRef();
                    customer.type = RecordType.customer;
                    customer.typeSpecified = true;
                    System.String entID = ((Customer)(res.recordList[0])).entityId;
                    customer.name = entID;
                    customer.internalId = ((Customer)(res.recordList[0])).internalId;
                    so.entity = customer;

                    // set the transaction date and status
                    so.tranDate = new System.DateTime();
                    so.orderStatus = SalesOrderOrderStatus._pendingFulfillment;

                    // Enter the internal ID for inventory items to be added to the SO
                    System.String[] itemKeysArray = NSUtility.ReadStringArraySimple("\nPlease enter the internal ID values for INVENTORY ITEMS seperated by commas (do not enter discount or subtotal items):\n");

                    SalesOrderItem[] salesOrderItemArray = new SalesOrderItem[itemKeysArray.Length];

                    // Create the correct sales order items and populate the
                    // quantity
                    for (int i = 0; i < itemKeysArray.Length; i++)
                    {
                        RecordRef item = new RecordRef();
                        item.type = RecordType.inventoryItem;
                        item.typeSpecified = true;
                        item.internalId = itemKeysArray[i];
                        salesOrderItemArray[i] = new SalesOrderItem();
                        salesOrderItemArray[i].item = item;

                        System.Double quantity = NSUtility.ReadSimpleDouble("\nPlease enter quantity for item " + itemKeysArray[i]+":\n");
                        salesOrderItemArray[i].quantity = quantity;
                        salesOrderItemArray[i].quantitySpecified = true;
                    }

                    SalesOrderItemList salesOrderItemList = new SalesOrderItemList();
                    salesOrderItemList.item = salesOrderItemArray;

                    so.itemList = salesOrderItemList;

                    if (Client.UseTba)
                    {
                        Client.SetPreferences();
                    }
                    WriteResponse writeRes = Client.Service.add(so);
                    if (writeRes.status.isSuccess)
                    {
                        Client.Out.WriteLn("\nSales order created successfully\nSales Order Internal ID=" + ((RecordRef)writeRes.baseRef).internalId);
                    }
                    else
                    {
                        Client.Out.Error(Client.GetStatusDetails(writeRes.status));
                    }
                }
                else
                {
                    Client.Out.WriteLn("\nSales order is not created because 0 or more than 1 customer records found for the entityID given");
                }
            }
            else
            {
                Client.Out.Error(Client.GetStatusDetails(res.status));
            }
        }

        /// <summary> Update a SalesOrder record using the update() operation. Users
        /// are required to enter the internalID for the order.
        /// 
        /// </summary>
        public static void UpdateSalesOrder()
        {
            SalesOrder so = new SalesOrder();

            // Get internal ID for update
            so.internalId = NSUtility.ReadInternalId("\nEnter internal ID for Sales Order record to be updated : ");

            // Enter the internal ID for inventory items to be added to the SO
            System.String[] itemKeysArray = NSUtility.ReadStringArraySimple("\nPlease enter the internal ID values for INVENTORY ITEMS seperated by commas (do not enter discount or subtotal items):\n");

            SalesOrderItem[] salesOrderItemArray = new SalesOrderItem[itemKeysArray.Length];

            // Create the correct sales order items and populate the
            // quantity
            for (int i = 0; i < itemKeysArray.Length; i++)
            {
                RecordRef item = new RecordRef();
                item.type = RecordType.inventoryItem;
                item.typeSpecified = true;
                item.internalId = itemKeysArray[i];
                salesOrderItemArray[i] = new SalesOrderItem();
                salesOrderItemArray[i].item = item;

                System.Double quantity = NSUtility.ReadSimpleDouble("\nPlease enter quantity for " + itemKeysArray[i] + ":\n"); 
                salesOrderItemArray[i].quantity = quantity;
                salesOrderItemArray[i].quantitySpecified = true;
            }

            SalesOrderItemList salesOrderItemList = new SalesOrderItemList();
            // sale more items
            salesOrderItemList.replaceAll = false;
            salesOrderItemList.item = salesOrderItemArray;

            so.itemList = salesOrderItemList;

            WriteResponse writeRes = Client.Service.update(so);
            if (writeRes.status.isSuccess)
            {
                Client.Out.WriteLn("\nSales order updated successfully");
            }
            else
            {
                Client.Out.Error(Client.GetStatusDetails(writeRes.status));
            }
        }


        /// <summary> Searches for SalesOrder records for a given customer.  The
        /// customer's entity ID needs to be entered by the user.  This
        /// method mainly performs the customer search, then delegates the
        /// actual transaction search to searchSOByEntityID.		
        /// </summary>
        public static void SearchSalesOrderByEntityID()
        {

            System.String entityIDString = NSUtility.ReadInternalId("\nEnter the entity ID for a customer:");

            CustomerSearch custSearch = new CustomerSearch();
            SearchStringField customerEntityID = new SearchStringField();
            customerEntityID.@operator = SearchStringFieldOperator.contains;
            customerEntityID.operatorSpecified = true;
            customerEntityID.searchValue = entityIDString;

            CustomerSearchBasic custBasic = new CustomerSearchBasic();
            custBasic.entityId = customerEntityID;

            custSearch.basic = custBasic;

            SearchResult res = Client.Service.search(custSearch);

            if (res.status.isSuccess)
            {
                if (res.totalRecords > 0)
                {
                    Record[] recordList;
                    List<Customer> customerList = new List<Customer>();
                    for (int i = 0; i <= res.totalPages - 1; i++)
                    {
                        recordList = res.recordList;
                        for (int j = 0; j <= recordList.Length - 1; j++)
                        {
                            customerList.Add((Customer)recordList[j]);
                        }
                        if (res.pageIndex < res.totalPages)
                        {
                            if (Client.UseTba)
                            {
                                Client.SetPreferences();
                            }
                            res = Client.Service.searchMoreWithId(res.searchId, res.pageIndex + 1);
                        }
                    }
                    foreach (Customer cust in customerList)
                    {
                        Client.Out.WriteLn("\n\nSales orders for customer " + cust.entityId + ":");
                        if (Client.UseTba)
                        {
                            Client.SetPreferences();
                        }
                        SearchSOByEntityID(cust);
                    }
                }
                else
                {
                    Client.Out.WriteLn("\nNo customer records found with entityID " + entityIDString);
                }
            }
            else
            {
                Client.Out.Error(Client.GetStatusDetails(res.status));
            }
        }

        /// <summary> This method searches for all the SalesOrders
        /// associated with the Customer record parameter.
        /// 
        /// </summary>		
        private static void SearchSOByEntityID(Customer customerRecord)
        {
            TransactionSearch xactionSearch = new TransactionSearch();
            SearchMultiSelectField entity = new SearchMultiSelectField();
            entity.@operator = SearchMultiSelectFieldOperator.anyOf;
            entity.operatorSpecified = true;
            RecordRef custRecordRef = new RecordRef();
            custRecordRef.internalId = customerRecord.internalId;
            custRecordRef.type = RecordType.customer;
            custRecordRef.typeSpecified = true;
            RecordRef[] custRecordRefArray = new RecordRef[1];
            custRecordRefArray[0] = custRecordRef;
            entity.searchValue = custRecordRefArray;

            SearchEnumMultiSelectField searchSalesOrderField = new SearchEnumMultiSelectField();
            searchSalesOrderField.@operator = SearchEnumMultiSelectFieldOperator.anyOf;
            searchSalesOrderField.operatorSpecified = true;
            System.String[] soStringArray = new System.String[1];
            soStringArray[0] = TRANSACTION_TYPE;
            searchSalesOrderField.searchValue = soStringArray;

            TransactionSearchBasic xactionBasic = new TransactionSearchBasic();
            xactionBasic.type = searchSalesOrderField;
            xactionBasic.entity = entity;

            xactionSearch.basic = xactionBasic;

            SearchResult res = Client.Service.search(xactionSearch);

            if (res.status.isSuccess)
            {
                Record[] recordList;
                for (int i = 1; i <= res.totalPages; i++)
                {
                    if (i > 1)
                    {
                        if (Client.UseTba)
                        {
                            Client.SetPreferences();
                        }
                        res = Client.Service.searchMoreWithId(res.searchId, i);
                        if (!res.status.isSuccess)
                        {
                            Client.Out.Error(Client.GetStatusDetails(res.status));
                            return;
                        }
                    }
                    recordList = res.recordList;

                    for (int j = 0; j < recordList.Length; j++)
                    {
                        if (recordList[j] is SalesOrder)
                        {
                            SalesOrder so = (SalesOrder)(recordList[j]);
                            Client.Out.WriteLn("\n     Sales Order #: " + so.tranId);
                            ProcessSalesOrderItemList(so);
                        }
                    }
                }
            }
            else
            {
                Client.Out.Error(Client.GetStatusDetails(res.status));
            }
        }

        /// <summary> This method prints out the line items, quantity, and total amount for
        /// a SalesOrder parameter.
        /// 
        /// </summary>		
        private static void ProcessSalesOrderItemList(SalesOrder so)
        {
            Client.Out.WriteLn("        Sales Order line item list:");
            if (so.itemList != null)
            {
                for (int i = 0; i < so.itemList.item.Length; i++)
                {
                    SalesOrderItem item = (SalesOrderItem)(so.itemList.item[i]);
                    Client.Out.WriteLn("        " + item.item.name + ", quantity: " + (int)item.quantity);
                }
                Client.Out.WriteLn("                                    " + "Total amount: " + so.total);
            }
        }

        /// <summary> This method searches for all the SalesOrders
        /// associated with the Customer record parameter.
        /// 
        /// </summary>		
        public static void SearchSalesOrdersWithAdvSearch()
        {

            Client.Out.WriteLn("\nEnter search parameters");

            // Instantiate a search object for transaction records.
            TransactionSearchAdvanced tranSearchAdv = new TransactionSearchAdvanced();
            TransactionSearch tranSearch = new TransactionSearch();
            TransactionSearchBasic tranSearchBasic = new TransactionSearchBasic();

            // Instantiate search return column objects
            Client.SetSearchPreferences(false, true);
            TransactionSearchRow tranSearchRow = new TransactionSearchRow();
            TransactionSearchRowBasic tranSearchRowBasic = new TransactionSearchRowBasic();

            // Search SOs only			
            SearchEnumMultiSelectField trantype = new SearchEnumMultiSelectField();
            trantype.@operator = SearchEnumMultiSelectFieldOperator.anyOf;
            trantype.operatorSpecified = true;
            System.String[] soStringArray = new System.String[1];
            soStringArray[0] = TRANSACTION_TYPE;
            trantype.searchValue = soStringArray;
            tranSearchBasic.type = trantype;

            // Search the tranId which is a string field
            String nameValue = NSUtility.ReadSimpleString(" Enter the tranId (press enter to skip): ");
            SearchStringField tranid = null;
            if (!nameValue.Equals(""))
            {
                tranid = new SearchStringField();
                tranid.@operator = SearchStringFieldOperator.contains;
                tranid.operatorSpecified = true;
                tranid.searchValue = nameValue;
                tranSearchBasic.tranId = tranid;
            }

            // Search the sales order customers
            String custKeysValue = NSUtility.ReadSimpleString("  Sales order customer (one or more internal IDs separated by commas, press enter to skip): ");
            SearchMultiSelectField customers = null;
            bool hasCustomers = custKeysValue != null && custKeysValue.Trim().Length > 0;
            if (hasCustomers)
            {
                customers = new SearchMultiSelectField();
                customers.@operator = SearchMultiSelectFieldOperator.anyOf;
                customers.operatorSpecified = true;
                string[] nskeys = custKeysValue.Split(new Char[] { ',' });

                RecordRef[] recordRefs = new RecordRef[custKeysValue.Length];
                for (int i = 0; i < nskeys.Length; i++)
                {
                    RecordRef recordRef = new RecordRef();
                    recordRef.internalId = nskeys[i];
                    recordRefs[i] = recordRef;
                }
                customers.searchValue = recordRefs;
                tranSearchBasic.entity = customers;
            }
            if (tranSearchBasic.tranId == null && !hasCustomers)
            {
                Client.Out.Info("\nNo search criteria was specified. Searching for all records.");
            }
            else
            {
                Client.Out.Info(
                    "\nSearching for sales orders with the following criteria: " +
                    (tranid == null ? "" : ("\n  tranId=" + tranSearchBasic.tranId.searchValue) + ", Operator=" + tranid.@operator.ToString()) +
                    (!hasCustomers ? "" : ("\n  customer internal IDs='" + custKeysValue + "', Operator=" + customers.@operator.ToString())));
            }

            // Define return columns
            // string column field
            SearchColumnStringField[] stringcols = new SearchColumnStringField[1];
            stringcols[0] = new SearchColumnStringField();

            // date column field
            SearchColumnDateField[] datecols = new SearchColumnDateField[1];
            datecols[0] = new SearchColumnDateField();

            // double column field
            SearchColumnDoubleField[] doublecols = new SearchColumnDoubleField[1];
            doublecols[0] = new SearchColumnDoubleField();


            // select columns
            SearchColumnSelectField[] selcols = new SearchColumnSelectField[1];
            selcols[0] = new SearchColumnSelectField();

            // Set return columns 
            tranSearchRowBasic.internalId = selcols;
            tranSearchRowBasic.tranId = stringcols;
            tranSearchRowBasic.dateCreated = datecols;
            tranSearchRowBasic.total = doublecols;
            tranSearchRowBasic.entity = selcols;


            // Apply search criteria
            tranSearch.basic = tranSearchBasic;
            tranSearchAdv.criteria = tranSearch;

            // Apply search return columns
            tranSearchRow.basic = tranSearchRowBasic;
            tranSearchAdv.columns = tranSearchRow;

            // Invoke search() web services operation
            SearchResult response = Client.Service.search(tranSearchAdv);

            // Process response
            if (response.status.isSuccess)
            {
                // Process the records returned in the response and print to console				
                if (response.totalRecords > 0)
                {
                    for (int i = 1; i <= response.totalPages; i++)
                    {
                        ProcessTransactionSearchResponse(response);
                        if (Client.UseTba)
                        {
                            Client.SetPreferences();
                        }
                        if (response.pageIndex < response.totalPages)
                        {
                            response = Client.Service.searchMoreWithId(response.searchId, i + 1);
                        }
                    }
                }
                else
                {
                    Client.Out.Info("\nNothing found.");
                }
            }
            else
            {
                Client.Out.Error(Client.GetStatusDetails(response.status));
            }
        }

        /// <summary>
        /// <p>Processes a search response and prints the return column results</p>
        /// </summary>
        private static void ProcessTransactionSearchResponse(SearchResult response)
        {
            Client.Out.Info("\nThe search() operation for transaction was run successfully.");
            Client.Out.Info("\n  Total Records = " + response.totalRecords);
            Client.Out.Info("  Total Pages = " + response.totalPages);
            Client.Out.Info("  Page Size = " + response.pageSize);
            Client.Out.Info("  Current Page Index = " + response.pageIndex);

            SearchRow[] records = response.searchRowList;

            for (int i = 0, j = (response.pageIndex - 1) * Client.PageSize; i < records.Length; i++, j++)
            {
                TransactionSearchRow transactionRow = (TransactionSearchRow)records[i];
                TransactionSearchRowBasic transactionRowBasic = transactionRow.basic;
                Client.Out.Info(
                    "\n  Transaction Return Columns Row[" + j + "]: " +
                    "\n    internalId=" + transactionRowBasic.internalId[0].searchValue.internalId +
                    "\n    tranId=" + transactionRowBasic.tranId[0].searchValue +
                    (transactionRowBasic.entity == null ? "" : ("\n    customer internalID=" + transactionRowBasic.entity[0].searchValue.internalId)) +
                    (transactionRowBasic.amount == null ? "" : ("\n    amount=" + transactionRowBasic.amount[0].searchValue)) +
                    (transactionRowBasic.dateCreated == null ? "" : ("\n    createdDate=" + transactionRowBasic.dateCreated[0].searchValue.ToShortDateString())));
            }
        }

        /// <summary> Fulfill a SalesOrder record using the initialize() and add() operations. Users
        /// are required to enter the internalID for a Sales Order record.
        /// </summary>
        public static void FulfillSalesOrder()
        {
            // Get SO internal ID for initialize op
            String soInternalId = NSUtility.ReadInternalId("\nEnter Internal ID for Sales Order record : ");

            InitializeRecord ir = new InitializeRecord();
            ir.type = InitializeType.itemFulfillment;
            InitializeRef iref = new InitializeRef();
            iref.typeSpecified = true;
            iref.type = InitializeRefType.salesOrder;
            iref.internalId = soInternalId;
            ir.reference = iref;

            // Perform initialize to get a copy of ItemFulfillment record
            ReadResponse getInitResp = Client.Service.initialize(ir);
            if (getInitResp.status.isSuccess)
            {
                // Keep a record of the original IF
                ItemFulfillment ifrec = (ItemFulfillment)getInitResp.record;

                ItemFulfillment recToFulfill = new ItemFulfillment();
                // Set createdFrom field
                recToFulfill.createdFrom = ifrec.createdFrom;

                ItemFulfillmentItemList ifitemlist = ifrec.itemList;
                ItemFulfillmentItem[] ifitems = new ItemFulfillmentItem[ifitemlist.item.Length];
                RecordRef locRef = new RecordRef();
                locRef.internalId = "1";
                for (int i = 0; i < ifitemlist.item.Length; i++)
                {
                    ItemFulfillmentItem ffItem = new ItemFulfillmentItem();
                    ffItem.item = ifitemlist.item[i].item;
                    ffItem.orderLineSpecified = true;
                    ffItem.orderLine = ifitemlist.item[i].orderLine;
                    ffItem.location = locRef;
                    ffItem.quantity = ifitemlist.item[i].quantityRemaining;
                    ffItem.quantitySpecified = true;
                    ifitems[i] = ffItem;
                }
                ItemFulfillmentItemList ifitemlistToFulfill = new ItemFulfillmentItemList();
                ifitemlistToFulfill.item = ifitems;
                recToFulfill.itemList = ifitemlistToFulfill;
                if (Client.UseTba)
                {
                    Client.SetPreferences();
                }
                WriteResponse writeRes = Client.Service.add(recToFulfill);

                if (writeRes.status.isSuccess)
                {
                    Client.Out.WriteLn("\nSales order fulfilled successfully\nItem Fulfilment Internal ID=" + ((RecordRef)writeRes.baseRef).internalId);
                }
                else
                {
                    Client.Out.Error(Client.GetStatusDetails(writeRes.status));
                }
            }
            else
            {
                Client.Out.Error(Client.GetStatusDetails(getInitResp.status));
            }
        }
    }
}
