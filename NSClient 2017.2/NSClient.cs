/// <summary>
/// 
/// Fully functional, command-line driven application that illustrates how
/// to connect to the NetSuite web services and invoke operations.
/// 
/// Copyright  NetSuite Inc. 1999. All rights reserved.
///
/// Author: Jan Arendtsz
/// Author: Aish Shukla
/// Author: Xi Liu
/// Author: Mihir Shah
/// Author: Andrej Hank
/// Author: Jakub Danek
///
/// Usage: NSClient.exe
///
/// </summary>

using System;
using System.Net;
using NSClient.com.netsuite.webservices;
using System.Xml;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NSClient
{
    /// <summary>
    /// Summary description for NSClient.
    /// </summary>
    public class NSClient
    {

        private NetSuiteService _service;

        /// <value>Proxy class that abstracts the communication with the 
        /// NetSuite Web Services. All NetSuite operations are invoked
        /// as methods of this class.</value>
        public NetSuiteService Service {
            get
            {
                // We need to create new TBA token for every request
                if(this.UseTba && _service.tokenPassport != null)
                    _service.tokenPassport = CreateTokenPassport();
                return _service;
            }
        }

        /// <value>Flag that indicates whether the user is currently 
        /// authentciated, and therefore, whether a valid session is 
        /// available</value>
        public bool IsAuthenticated { get; private set; }

        /// <value>Utility class for logging</value>
        public Logger Out { get; private set; }

		/// <value>A NameValueCollection that abstracts name/value pairs from
		/// the app.config file in the Visual .NET project. This file is called
		/// [AssemblyName].exe.config in the distribution</value>
		public System.Collections.Specialized.NameValueCollection DataCollection { get; private set; }

        /// <value>Default page size used for search in this application</value>
        public int PageSize { get; private set; }

        /// <value>Flag saying whether authentication is token based. </value>
        public bool UseTba { get; private set; }

        /// Set up request level preferences as a SOAP header
        public Preferences Prefs { get; private set; }
        public SearchPreferences SearchPreferences { get; private set; }

        // List of all posible actions and their implementations
        private static SuiteTalkAction[] SuiteTalkActions = new SuiteTalkAction[]
        {
            new SuiteTalkAction("Add a Customer", NSEntity.AddCustomer),
            new SuiteTalkAction("Add a Customer with Custom Fields (CFs must exist)", NSEntity.AddCustomerWithCustomFields),
            new SuiteTalkAction("Update a Customer (internal ID required)", NSEntity.UpdateCustomer),
            new SuiteTalkAction("Upsert a Customer (External ID required)", NSEntity.UpsertCustomer),
            new SuiteTalkAction("Update a List of Customers (internal IDs required)", NSEntity.UpdateCustomerList),
            new SuiteTalkAction("Get a Customer (internal ID required)", NSEntity.GetCustomer),
            new SuiteTalkAction("Get a List of Customers (internal IDs required)", NSEntity.GetCustomerList),
            new SuiteTalkAction("Delete a List of Customers (internal IDs required)", NSEntity.DeleteCustomerList),
            new SuiteTalkAction("Add an Inventory Item", NSItems.AddInventoryItem),
            new SuiteTalkAction("Add a Sales Order", NSTrasactions.AddSalesOrder),
            new SuiteTalkAction("Update a Sales Order to add a new item (internal id required)", NSTrasactions.UpdateSalesOrder),
            new SuiteTalkAction("Fulfill an Order (Sales Order internal id required)", NSTrasactions.FulfillSalesOrder),
            new SuiteTalkAction("Search Sales Orders by Customer Entity ID", NSTrasactions.SearchSalesOrderByEntityID),
            new SuiteTalkAction("Search Sales Orders with Advanced Search", NSTrasactions.SearchSalesOrdersWithAdvSearch),
            new SuiteTalkAction("Add a Custom Record (Custom Record type must exist, internal ID required)", NSCustomRecords.AddCustomRecord),
            new SuiteTalkAction("Search for a Custom Record (internal ID required)", NSCustomRecords.SearchCustomRecord),
            new SuiteTalkAction("Delete a Custom Record (internal ID required)", NSCustomRecords.DeleteCustomRecord),
            new SuiteTalkAction("Get Other List Values", NSSpecialities.GetAll),
            new SuiteTalkAction("Upload a File", NSSpecialities.UploadFile),
            new SuiteTalkAction("Get select field values", NSSpecialities.GetSelectFieldValues),
        };
        
        /// <summary>
        /// Constructor
        /// </summary>
        public NSClient(  )
		{
            IsAuthenticated = false;
			PageSize = 5;            

			Out = new Logger( "info" );

			// Reference to config file that contains sample data. This file
			// is named App.config in the Visual .NET project or, <AssemblyName>.exe.config
			// in the distribution
			DataCollection = System.Configuration.ConfigurationManager.AppSettings;

			//Decide between standard login and TBA
			UseTba = "true".Equals(DataCollection["login.useTba"]);

            // Instantiate the NetSuite web services
            _service = new DataCenterAwareNetSuiteService(DataCollection["login.acct"], "true".Equals(DataCollection["promptForLogin"]) && !UseTba);
            _service.Timeout = 1000 * 60 * 60 * 2;

			//Enable cookie management
			Uri myUri = new Uri("https://webservices.netsuite.com");
            _service.CookieContainer = new CookieContainer();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            // Force TLS 1.2
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // In order to enable SOAPscope to work through SSL. Refer to FAQ for more details
            ServicePointManager.ServerCertificateValidationCallback += delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
               return true;
            };

			NSClient ns = null;
            try
            {
                ns = new NSClient();
                NSBase.Client = ns;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error while loading the application:" + ex.Message);
				Console.WriteLine("Press Enter to quit ... ");
				Console.ReadKey();
                return;
			}
            ProcessCommands(ns);
        }

        private static void ProcessCommands(NSClient ns)
        {
            if (ns == null)
                return;

            var tryAgain = true;
            // Iterate through command options
            do
            {
                ns.SetPreferences();
                int myChoice = 0;
                string strMyChoice = null;
                try
                {
                    ns.Out.WriteLn("\nAdd Inventory:");
                    for (int i = 1; i < SuiteTalkActions.Length + 1; i++)
                    {
                        SuiteTalkAction action = SuiteTalkActions[i - 1];
                        String prefix = " ";
                        if (i < 10)
                        {
                            prefix += " ";
                        }
                        ns.Out.WriteLn(prefix + i.ToString() + ") " + action.Name);
                    }
                    ns.Out.WriteLn("  Q) Quit");
                    ns.Out.Write("\nSelection: ");

                    strMyChoice = ns.Out.ReadLn().ToUpper();
                    ns.Out.Write("\n");
                    if (String.Equals(strMyChoice, "Q"))
                    {
                        ns.Out.WriteLn("\nPress Enter to quit ... ");
                        String response = ns.Out.ReadLn();
                        break;
                    }
                    else
                    {
                        myChoice = Convert.ToInt32(strMyChoice);
                        if (myChoice > 0 && myChoice < SuiteTalkActions.Length + 2)
                        {
                            SuiteTalkActions[myChoice - 1].Method();
                        }
                        else
                            throw new FormatException();
                    }
                }
                catch (FormatException)
                {
                    NSUtility.PrintInvalidChoice();
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    // Get the fault type. It's the only child element of the detail element.
                    String fault = ex.Detail.FirstChild.Name;

                    // Get the list of child elements of the fault type element. 
                    // It should include the code and message elements
                    System.Collections.IEnumerator ienum = ex.Detail.FirstChild.ChildNodes.GetEnumerator();
                    String code = null;
                    String message = null;
                    while (ienum.MoveNext())
                    {
                        XmlNode node = (XmlNode)ienum.Current;
                        if (node.Name.Contains("code"))
                            code = node.InnerText;
                        else if (node.Name.Contains("message"))
                            message = node.InnerText;
                    }
                    ns.Out.Fault(fault, code, message);

                    // Check whether the session is invalid
                    if (code != null && (code.StartsWith("INVALID_PSWD") || code.StartsWith("INVALID_LOGIN")))
                    {
                        ns.IsAuthenticated = false;
                    }
                }
                catch (WebException ex)
                {
                    ns.Out.Fault(ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ns.Out.Fault(ex.Message);
                }
                catch (Exception ex)
                {
                    ns.Out.Error(ex.Message);
                }
                finally
                {
                    ns.Out.Write("\n TryAgain (y/n): ");

                    tryAgain = ns.Out.ReadLn().ToLower().Equals("y");
                }
            }
            while (tryAgain);
        }


        
		/// <summary>
		/// <p>This function builds the Pereferences and SearchPreferences in the SOAP header. </p>
		/// </summary>
		public void SetPreferences()
		{
			// Set up request level preferences as a SOAP header
			Prefs = new Preferences();
			_service.preferences = Prefs;
			SearchPreferences = new SearchPreferences();
            _service.searchPreferences = SearchPreferences;

			// Preference to ask NS to treat all warnings as errors
			Prefs.warningAsErrorSpecified = true;
			Prefs.warningAsError = false;
            Prefs.ignoreReadOnlyFieldsSpecified = true;
            Prefs.ignoreReadOnlyFields = true;

            SearchPreferences.pageSize = PageSize;
			SearchPreferences.pageSizeSpecified = true;
			// Setting this bodyFieldsOnly to true for faster search times on Opportunities
			SearchPreferences.bodyFieldsOnly = true;
            PrepareLoginPassport();
        }


        private void PrepareLoginPassport()
        {
            /*if (!UseTba)
            {
                if (_service.passport == null)
                {
                    _service.applicationInfo = CreateApplicationId();

                    // Populate Passport object with all login information
                    Passport passport = new Passport();
                    RecordRef role = new RecordRef();

                    // Determine whether to get login information from config 
                    // file or prompt for it
                    if ("true".Equals(DataCollection["promptForLogin"]))
                    {
                        Out.WriteLn("\nPlease enter your login information: ");
                        Out.Write("  E-mail: ");
                        passport.email = Out.ReadLn();
                        Out.Write("  Password: ");
                        passport.password = Out.ReadPassword();
                        Out.Write("  Role internal ID (press enter for default role): ");
                        String roleEntry = (Out.ReadLn()).Trim();
                        if (roleEntry.Length > 0)
                        {
                            role.internalId = roleEntry;
                            passport.role = role;
                        }
                        Out.Write("  Account: ");
                        passport.account = Out.ReadLn();
                        ((DataCenterAwareNetSuiteService)_service).SetAccount(passport.account);
                    }
                    else
                    {
                        passport.email = DataCollection["login.email"];
                        passport.password = DataCollection["login.password"];
                        role.internalId = DataCollection["login.roleNSkey"];
                        passport.role = role;
                        passport.account = DataCollection["login.acct"];
                    }
                    _service.passport = passport; 
                }
            }
            else*/
            //{
                _service.tokenPassport = CreateTokenPassport();
            //}
        }


        /// <summary>
        /// Update search preference to either return body fields, return columns or full records
        /// </summary>
        /// <param name="bodyFieldsOnly"></param>
        /// <param name="returnColumns"></param>
        public void SetSearchPreferences(bool bodyFieldsOnly, bool returnColumns)
		{
            _service.searchPreferences.bodyFieldsOnly = bodyFieldsOnly;
            _service.searchPreferences.returnSearchColumns = returnColumns;
		}


        /// <summary>
        /// <p>Processes the status object and prints the status details</p>
        /// </summary>
        public String GetStatusDetails( Status status )
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for ( int i=0; i < status.statusDetail.Length; i++ )
			{
				sb.Append( "[Code=" + status.statusDetail[i].code + "] " + status.statusDetail[i].message + "\n" );
			}
			return sb.ToString();
		}

        public TokenPassport CreateTokenPassport()
		{
			string accountId = DataCollection["login.acctId"];
			string accounthyphen = DataCollection["login.accthyphen"];
			string account = DataCollection["login.acct"];
			string consumerKey = DataCollection["login.tbaConsumerKey"];
			string consumerSecret = DataCollection["login.tbaConsumerSecret"];
			string tokenId = DataCollection["login.tbaTokenId"];
			string tokenSecret = DataCollection["login.tbaTokenSecret"];
						
			string nonce = ComputeNonce();
			long timestamp = ComputeTimestamp();
            TokenPassportSignature signature = ComputeSignature(account, consumerKey, consumerSecret, tokenId, tokenSecret, nonce, timestamp);

			TokenPassport tokenPassport = new TokenPassport();
			tokenPassport.account = account;
			tokenPassport.consumerKey = consumerKey;
			tokenPassport.token = tokenId;
			tokenPassport.nonce = nonce;
			tokenPassport.timestamp = timestamp;
			tokenPassport.signature = signature;
			return tokenPassport;
		}

        private string ComputeNonce()
		{
            var isAlphanumeric = false;
            var str = string.Empty;
            while (!isAlphanumeric)
            {
                //str = GenerateRandomString1();
                str = GenerateRandomString();
                //str = RandomString();
                Out.Write($"\n RandomString : {str}");
                isAlphanumeric = IsAlphaNumeric(str);
            }
                        
            return str;
        }

        private string GenerateRandomString1()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] data = new byte[50];
            rng.GetBytes(data);
            return Math.Abs(BitConverter.ToInt32(data, 0)).ToString();
            //return Encoding.UTF8.GetString(data);
        }

        // Generates a random string with a given size.    
        public string RandomString(int size = 20, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.  

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length=26  

            for (var i = 0; i < size; i++)
            {
                var @char = (char)new Random().Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }

        private string GenerateRandomString(int length = 20)
        {
            RNGCryptoServiceProvider rProvider = new RNGCryptoServiceProvider();
            StringBuilder res = new StringBuilder();
            byte[] random = new byte[1];
            using (rProvider)
            {
                while (0 < length--)
                {
                    char rndChar = '\0';
                    do
                    {
                        rProvider.GetBytes(random);
                        rndChar = (char)((random[0] % 92) + 33);
                    } while (!char.IsLetterOrDigit(rndChar));
                    res.Append(rndChar);
                }
            }
            return res.ToString();
        }

        private bool IsAlphaNumeric(string str)
        {
            return Regex.IsMatch(str, @"^[a-zA-Z0-9]+$");
        }

		private long ComputeTimestamp()
		{
            var unixTimestamp = ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            string timestamp = unixTimestamp.ToString();
            Out.Write($"\n TimeStamp : {timestamp}");
            return Convert.ToInt64(timestamp);
        }

        private TokenPassportSignature ComputeSignature(string compId, string consumerKey, string consumerSecret,  
										string tokenId, string tokenSecret, string nonce, long timestamp)
		{
            string baseString = $"{compId}&{consumerKey}&{tokenId}&{nonce}&{timestamp}";
			string key = $"{consumerSecret}&{tokenSecret}";
            string algorithm = string.Empty;
			string signature = Sign(baseString, key, out algorithm);
			TokenPassportSignature sign = new TokenPassportSignature();
			sign.algorithm = algorithm.Trim();
            sign.Value = signature.Trim();
            Out.Write($"\n algorithm : {algorithm}");
            Out.Write($"\n signature : {signature}");
            return sign;
		}

        private string Sign(string data, string key, out string algorithm)
        {
            Out.Write($"\n base : {data}");
            Out.Write($"\n key : {key}");
            algorithm = "HMAC_SHA256";
            var encoding = Encoding.Default;
            HMAC hmac = null;
            if(algorithm.Contains("SHA256"))
                hmac = new HMACSHA256(encoding.GetBytes(key.Trim()));
            else
                hmac = new HMACSHA1(encoding.GetBytes(key.Trim()));

            return Convert.ToBase64String(hmac.ComputeHash(encoding.GetBytes(data.Trim())));
        }

        private long TruncateTimestamp(long now, int len = 10)
        {
            string num = Convert.ToString(now);
            num = num.Substring(0, len);
            now = Convert.ToInt64(num);
            return now;
        }

		private ApplicationInfo CreateApplicationId()
		{
			ApplicationInfo applicationInfo = new ApplicationInfo();
			applicationInfo.applicationId = DataCollection["login.appId"];
			return applicationInfo;
		}

		/// <summary>
		/// Use to get default values for deletion reason
		/// </summary>
		/// <returns>DeletionReason with some default values</returns>
		public DeletionReason GetDefaultDeletionReason()
		{
			DeletionReason deletionReason = new DeletionReason();
			RecordRef deletionReasonCodeRef = new RecordRef();
			deletionReasonCodeRef.internalId = "1";
			deletionReason.deletionReasonCode = deletionReasonCodeRef;
			deletionReason.deletionReasonMemo = "Deleted from Sample Apps.";
			return deletionReason;
		}
	}
	class OverrideCertificatePolicy : ICertificatePolicy
	{
		public bool CheckValidationResult(ServicePoint srvPoint, System.Security.Cryptography.X509Certificates.X509Certificate certificate, WebRequest request, int certificateProblem)
		{
			return true;
		}
	}

    /// <summary>    
	/// Since 12.2 endpoint accounts are located in multiple datacenters with different domain names.
    /// In order to use the correct one, wrap the Service and get the correct domain first.
	///
	/// See getDataCenterUrls WSDL method documentation in the Help Center.	 
    /// </summary>
    class DataCenterAwareNetSuiteService : NetSuiteService
    {

        private System.Uri OriginalUri;

        public DataCenterAwareNetSuiteService(string account, bool doNotSetUrl)
            : base()
        {
            OriginalUri = new System.Uri(this.Url);
            if (account == null || account.Length == 0)
                account = "empty";
            if (!doNotSetUrl)
            { 
                DataCenterUrls urls = getDataCenterUrls(account).dataCenterUrls;
                Uri dataCenterUri = new Uri(urls.webservicesDomain + OriginalUri.PathAndQuery);
                this.Url = dataCenterUri.ToString();
            }
        }

        public void SetAccount(string account)
        {
            if (account == null || account.Length == 0)
                account = "empty";

            this.Url = OriginalUri.AbsoluteUri;
            DataCenterUrls urls = getDataCenterUrls(account).dataCenterUrls;
            Uri dataCenterUri = new Uri(urls.webservicesDomain + OriginalUri.PathAndQuery);
            this.Url = dataCenterUri.ToString();
        }
    }

    /// <summary>
    /// Single SuiteTalk action with name and implementations
    /// </summary>
    public class SuiteTalkAction
    {
        public delegate void SuiteTalkActionDelegate();

        public String Name;
        public SuiteTalkActionDelegate Method;

        public SuiteTalkAction(String name, SuiteTalkActionDelegate method)
        {
            Method = method;
            Name = name;
        }
    }

    /// <summary>
    /// Base class of all implemented methods calling SuiteTalk 
    /// </summary>
    class NSBase
    {
        protected static NSClient _client;

        public static NSClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new NSClient();
                }
                return _client;
            }
            set
            {
                _client = value;
            }
        }
    }

}
