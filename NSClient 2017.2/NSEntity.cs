/// <summary>
/// Copyright  NetSuite Inc. 1999. All rights reserved.
/// </summary>

using System;
using NSClient.com.netsuite.webservices;
using System.Collections;

namespace NSClient
{
    /// <summary>
    /// SuiteTalk Entity handling examples
    /// </summary>
    class NSEntity : NSBase
    {

        /// <summary>
        /// <p>Demonstrates how to add a Customer record into NetSuite using
        /// the add() operation. It also adds a second customer that is a 
        /// sub-customer of the first customer.</p>
        /// </summary>
        public static void AddCustomer()
        {

            Customer customer = new Customer();

            // Populate the address list for this customer. You can put in as many 
            // adresses as you like. 
            CustomerAddressbook addressbook = new CustomerAddressbook();

            addressbook.defaultShipping = true;
            addressbook.defaultShippingSpecified = true;
            addressbook.defaultBilling = false;
            addressbook.defaultBillingSpecified = true;
            addressbook.label = "Shipping Address";
            Address address = new Address();
            addressbook.addressbookAddress = address;
            address.addressee = "William Sanders";
            address.attention = "William Sanders";
            address.addr1 = "4765 Sunset Blvd";
            address.city = "San Francisco";
            address.state = "CA";
            address.zip = "94131";
            address.country = Country._unitedStates;

            Client.Out.WriteLn(
                    "\nPlease enter the following customer information. " +
                    "Note that some fields have already been populated. ");
            customer.companyName = NSUtility.ReadSimpleString("  Company name: ");
            String sEntityId = NSUtility.ReadSimpleString("  Entity name (optional depending on your account configuration): ");
            if (sEntityId != null && !sEntityId.Trim().Equals(""))
                customer.entityId = sEntityId;
            customer.email = NSUtility.ReadSimpleString("  E-mail: "); ;
            address.addr1 = NSUtility.ReadSimpleString("  Address 1: ");
            address.addr2 = NSUtility.ReadSimpleString("  Address 2: ");
            address.addr3 = NSUtility.ReadSimpleString("  Address 3: ");

            // Set email preference
            customer.emailPreference = EmailPreference._hTML;
            customer.emailPreferenceSpecified = true;

            // Set entity status. The internal ID can be obtained from Setup > Sales > Customer Statuses.
            // The default status is "Closed Won" which has an internal ID of 13
            RecordRef status = new RecordRef();
            status.internalId = NSUtility.ReadStringWithDefault("  Entity status internal ID (press enter for default value of Closed Won): ", "13");

            customer.entityStatus = status;



            // Attach the CustomerAddressbookList to the customer
            CustomerAddressbookList addressList = new CustomerAddressbookList();
            CustomerAddressbook[] addresses = new CustomerAddressbook[1];
            addresses[0] = addressbook;
            addressList.addressbook = addresses;
            customer.addressbookList = addressList;

            // Invoke add() operation
            WriteResponse response = Client.Service.add(customer);

            // Process the response
            ProcessCustomerWriteResponse(response, customer);
        }


