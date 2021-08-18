using NSClient.com.netsuite.webservices;
using System;

namespace NSClient
{
    /// <summary>
    /// SuiteTalk UploadFile and other specialized calls
    /// </summary>
    class NSSpecialities : NSBase
    {
        private const String SELECTFIELD_SUBSIDIARY = "subsidiary";

        /// <summary>
        /// <p>Demonstrates how to upload a file from a computer
        /// using the add() operation.</p>
        /// </summary>
        public static void UploadFile()
        {

            // Prompt for the file name
            String sFileName = NSUtility.ReadSimpleString("\nFile to be uploaded: ");

            // Prompt for the NetSuite file name
            String sNsFileName = NSUtility.ReadSimpleString("\nFile name to be used in NetSuite: ");

            // Prompt for the file type
            String sFileType = NSUtility.ReadSimpleString("\nFile type (e.g. plaintext, csv, etc.): ");

            //Prompt user for the folder internal ID
            String sFolderId = NSUtility.ReadSimpleString("\nInternal ID for folder: ");

            File uploadFile = new File();
            uploadFile.attachFromSpecified = true;
            uploadFile.attachFrom = FileAttachFrom._computer;

            // Specify a folder
            // Please note that you may need to set your own folder internalId
            if (sFolderId != null)
            {
                RecordRef folderRef = new RecordRef();
                folderRef.internalId = sFolderId;
                uploadFile.folder = folderRef;
            }

            // Specify the NetSuite filename
            if (sNsFileName != null)
                uploadFile.name = sNsFileName;

            uploadFile.fileTypeSpecified = true;
            if (sFileType != null)
            {
                if (sFileType.Trim().ToLower().Equals("plaintext"))
                    uploadFile.fileType = MediaType._PLAINTEXT;
                else if (sFileType.Trim().ToLower().Equals("image"))
                    uploadFile.fileType = MediaType._IMAGE;
                else if (sFileType.Trim().ToLower().Equals("csv"))
                    uploadFile.fileType = MediaType._CSV;
                else
                    uploadFile.fileType = MediaType._PLAINTEXT;
            }
            else
                uploadFile.fileType = MediaType._PLAINTEXT;

            uploadFile.content = LoadFile(sFileName);

            // Invoke add() operation to upload the file to NetSuite
            WriteResponse response = Client.Service.add(uploadFile);

            // Process the response
            if (response.status.isSuccess)
            {
                Client.Out.Info(
                    "\nThe file was uploaded successfully:" +
                    "\nFile Record key=" + ((RecordRef)response.baseRef).internalId);
            }
            else
            {
                Client.Out.Error("The file was not uploaded:", true);
                Client.Out.Error(Client.GetStatusDetails(response.status));
            }
        }

