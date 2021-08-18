using NSClient.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSClient
{
    /// <summary>
    /// Utility class for reading data from input
    /// </summary>
    static class NSUtility
    {
        public static bool TryReadIntChoice(String message,out int myChoice)
        {
            NSBase.Client.Out.Write(message);
            var strMyChoice = NSBase.Client.Out.ReadLn().ToUpper();
            if (String.Equals(strMyChoice, "Q"))
            {
                myChoice = 0;
                return false;
            }
            else
            {
                myChoice = Convert.ToInt32(strMyChoice);
                return true;
            }
        }

        public static int ReadIntWithDefault(String message, int defaultValue)
        {
            NSBase.Client.Out.Write(message);
            var strMyChoice = NSBase.Client.Out.ReadLn().ToUpper()?.Trim();
            var intMyChoice = defaultValue;
            Int32.TryParse(strMyChoice, out intMyChoice);
            return intMyChoice;
        }

        public static String ReadInternalId(String message)
        {
            NSBase.Client.Out.Write(message);
            var internalId = NSBase.Client.Out.ReadLn();
            return internalId;
        }

        public static String ReadStringWithDefault(String message, String defaultValue)
        {
            NSBase.Client.Out.Write(message);
            var stringValue = NSBase.Client.Out.ReadLn();
            if (stringValue?.Trim().Length == 0)
                stringValue = defaultValue;
            return stringValue;
        }

        public static StringCustomFieldRef ReadStringCustomFieldValueWithDefault(String defaultValue)
        {
            var value = new StringCustomFieldRef();
            String stringValue = ReadStringWithDefault("  Value (press enter for default value): ", defaultValue);
            value.value = stringValue;
            return value;
        }

        public static bool ReadBooleanSimple(String message, bool defaultValue)
        {
            String stringValue;
            NSBase.Client.Out.Write(message);
            stringValue = NSBase.Client.Out.ReadLn().ToUpper();
            if (stringValue.Equals("T") || stringValue.Equals("Y"))
                return true;
            else if (stringValue.Equals("F") || stringValue.Equals("N"))
                return false;
            else
                return defaultValue;
        }

        public static double ReadSimpleDouble(String message)
        {
            String stringValue;
            NSBase.Client.Out.Write(message);
            stringValue = NSBase.Client.Out.ReadLn().ToUpper();
            double value = 0;
            Double.TryParse(stringValue, out value);
            return value;
        }

        public static BooleanCustomFieldRef ReadBooleanCustomFieldValueWithDefault(bool defaultValue)
        {
            var value = new BooleanCustomFieldRef();
            value.value = ReadBooleanSimple("  Value [T/F] (press enter for " + (defaultValue ? "T" : "F") + "): ", defaultValue);
            return value;
        }

        public static String ReadSimpleString(String message)
        {
            NSBase.Client.Out.Write(message);
            return NSBase.Client.Out.ReadLn();
        }

        public static String[] ReadStringArraySimple(String message)
        {
            NSBase.Client.Out.Write(message);
            String values = NSBase.Client.Out.ReadLn();
            string[] valuesParsed = values.Split(new Char[] { ',' }).Select(value => value.Trim()).ToArray();
            return valuesParsed;
        }

        public static MultiSelectCustomFieldRef ReadMultiSelectCustomField(String message)
        {
            // Create a multi select custom field of type MultiSelectCustomFieldRef 
            MultiSelectCustomFieldRef multiSelectCF = new MultiSelectCustomFieldRef();

            // Prompt for values as internal IDs and put into a ListOrRecordRef array
            String[] valuesParsed = ReadStringArraySimple(message);
            ListOrRecordRef[] multiSelectRefArray = new ListOrRecordRef[valuesParsed.Length];

            // For each submitted internal ID, populate a ListOrRecordRef object
            for (int i = 0; i < valuesParsed.Length; i++)
            {
                ListOrRecordRef multiSelectRef = new ListOrRecordRef();
                multiSelectRef.internalId = valuesParsed[i].Trim();
                multiSelectRefArray[i] = multiSelectRef;
            }

            // Add to ArrayList
            multiSelectCF.value = multiSelectRefArray;
            return multiSelectCF;
        }

        public static void PrintInvalidChoice()
        {
            NSBase.Client.Out.Info("\n  Invalid choice. Please select once of the following options.");
        }
    }
}