        /// <summary>
        /// <p>Demonstrates how to Add/Update a Customer record into NetSuite using
        /// the upsert() operation.</p>
        /// </summary>
        public static void UpsertCustomer()
        {

            Customer customer = new Customer();

            // Set entityId, company name, and email

            Client.Out.WriteLn(
                    "\nPlease enter the following customer information. " +
                    "Note that some fields have already been populated. ");
            customer.externalId = NSUtility.ReadInternalId("  ExternalId: ");
            customer.companyName = NSUtility.ReadSimpleString("  Company name: ");
            String sEntityId = NSUtility.ReadSimpleString("  Entity name (optional depending on your account configuration): ");
            if (sEntityId != null && !sEntityId.Trim().Equals(""))
                customer.entityId = sEntityId;
            customer.email = NSUtility.ReadSimpleString("  E-mail: ");

            // Set email preference
            customer.emailPreference = EmailPreference._hTML;
            customer.emailPreferenceSpecified = true;

            // Set entity status. The internal ID can be obtained from Setup > Sales > Customer Statuses.
            // The default status is "Closed Won" which has an internal ID of 13
            RecordRef status = new RecordRef();
            status.internalId = NSUtility.ReadStringWithDefault("  Entity status internal ID (press enter for default value of Closed Won): ", "13");

            customer.entityStatus = status;

            // Populate the address list for this customer. You can put in as many 
            // addresses as you like. 
            CustomerAddressbook addressbook = new CustomerAddressbook();
            addressbook.defaultShipping = true;
            addressbook.defaultShippingSpecified = true;
            addressbook.defaultBilling = false;
            addressbook.defaultBillingSpecified = true;
            addressbook.label = "Shipping Address";
            Address address = new Address();
            addressbook.addressbookAddress = address;
            address.addressee = "William Sanders";
            address.attention = "William Sanders";
            address.addr1 = "4765 Sunset Blvd";
            address.city = "San Francisco";
            address.state = "CA";
            address.zip = "94131";
            address.country = Country._unitedStates;

            // Attach the CustomerAddressbookList to the customer
            CustomerAddressbookList addressList = new CustomerAddressbookList();
            CustomerAddressbook[] addresses = new CustomerAddressbook[1];
            addresses[0] = addressbook;
            addressList.addressbook = addresses;
            customer.addressbookList = addressList;

            // Invoke upsert() operation
            WriteResponse response = Client.Service.upsert(customer);

            // Process the response
            ProcessCustomerWriteResponse(response, customer);
        }

        /// <summary>
        /// <p>Demonstrates how to use custom fields when adding a Customer 
        /// record into NetSuite using the add() operation. The custom fields
        /// first need to be created in NetSuite, and the IDs for these fields 
        /// need to be obtained.</p>
        /// </summary>
        public static void AddCustomerWithCustomFields()
        {

            Customer customer = new Customer();

            // Set name and email
            customer.entityId = "XYZ Custom Inc";
            customer.companyName = "XYZ Custom, Inc.";

            // Create an ArrayList of custom fields, so that an arbitrary 
            // number of custom fields can be added.
            System.Collections.ArrayList cfArrayList = new System.Collections.ArrayList();

            int myChoice;

            Client.Out.WriteLn(
                "\nIn order to populate custom fields, ensure that these fields " +
                "already exist for the customer record. ");

            // Prompt for custom field type, instantiate that custom field, and 
            // add that field to array list
            while (true)
            {
                try
                {
                    Client.Out.WriteLn("\n  Select the type of custom field to be populated:");
                    Client.Out.WriteLn("    1) String (maps to one of Free-Form Text, Text Area, etc in the UI)");
                    Client.Out.WriteLn("    2) Boolean");
                    Client.Out.WriteLn("    3) List (maps to the List/Record type in UI)");
                    Client.Out.WriteLn("    4) Multi-Select List (maps to the Multiple Select type in UI)");
                    Client.Out.WriteLn("    Q) Quit, no more custom fields to be populated");

                    if (!NSUtility.TryReadIntChoice("\n  Custom field type selection: ", out myChoice))
                    {
                        break;
                    }
                    else
                    {
                        // Prompt for internal ID
                        String cfNSKey = NSUtility.ReadInternalId("\n  Internal ID for custom field: ");
                        String cfValue = null;

                        // Create instance of requested custom field, and prompt for
                        // a value(s). Then add this custom field to array list.
                        switch (myChoice)
                        {
                            case 1:

                                // Create a string custom field of type StringCustomFieldRef 
                                // Prompt for value and assign to CF
                                StringCustomFieldRef strCF = NSUtility.ReadStringCustomFieldValueWithDefault("Test Value");
                                strCF.internalId = cfNSKey;

                                // Add to ArrayList
                                cfArrayList.Add(strCF);
                                break;

                            case 2:

                                // Create a boolean custom field of type BooleanCustomFieldRef 
                                // Prompt for value and assign to CF
                                BooleanCustomFieldRef boolCF = NSUtility.ReadBooleanCustomFieldValueWithDefault(true);
                                boolCF.internalId = cfNSKey;

                                // Add to ArrayList
                                cfArrayList.Add(boolCF);
                                break;

                            case 3:

                                // Create a list/record custom field of type SelectCustomFieldRef 
                                SelectCustomFieldRef listCF = new SelectCustomFieldRef();
                                listCF.internalId = cfNSKey;

                                // Prompt for value and put into a ListOrRecordRef
                                cfValue = NSUtility.ReadSimpleString("  Value for Custom Field (must be an internal ID): ");
                                ListOrRecordRef listRef = new ListOrRecordRef();
                                listRef.internalId = cfValue;

                                // Add to ArrayList
                                listCF.value = listRef;
                                cfArrayList.Add(listCF);
                                break;

                            case 4:
                                var multiselectCF = NSUtility.ReadMultiSelectCustomField("  Values for Custom Field (must be internal IDs, separated by commas): ");
                                multiselectCF.internalId = cfNSKey;
                                cfArrayList.Add(multiselectCF);
                                break;

                            case 5:
                                break;
                        }
                    }
                }
                catch (System.FormatException)
                {
                    NSUtility.PrintInvalidChoice();
                }
            }

            // Add custom fields to customer record
            CustomFieldRef[] cfRefs = null;
            if (cfArrayList.Count > 0)
            {
                cfRefs = new CustomFieldRef[cfArrayList.Count];

                IEnumerator ienum = cfArrayList.GetEnumerator();
                for (int i = 0; ienum.MoveNext(); i++)
                {
                    cfRefs[i] = (CustomFieldRef)ienum.Current;
                }
            }
            else
            {
                Client.Out.Info("\nThere were no custom fields specified. The customer will not be added.");
                return;
            }

            customer.customFieldList = cfRefs;

            // Invoke add() operation
            WriteResponse response = Client.Service.add(customer);

            // Process the response
            ProcessCustomerWriteResponse(response, customer);
        }


