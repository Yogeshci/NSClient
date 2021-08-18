using NetsuiteConnectTest.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NetsuiteConnectTest
{
    public class Class1
    {
        public static DataCenterAwareNetSuiteService service;

        private const string account = "6236462_SB1";

        //private const string appID = "E49FFC32-7579-4794-98BC-F554CB3D7F2B";
        private const string appID = "4303113C-6116-49E3-AEFB-66E30678EA08";

        //CONSUMER KEY
        private const string consumerKey = "522e11d11de8d10b09251f45f36611b56e2c822cc1b6240ced220695722704a2";

        //CONSUMER SECRET
        private const string consumerSecret = "4cd1facea947575bc8d3fb793b8e2e8ec7e2f0e36727478cc521d8584494a348";

        //TOKEN ID
        private const string tokenId = "2e787529699b8d56fbe2287869f3e3905743145a87ff582cd35b2590b60d83df";

        //TOKEN SECRET
        private const string tokenSecret = "cd1c7a112fd8908fa4239e5f587174b8cabf158f61007571baa4c37aee1aadc3";

        protected static Random random = new Random();

        public static void Main(string[] args)
        {
            var tryAgain = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            service = new DataCenterAwareNetSuiteService(account);

            //do
            //{
                service.Timeout = 1000 * 60 * 60 * 2;
                try
                {

                    /*ItemSearchBasic basic = new ItemSearchBasic()
                    {
                        internalId = new SearchMultiSelectField()
                        {
                            @operator = SearchMultiSelectFieldOperator.anyOf
                    ,
                            operatorSpecified = true
                    ,
                            searchValue = new RecordRef[] {
                            new RecordRef() {
                                internalId = "2234"
                            }
                    }
                        }
                    };*/

                    RecordRef recref = new RecordRef();
                    recref.type = RecordType.inventoryItem;
                    recref.typeSpecified = true;
                    recref.internalId = "47512";

                    service.tokenPassport = createTokenPassport();
                    ReadResponse result = service.get(recref);
                    if (result.status.isSuccess)
                    {
                       // Console.WriteLine("Success!");
                        var recod = (InventoryItem)result.record;
                        //Console.WriteLine(recod.subsidiaryList[0].name);
                        //Console.WriteLine(recod.displayName);
                       //Console.WriteLine(recod.itemId);
                    }
                    else
                    {
                       // Console.WriteLine("Failed: ");
                        foreach (StatusDetail detail in result.status.statusDetail)
                        {
                           // Console.WriteLine(" " + detail.message);
                        }
                    }

                   // Console.WriteLine();
                   // Console.WriteLine("Hit enter to close this window.");

                }
                catch (Exception ex)
                {
                    StringBuilder strbdr = new StringBuilder(ex.Message);
                    while (ex.InnerException != null)
                    {
                        strbdr.AppendLine($"Inner Exception -->> {ex.Message}");
                    }
                   // Console.WriteLine(strbdr.ToString());
                }
                finally
                {
                    ///Console.WriteLine("Enter 'Y/y' to try again");
                    //tryAgain = Console.ReadLine().ToLower().Contains("y");
                }
           // } while (tryAgain);

        }

        public static TokenPassport createTokenPassport()
        {
            string nonce = computeNonce();
            long timestamp = computeTimestamp();
            TokenPassportSignature signature = computeSignature(account, consumerKey, consumerSecret, tokenId, tokenSecret, nonce, timestamp);

            TokenPassport tokenPassport = new TokenPassport();
            tokenPassport.account = account;
            tokenPassport.consumerKey = consumerKey;
            tokenPassport.token = tokenId;
            tokenPassport.nonce = nonce;
            tokenPassport.timestamp = timestamp;
            tokenPassport.signature = signature;
            return tokenPassport;
        }

        private static string computeNonce()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] data = new byte[40];
            rng.GetBytes(data);
            int value = Math.Abs(BitConverter.ToInt32(data, 0));
            var nonce = value.ToString();
            Console.WriteLine("Nonce is " + nonce);
            return nonce;
        }

        private static long computeTimestamp()
        {
            /*var diff = DateTime.Now.Subtract(DateTime.MinValue);
            var millTotal = (long) diff.TotalSeconds;
            var utctotal = ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            return utctotal;*/

            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string timestamp = unixTimestamp.ToString();
            Console.WriteLine("Timestamp is " + timestamp);
            return Convert.ToInt64(timestamp);
        }



        private static TokenPassportSignature computeSignature(string compId, string consumerKey, string consumerSecret,
        string tokenId, string tokenSecret, string nonce, long timestamp)
        {
            string baseString = compId + "&" + consumerKey + "&" + tokenId + "&" + nonce + "&" + timestamp;
            string key = consumerSecret + "&" + tokenSecret;
            string signature = " ";
            var encoding = Encoding.Default;
            var algorithm = "HMAC_SHA256";
            byte[] keyBytes = encoding.GetBytes(key);
            byte[] baseStringBytes = encoding.GetBytes(baseString);

            GetComputedSignature(algorithm, keyBytes, baseStringBytes, out signature);

            TokenPassportSignature sign = new TokenPassportSignature();
            sign.algorithm = "HMAC_SHA256";
            sign.Value = signature;
            Console.WriteLine("Computed Signature is " + signature);
            return sign;
        }

        private static void GetComputedSignature(string algorithm, byte[] keyBytes, byte[] baseStringBytes, out string signature)
        {
            signature = string.Empty;
            byte[] hashBaseString;
            if (algorithm.Contains("256"))
            {
                using (var hmacSha1 = new HMACSHA256(keyBytes))
                {
                    hashBaseString = hmacSha1.ComputeHash(baseStringBytes);
                }
            }
            else
            {
                using (var hmacSha1 = new HMACSHA1(keyBytes))
                {
                    hashBaseString = hmacSha1.ComputeHash(baseStringBytes);
                }
            }
            signature = Convert.ToBase64String(hashBaseString);

        }
    }

    public class DataCenterAwareNetSuiteService : NetSuiteService
    {
        public DataCenterAwareNetSuiteService()
        {

        }
        public DataCenterAwareNetSuiteService(string account)
        : base()
        {
            System.Uri originalUri = new System.Uri(this.Url);
            DataCenterUrls urls = getDataCenterUrls(account).dataCenterUrls;
            Uri dataCenterUri = new Uri(urls.webservicesDomain + originalUri.PathAndQuery);
            this.Url = dataCenterUri.ToString();
        }

    }
}
