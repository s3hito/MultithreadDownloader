using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadDownloader
{
    public static class Extensions
    {
        static public string GetDescription(this Enum enumVal)
        {
            FieldInfo field = enumVal.GetType().GetField(enumVal.ToString());
            
            if (field == null)
            {
                return enumVal.ToString();
            }
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }
            return enumVal.ToString();
        }
    }
}