        /// <summary>
        /// <p>Demonstrates how to update an existing customer record in 
        /// NetSuite using the update() operation which uses an internal ID to reference 
        /// the record to be updated.</p>
        /// </summary>
        public static void UpdateCustomer()
        {

            Customer customer = new Customer();

            // Get internal ID for update
            customer.internalId = NSUtility.ReadInternalId("\nEnter internal ID for customer record to be updated : ");

            // Set name and email
            customer.companyName = "XYZ 2, Inc.";
            customer.email = "bsanders@xyz.com";

            customer.entityId = NSUtility.ReadStringWithDefault("  Entity name (optional depending on your account configuration and hit Enter to skip): ","");

            // Populate the address. Updating a list through WS results in the 
            // entire contents of the previous list being replaced by the new
            // list.
            CustomerAddressbook addressbook = new CustomerAddressbook();
            addressbook.defaultBilling = false;
            addressbook.defaultBillingSpecified = true;
            addressbook.label = "Billing Address";
            Address address = new Address();
            addressbook.addressbookAddress = address;
            address.addr1 = "4765 Sunset Blvd";
            address.city = "San Mateo";
            address.state = "CA";
            address.country = Country._unitedStates;

            // Attach the address to the customer
            CustomerAddressbookList addressList = new CustomerAddressbookList();
            CustomerAddressbook[] addresses = new CustomerAddressbook[1];
            addresses[0] = addressbook;
            addressList.addressbook = addresses;
            customer.addressbookList = addressList;

            // Invoke update() operation
            WriteResponse response = Client.Service.update(customer);

            // Process the response
            ProcessCustomerWriteResponse(response, customer);
        }


