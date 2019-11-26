using System;
using System.Xml.Linq;

namespace Wox.Plugin.OneNote99
{
    internal static class OneNoteXmlHelper
    {
        public static int? GetPageLevel(XElement element)
        {
            var value = element?.Attribute("pageLevel")?.Value;

            if (value != null && Int32.TryParse(value, out int res))
            {
                return res;
            }

            return null;
        }

        public static string GetId(XElement xElement)
        {
            return xElement.Attribute(OneNoteApi.IDAttribute)?.Value;
        }

        public static string GetName(XElement element) => element.Attribute(OneNoteApi.NameAttribute)?.Value;

        public static string GetFullNameHierarchy(XElement xElement)
        {
            var name = "";
            xElement = xElement?.Parent;
            while (xElement != null)
            {
                name = GetName(xElement) + '\\' + name;
                xElement = xElement.Parent;
            }

            return name.Trim('\\');
        }
    }
}