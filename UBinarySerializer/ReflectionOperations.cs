using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullSoftware.Serialization
{
    internal static class ReflectionOperations
    {
        /// <summary>
        /// Determinants whether specified type is derived from list type.
        /// </summary>
        /// <param name="type">Type to check for list.</param>
        /// <returns>true is current type is derived from list type, otherwise false.</returns>
        public static bool CheckIfListType(this Type type)
        {
            return type.IsGenericType && type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));
        }

        /// <summary>
        /// Gets list element type from specified type.
        /// </summary>
        /// <param name="listType">Type that derived from <see cref="IList{T}"/>.</param>
        /// <returns>Element type of <see cref="IList{T}"/>.</returns>
        public static Type GetListItemType(this Type listType)
        {
            Type result = listType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>));

            if (result is null)
                throw new InvalidOperationException($"Failed to determinate collection element type. Collection type: {listType}");

            return result.GenericTypeArguments[0];
        }
    }
}