        /// <summary>
        /// <p>Demonstrates how to update a list of existing customer records 
        /// in NetSuite using the updateList() operation.</p>
        /// </summary>
        public static void UpdateCustomerList()
        {

            // Prompt for list of internal IDs and put in an array
            string[] nsKeys = NSUtility.ReadStringArraySimple("\nEnter internal IDs for customer records to be updated (separated by commas): "); 

            // Create an array of Record objects to hold the customers
            Record[] records = new Record[nsKeys.Length];

            // For each submitted internal ID, populate a customer object
            for (int i = 0; i < nsKeys.Length; i++)
            {
                Customer customer = new Customer();

                // Update name
                customer.entityId = "XYZ Inc " + i;
                customer.companyName = "XYZ, Inc. " + i;

                customer.internalId = nsKeys[i].Trim();
                records[i] = customer;
            }

            // Invoke updateList() operation to update customers
            WriteResponse[] responses = Client.Service.updateList(records).writeResponse;

            // Process responses for all successful updates
            Client.Out.Info("\nThe following customers were updated successfully:");
            bool hasFailures = false;
            for (int i = 0; i < responses.Length; i++)
            {
                if ((responses[i] != null) && (responses[i].status.isSuccess))
                {
                    Client.Out.Info("\n  Customer[" + i + "]:");
                    Client.Out.Info(
                        "    key=" + ((RecordRef)responses[i].baseRef).internalId +
                        "\n    entityId=" + ((Customer)records[i]).entityId +
                        "\n    companyName=" + ((Customer)records[i]).companyName);
                }
                else
                {
                    hasFailures = true;
                }
            }

            // Process responses for all unsuccessful updates
            if (hasFailures)
            {
                Client.Out.Info("\nThe following customers were not updated:\n");
                for (int i = 0; i < responses.Length; i++)
                {
                    if ((responses[i] != null) && (!responses[i].status.isSuccess))
                    {
                        Client.Out.Info("  Customer[" + i + "]:");
                        Client.Out.Info("    key=" + ((RecordRef)responses[i].baseRef).internalId);
                        Client.Out.ErrorForRecord(Client.GetStatusDetails(responses[i].status));
                    }
                }
            }
        }


        /// <summary>
        /// <p>Demonstrates how to delete a list of existing customer records 
        /// in NetSuite using the deleteList() operation.</p>
        /// </summary>
        public static void DeleteCustomerList()
        {

            // Prompt for list of internal IDs and put in an array
            string[] nsKeys = NSUtility.ReadStringArraySimple("\nEnter internal IDs for customer records to be deleted (separated by commas): ");

            // First get the records from NS
            Client.Out.Write("\nChecking validity of internal IDs by using getList() to retrieve records ...\n");
            int numRecords = GetCustomerList(nsKeys, true);

            // Delete records, but only if there are records to delete
            if (numRecords > 0)
            {
                // Build an array of RecordRef objects 
                RecordRef[] recordRefs = new RecordRef[nsKeys.Length];
                for (int i = 0; i < nsKeys.Length; i++)
                {
                    RecordRef recordRef = new RecordRef();
                    recordRef.internalId = nsKeys[i];
                    recordRefs[i] = recordRef;
                    recordRefs[i].type = RecordType.customer;
                    recordRefs[i].typeSpecified = true;
                }

                bool userResponse = NSUtility.ReadBooleanSimple("\nDelete all the records above? [Y/N]:", false);
                Client.Out.Write("\n");

                // Delete records
                if (userResponse)
                {
                    if (Client.UseTba)
                    {
                        Client.SetPreferences();
                    }
                    WriteResponse[] delResponses = Client.Service.deleteList(recordRefs, Client.GetDefaultDeletionReason()).writeResponse;

                    // process response
                    Client.Out.Info("\nThe following customers were deleted:\n");
                    for (int i = 0; i < delResponses.Length; i++)
                    {
                        if (delResponses[i].status.isSuccess)
                        {
                            Client.Out.Info("  Customer[" + i + "]:");
                            Client.Out.Info("    key=" + ((RecordRef)delResponses[i].baseRef).internalId);
                        }
                        else
                        {
                            Client.Out.Info("  Customer[" + i + "]:");
                            Client.Out.ErrorForRecord(Client.GetStatusDetails(delResponses[i].status));
                        }
                    }
                }
                else
                {
                    Client.Out.Info("\nSince your answer was not Y, the records were not deleted.");
                }
            }
            else
            {
                Client.Out.Info("\nThere were no valid records to be deleted.");
            }
        }


