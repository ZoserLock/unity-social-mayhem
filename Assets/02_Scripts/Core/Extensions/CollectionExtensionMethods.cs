using System.Collections.Generic;
using System.Text;

namespace Zen.Core
{
    public static class CollectionExtensionMethods
    {
        // This method is used to expand a collection into a string. e.g. [1, 2, 3] -> "1, 2, 3"
        public static string Expand<T>(this IEnumerable<T> enumerable)
        {
            var sb = new StringBuilder();
            foreach (var element in enumerable)
            {
                sb.Append(element.ToString());
                sb.Append(", ");
            }
            return sb.ToString();
        }
    }
}