        private static byte[] LoadFile(String sFileName)
        {
            System.IO.FileStream inFile;
            byte[] data;

            try
            {
                inFile = new System.IO.FileStream(sFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                data = new Byte[inFile.Length];
                long bytesRead = inFile.Read(data, 0, (int)inFile.Length);
                inFile.Close();
            }
            catch (System.Exception exp)
            {
                // Error creating stream or reading from it.
                Client.Out.Error(exp.Message);
                return null;
            }

            return data;
        }


        /// <summary>
        /// <p>Demonstrates how to get a list of values for an other list type  
        /// in NetSuite using the getAll() operation. </p>
        /// </summary>
        public static void GetAll()
        {

            // Instantiate GetAllRecord and set the record type
            GetAllRecord record = new GetAllRecord();
            record.recordTypeSpecified = true;

            Client.Out.WriteLn("\nSelect the list type to be retrieved");
            Client.Out.WriteLn("  1) Budget Categories");
            Client.Out.WriteLn("  2) Campaign Categories");
            Client.Out.WriteLn("  3) States");
            Client.Out.WriteLn("  4) Currencies");
            int choice = NSUtility.ReadIntWithDefault("\nSelection: ", 0);
            Client.Out.Write("\n");

            if (choice > 4 || choice < 1)
            {
                NSUtility.PrintInvalidChoice();
                return;
            }
            switch (choice)
            {
                case 1:
                    record.recordType = GetAllRecordType.budgetCategory;
                    break;
                case 2:
                    record.recordType = GetAllRecordType.campaignCategory;
                    break;
                case 3:
                    record.recordType = GetAllRecordType.state;
                    break;
                case 4:
                    record.recordType = GetAllRecordType.currency;
                    break;
                default:
                    record.recordTypeSpecified = false;
                    break;
            }

            // Invoke getAll() operation
            GetAllResult result = Client.Service.getAll(record);

            if (result.recordList == null || result.totalRecords == 0)
            {
                Client.Out.WriteLn("  No record found.");
                return;
            }

            Record[] records = result.recordList;

            if (result.status.isSuccess)
            {
                Client.Out.Info(
                    "\nThe requested list for " + record.recordType.ToString() +
                    " returned " + result.totalRecords + " records:");
                int numRecords = 0;
                for (int i = 0; i < records.Length; i++)
                {
                    numRecords++;
                    switch (choice)
                    {
                        case 1:
                            BudgetCategory budgetCategory = (BudgetCategory)records[i];
                            Client.Out.Info("  key=" + budgetCategory.internalId + ", name=" + budgetCategory.name);
                            break;
                        case 2:
                            CampaignCategory campaignCategory = (CampaignCategory)records[i];
                            Client.Out.Info("  key=" + campaignCategory.internalId + ", name=" + campaignCategory.name);
                            break;
                        case 3:
                            State state = (State)records[i];
                            Client.Out.Info("  key=" + state.internalId + ", name=" + state.fullName);
                            break;
                        case 4:
                            Currency currency = (Currency)records[i];
                            Client.Out.Info("  key=" + currency.internalId + ", name=" + currency.name);
                            break;
                    }
                } // for
            }
        }

        /// <summary>
        /// <p>Demonstrates how to get a list of posible values for Select field  
        /// in NetSuite using the getSelectValue() operation. </p>
        /// </summary>
        public static void GetSelectFieldValues()
        {
            // Instantiate GetSelectValueFieldDescription and set the record type
            var fieldDescription = new GetSelectValueFieldDescription();
            fieldDescription.recordTypeSpecified = false;
            while (!fieldDescription.recordTypeSpecified)
            {
                Client.Out.WriteLn("\nSelect the record type of field to be retrieved");
                Client.Out.WriteLn("  1) Customer");
                Client.Out.WriteLn("  2) Vendor");
                Client.Out.WriteLn("  3) Inventory Item");
                Client.Out.WriteLn("  4) Account");
                int choice = NSUtility.ReadIntWithDefault("\nSelection: ", 0);
                Client.Out.Write("\n");

                fieldDescription.recordTypeSpecified = true;
                switch (choice)
                {
                    case 1:
                        fieldDescription.recordType = RecordType.customer;
                        break;
                    case 2:
                        fieldDescription.recordType = RecordType.vendor;
                        break;
                    case 3:
                        fieldDescription.recordType = RecordType.inventoryItem;
                        break;
                    case 4:
                        fieldDescription.recordType = RecordType.account;
                        break;
                    default:
                        fieldDescription.recordTypeSpecified = false;
                        Client.Out.Info("\n  Invalid choice. Please select one of the following options.");
                        break;
                }
            }

            // Get record name
            String fieldChoice = NSUtility.ReadSimpleString("Write field name of selected record (press enter for default value of subsidiary): ");
            // Set default value
            if (!(fieldChoice?.Length > 0))
            {
                fieldChoice = SELECTFIELD_SUBSIDIARY;
            }

            fieldDescription.field = fieldChoice;

            // pages starting from 1
            var response = Client.Service.getSelectValue(fieldDescription, 1);

            if (response.status.isSuccess)
            {
                // Process the records returned in the response and print to console
                // Get more records with pagination
                if (response.totalRecords > 0)
                {
                    for (int i = 0; i < response.baseRefList.Length; i++)
                    {
                        var recordRef = (RecordRef)response.baseRefList[i];
                        Client.Out.Info(
                            "\n  Select Field Return Columns Row[" + i + "]: " +
                            "\n    internalId=" + recordRef.internalId +
                            (recordRef.name == null ? "" : ("\n    name=" + recordRef.name.Replace("&nbsp;",""))) +
                            (recordRef.typeSpecified ? "" : ("\n    type=" + recordRef.type))
                        );
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

    }
}
