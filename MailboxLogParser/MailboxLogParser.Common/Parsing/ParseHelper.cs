using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace MailboxLogParser.Common.Parsing
{
    internal class ParseHelper
    {
        #region Get XML Elements

        internal static string GetElementValue(string data, string elementName)
        {
            data = CleanUpData(data);
            XElement root = XElement.Parse(data);

            return GetElementValue(root, elementName);
        }

        internal static string GetElementValue(XElement data, string elementName)
        {
            XElement element = null;
            if (ParseHelper.TryGetXElement(data, elementName, out element))
            {
                return element.Value;
            }

            return String.Empty;
        }

        internal static bool TryGetAttributeOfElement(XElement data, string elementName, string attributeName, out XAttribute attribute)
        {
            attribute = null;
            XElement element = null;
            if (ParseHelper.TryGetXElement(data, elementName, out element))
            {
                if (ParseHelper.TryGetXAttribute(element, attributeName, out attribute))
                {
                    return true;
                }
            }

            return false;
        }

        internal static string GetAttributeValue(XElement data, string attributeName)
        {
            XAttribute attribute = null;
            if (ParseHelper.TryGetXAttribute(data, attributeName, out attribute))
            {
                return attribute.Value;
            }

            return String.Empty;
        }

        internal static bool TryGetXElement(string data, string elementName, out XElement element)
        {
            element = null;

            data = CleanUpData(data);
            XElement root = XElement.Parse(data);

            return TryGetXElement(root, elementName, out element);
        }

        internal static bool TryGetXElement(XElement data, string elementName, out XElement element)
        {
            element = null;

            try
            {
                IEnumerable<XElement> elements =
                    from e in data.Descendants()
                    where e.Name.LocalName == elementName
                    select (XElement)e;

                if (elements.Count() != 1)
                {
                    return false;
                }

                element = elements.First();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(true, "Exception in TryGetXElement: " + ex.GetType().FullName + ", " + ex.Message);
                return false;
            }
        }

        internal static bool TryGetXAttribute(XElement element, string attributeName, out XAttribute attribute)
        {
            attribute = null;

            try
            {
                var attributes =
                    from a in element.Attributes()
                    where a.Name == attributeName
                    select (XAttribute)a;

                if (attributes.Count() != 1)
                {
                    return false;
                }

                attribute = attributes.First();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(true, "Exception in TryGetXAttribute: " + ex.GetType().FullName + ", " + ex.Message);
                return false;
            }
        }

        internal static IEnumerable<XElement> GetXElements(string data, string elementName)
        {
            try
            {
                data = CleanUpData(data);

                XElement root = XElement.Parse(data);

                return GetXElements(root, elementName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(true, "Exception in GetXElements: " + ex.GetType().FullName + ", " + ex.Message);
                return new List<XElement>();
            }
        }

        internal static IEnumerable<XElement> GetXElements(XElement data, string elementName)
        {
            IEnumerable<XElement> elements =
                from e in data.Descendants()
                where e.Name.LocalName == elementName
                select (XElement)e;

            return elements;
        }

        private static string CleanUpData(string data)
        {
            // Clean up 'Body' tags...
            // The Sync response sometimes contains invalid XML.  If there is an Add 
            // the body element may be <Body= 308 bytes>
            if (data.Contains("<Body="))
            {
                data = data.Replace("<Body=", "<Body>");
                data = data.Replace(" bytes/>", "</Body>");
            }

            return data.Trim();
        }

        #endregion

        #region String Parsing

        internal static string ParseStringBetweenTokens(string text, string startToken, params string[] endTokens)
        {
            return ParseStringBetweenTokens(text, true, startToken, endTokens);
        }

        internal static string ParseStringBetweenTokens(string text, bool removeBreaks, string startToken, params string[] endTokens)
        {
            if (!text.Contains(startToken))
            {
                return string.Empty;
            }

            int begin = text.IndexOf(startToken) + startToken.Length;
            int end = text.Length;

            foreach (string endToken in endTokens)
            {
                int i = text.IndexOf(endToken, begin);
                if (i > -1 && i < end)
                {
                    end = text.IndexOf(endToken, begin);
                }
            }

            string found = text.Substring(begin, end - begin);

            if (removeBreaks)
            {
                found = found.Replace("\r", "");
                found = found.Replace("\n", "");
            }

            return found;
        }

        internal static bool TryGetStringBetweenTokens(string text, string token1, string token2, out string found)
        {
            found = string.Empty;

            int i1 = text.IndexOf(token1);
            int i2 = text.IndexOf(token2);


            return false;
        }

        #endregion
    }
}
