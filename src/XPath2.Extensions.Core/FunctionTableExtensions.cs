﻿using System;
using System.Text;
using Wmhelp.XPath2.MS;

namespace Wmhelp.XPath2.Extensions.Core
{
    public static class FunctionTableExtensions
    {
        /// <summary>
        /// Extend the XPath2 FunctionTable with:
        /// - generate-id
        /// - base64encode
        /// - base64decode
        /// </summary>
        /// <param name="functionTable">The function table.</param>
        public static void AddCoreExtensions(this FunctionTable functionTable)
        {
            functionTable.AddGenerateId();
            functionTable.AddBase64Encode();
            functionTable.AddBase64Decode();
        }

        /// <summary>
        /// Extend the XPath2 FunctionTable with 'generate-id' function to generate a Guid (http://www.w3schools.com/xsl/func_generateid.asp)
        /// </summary>
        /// <param name="functionTable">The function table.</param>
        public static void AddGenerateId(this FunctionTable functionTable)
        {
            functionTable.Add(XmlReservedNs.NsXQueryFunc, "generate-id", 0, XPath2ResultType.String, (context, provider, args) => Guid.NewGuid().ToString().ToLower());
        }

        /// <summary>
        /// Extend the XPath2 FunctionTable with 'base64encode' function to encode a string to base64 string.
        /// </summary>
        /// <param name="functionTable">The function table.</param>
        public static void AddBase64Encode(this FunctionTable functionTable)
        {
            string Base64EncodeDelegate(XPath2Context context, IContextProvider provider, object[] args)
            {
                string value = CoreFuncs.CastToStringExactOne(context, args[0]);
                Encoding encoding = args.Length == 2 ? ParseEncodingFromArg(context, args[1]) : Encoding.UTF8;

                try
                {
                    return Convert.ToBase64String(encoding.GetBytes(value));
                }
                catch (Exception ex)
                {
                    throw new XPath2Exception("InvalidFormat", ex);
                }
            }

            // base64encode with default UTF-8 encoding
            functionTable.Add(XmlReservedNs.NsXQueryFunc, "base64encode", 1, XPath2ResultType.String, Base64EncodeDelegate);

            // base64encode with specified encoding
            functionTable.Add(XmlReservedNs.NsXQueryFunc, "base64encode", 2, XPath2ResultType.String, Base64EncodeDelegate);
        }

        /// <summary>
        /// Extend the XPath2 FunctionTable with 'base64decode' function to decode a base64 string to a string.
        /// </summary>
        /// <param name="functionTable">The function table.</param>
        public static void AddBase64Decode(this FunctionTable functionTable)
        {
            string Base64DecodeDelegate(XPath2Context context, IContextProvider provider, object[] args)
            {
                bool fixPadding = true;
                Encoding encoding = Encoding.UTF8;
                string value = CoreFuncs.CastToStringExactOne(context, args[0]);

                if (args.Length == 2)
                {
                    try
                    {
                        // first try to cast to bool
                        fixPadding = CoreFuncs.GetBooleanValue(args[1]);
                    }
                    catch (Exception)
                    {
                        // else parse as encoding
                        encoding = ParseEncodingFromArg(context, args[1]);
                    }
                }

                if (args.Length == 3)
                {
                    encoding = ParseEncodingFromArg(context, args[1]);
                    fixPadding = CoreFuncs.GetBooleanValue(args[2]);
                }

                if (fixPadding)
                {
                    value = value.Trim('=');
                    int mod = value.Length % 4;
                    if (mod != 0)
                    {
                        value = string.Concat(value, new string('=', 4 - mod));
                    }
                }

                try
                {
                    return encoding.GetString(Convert.FromBase64String(value));
                }
                catch (Exception ex)
                {
                    throw new XPath2Exception("InvalidFormat", ex.Message);
                }
            }

            // base64decode with default UTF-8 encoding
            functionTable.Add(XmlReservedNs.NsXQueryFunc, "base64decode", 1, XPath2ResultType.String, Base64DecodeDelegate);

            // base64decode with specified encoding (string) or fixPadding (bool)
            functionTable.Add(XmlReservedNs.NsXQueryFunc, "base64decode", 2, XPath2ResultType.String, Base64DecodeDelegate);

            // base64decode with specified encoding (string) and fixPadding (bool)
            functionTable.Add(XmlReservedNs.NsXQueryFunc, "base64decode", 3, XPath2ResultType.String, Base64DecodeDelegate);
        }

        #region private helper methods
        private static Encoding ParseEncodingFromArg(XPath2Context context, object arg)
        {
            string name = CoreFuncs.CastToStringOptional(context, arg);

            try
            {
                return Encoding.GetEncoding(name);
            }
            catch (Exception)
            {
                throw new XPath2Exception("FORG0001", Properties.Resources.FORG0001, name, "Encoding.GetEncoding()");
            }
        }
        #endregion
    }
}