        /// <summary>
        /// <p>Demonstrates how to get an existing record 
        /// in NetSuite using the get() operation.</p>
        /// </summary>
        public static void GetCustomer()
        {

            // Prompt for the internal ID
            String nsKey = NSUtility.ReadInternalId("\nInternal ID for record to be retrieved: ");

            // Invoke the get() operation to retrieve the record
            RecordRef recordRef = new RecordRef();
            recordRef.internalId = nsKey;
            recordRef.type = RecordType.customer;
            recordRef.typeSpecified = true;

            ReadResponse response = Client.Service.get(recordRef);

            // Process response from get() operation
            Client.Out.Info("\nRecord returned from get() operation: ");

            Customer customer = (Customer)response.record;
            ProcessCustomerReadResponse(response, customer);
        }


        /// <summary>
        /// <p>Demonstrates how to get a list of existing records 
        /// in NetSuite using the getList() operation.</p>
        /// </summary>
        public static void GetCustomerList()
        {
            // Prompt for list of internal IDs and put in an array
            string[] nsKeys = NSUtility.ReadStringArraySimple("\nInternal IDs for records to retrieved (separated by commas): ");
            GetCustomerList(nsKeys, false);
        }


        /// <summary>
        /// <p>Helper method that's an abstraction of the getList() operation  
        /// used by other methods</p>
        /// </summary>
        private static int GetCustomerList(string[] nsKeys, bool isExternal)
        {
            // Build an array of RecordRef objects and invoke the getList()
            // operation to retrieve these records
            RecordRef[] recordRefs = new RecordRef[nsKeys.Length];
            for (int i = 0; i < nsKeys.Length; i++)
            {
                RecordRef recordRef = new RecordRef();
                recordRef.internalId = nsKeys[i];
                recordRefs[i] = recordRef;
                recordRefs[i].type = RecordType.customer;
                recordRefs[i].typeSpecified = true;
            }
            ReadResponse[] getResponses = Client.Service.getList(recordRefs).readResponse;

            // Process response from get() operation
            if (!isExternal)
                Client.Out.Info("\nRecords returned from getList() operation: \n");

            int numRecords = 0;
            for (int i = 0; i < getResponses.Length; i++)
            {
                Client.Out.Info("\n  Record[" + i + "]: ");
                Customer customer = (Customer)getResponses?[i]?.record;
                if(ProcessCustomerReadResponse(getResponses[i], customer))
                    numRecords++;
            }

            return numRecords;
        }



        private static void ProcessCustomerWriteResponse(WriteResponse response, Customer customer)
        {
            // Process the response
            if (response.status.isSuccess)
            {
                Client.Out.Info(
                    "\nThe following customer was added/updated successfully:" +
                    "\n  key=" + ((RecordRef)response.baseRef)?.internalId +
                    "\n  ExternalID=" + ((RecordRef)response.baseRef)?.externalId +
                    "\n  entityId=" + customer?.entityId +
                    "\n  companyName=" + customer?.companyName +
                    "\n  email=" + customer?.email +
                    "\n  statusKey=" + customer?.entityStatus?.internalId +
                    "\n  addressbookList[0].label=" + customer?.addressbookList?.addressbook[0]?.label);
            }
            else
            {
                Client.Out.Error("The customer was not added/updated:", true);
                Client.Out.Error(Client.GetStatusDetails(response.status));
            }
        }

        private static bool ProcessCustomerReadResponse(ReadResponse response, Customer customer)
        {
            // Process the response
            if (response.status.isSuccess)
            {
                Client.Out.Info(
                    "    internalId=" + customer.internalId +
                    "\n    entityId=" + customer.entityId +
                    (customer.companyName == null ? "" : ("\n    companyName=" + customer.companyName)) +
                    (customer.entityStatus == null ? "" : ("\n    status=" + customer.entityStatus.name)) +
                    (customer.email == null ? "" : ("\n    email=" + customer.email)) +
                    (customer.phone == null ? "" : ("\n    phone=" + customer.phone)) +
                    "\n    isInactive=" + customer.isInactive +
                    (!customer.dateCreatedSpecified ? "" : ("\n    dateCreated=" + customer.dateCreated.ToShortDateString())));
                return true;
            }
            else
            {
                Client.Out.ErrorForRecord(Client.GetStatusDetails(response.status));
                return false;
            }
        }
    }
}
