using System;
using NSClient.com.netsuite.webservices;
using System.Collections;

namespace NSClient
{
    /// <summary>
    /// SuiteTalk Custom Records handling examples
    /// </summary>
    class NSCustomRecords : NSBase
    {
        /// <summary>
        /// <p>Adds a Custom Record of a certain Custom Record type into NetSuite.
        ///  Custom Record type must already exist in the system and internal ID for the 
        ///  Custom Record type must be supplied.</p>
        /// </summary>
        public static void AddCustomRecord()
        {

            CustomRecord customRecord = new CustomRecord();
            var customRecordRef = new RecordRef();

            customRecordRef.internalId = NSUtility.ReadInternalId("Enter internal ID for Custom Record Type to be added: ");
            customRecord.name = NSUtility.ReadInternalId("  Custom Record name: ");
            customRecord.recType = customRecordRef;
            // Invoke add() operation
            WriteResponse response = Client.Service.add(customRecord);

            // Process the response
            if (response.status.isSuccess)
            {
                Client.Out.Info(
                    "\nThe following Custom Record was added successfully:" +
                    "\n  Custom Record type key=" + ((CustomRecordRef)response.baseRef).typeId +
                    "\n  Custom Record key=" + ((CustomRecordRef)response.baseRef).internalId +
                    "\n  Custom Record key name=" + customRecord.name);
            }
            else
            {
                Client.Out.Error("The Custom Record was not added:", true);
                Client.Out.Error(Client.GetStatusDetails(response.status));
            }
        }


        /// <summary>
        /// <p>Deletes a Custom Record. Custom Record type must already exist in the system.
        ///  internal ID for the Custom Record type must be supplied. internal ID for the Custom Record
        ///  to be deleted must be provided.</p>
        /// </summary>
        public static void DeleteCustomRecord()
        {
            CustomRecordRef customRecordRef = new CustomRecordRef();
            //Prompt user for the internal ID for Custom Record type to be deleted	
            customRecordRef.typeId = NSUtility.ReadInternalId("Enter internal ID for Custom Record Type to be deleted: ");
            //Prompt user for internal ID for Custom Record to be deleted
            customRecordRef.internalId = NSUtility.ReadInternalId("Enter internal ID for Custom Record to be deleted: ");

            // Delete records
            WriteResponse delResponse = Client.Service.delete(customRecordRef, Client.GetDefaultDeletionReason());

            // process response
            Client.Out.Info("\nThe following Custom Record deleted:\n");
            if (delResponse.status.isSuccess)
            {
                Client.Out.Info("    key=" + ((CustomRecordRef)delResponse.baseRef).internalId);
            }
            else
            {
                Client.Out.ErrorForRecord(Client.GetStatusDetails(delResponse.status));
            }
        }


        /// <summary>
        /// <p>Searches for a Custom Record. Since Custom Record implementations
        /// can vary vastly, this search is only implemented to search on the Custom
        /// Record "Name". Please ensure that the custom record type you are searching
        /// for has the "Show Name" check box checked.</p>
        /// </summary>
        public static void SearchCustomRecord()
        {

            Client.Out.WriteLn("\nEnter search parameters");
            CustomRecordSearch customRecordSearch = new CustomRecordSearch();
            CustomRecordSearchBasic customRecordSearchBasic = new CustomRecordSearchBasic();
            RecordRef recordRef = new RecordRef();
            //Prompt user for the internal ID for Custom Record type to be searched
            recordRef.internalId = NSUtility.ReadInternalId("Enter internal ID for Custom Record type to be searched: ");
            customRecordSearchBasic.recType = recordRef;
            //Prompt user for name for Custom Record to be searched
            String nameValue = NSUtility.ReadInternalId("Enter name for Custom Record to be searched: ");
            SearchStringField customRecordName = null;
            if (!nameValue.Equals(""))
            {
                customRecordName = new SearchStringField();
                customRecordName.@operator = SearchStringFieldOperator.contains;
                customRecordName.operatorSpecified = true;
                customRecordName.searchValue = nameValue;
                customRecordSearchBasic.name = customRecordName;
            }
            else
                Client.Out.Info("\nNo search criteria was specified. Searching for all records.");

            customRecordSearch.basic = customRecordSearchBasic;
            // Invoke search() web services operation
            SearchResult response = Client.Service.search(customRecordSearch);

            // Process response
            if (response.status.isSuccess)
            {
                // Process the records returned in the response and print to console
                // Get more records with pagination
                if (response.totalRecords > 0)
                {
                    for (int i = 1; i <= response.totalPages; i++)
                    {
                        ProcessCustomRecordSearchResponse(response);
                        if (response.pageIndex < response.totalPages)
                        {
                            Client.SetPreferences();
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
        /// <p>Processes a search response and prints the results</p>
        /// </summary>
        public static void ProcessCustomRecordSearchResponse(SearchResult response)
        {
            Client.Out.Info("\nThe search() operation for custom records was run successfully.");
            Client.Out.Info("\n  Total Records = " + response.totalRecords);
            Client.Out.Info("  Total Pages = " + response.totalPages);
            Client.Out.Info("  Page Size = " + response.pageSize);
            Client.Out.Info("  Current Page Index = " + response.pageIndex);

            Record[] records = response.recordList;

            CustomRecord customRecord;
            for (int i = 0, j = (response.pageIndex - 1) * Client.PageSize; i < records.Length; i++, j++)
            {
                customRecord = (CustomRecord)records[i];
                Client.Out.Info(
                    "\n  Record[" + j + "]: " +
                    "\n  Custom Record type key: " + (customRecord.recType).internalId +
                    "\n    internal ID=" + customRecord.internalId +
                    "\n    name=" + customRecord.name);
            }
        }
    }
}
