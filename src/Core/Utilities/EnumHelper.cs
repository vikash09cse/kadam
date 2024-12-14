using System.ComponentModel;
using System.Reflection;

namespace Core.Utilities
{
    public static class EnumHelper<T> where T : Enum
    {
        public static string GetDescription(T enumValue)
        {
            FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute == null ? enumValue.ToString() : attribute.Description;
        }

        public static List<(T Value, string Description)> GetEnumList()
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .Select(e => (e, GetDescription(e)))
                       .ToList();
        }

       
    }
}
