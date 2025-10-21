namespace DevOps.Helpers
{
    internal static class EnumHelper
    {
        public static string GetEnumNamesAsString<T>(string separator) where T : Enum
        {
            return string.Join(separator, GetEnumNamesAsArray<T>());
        }
        public static string[] GetEnumNamesAsArray<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T));
        }
        public static T GetRandomEnum<T>() where T : Enum
        {
            var array = Enum.GetValues(typeof(T));
            return (T)array.GetValue(new Random().Next(array.Length));
        }

        public static T GetEnumValueByString<T>(string enumString) where T : Enum
        {
            foreach (var item in Enum.GetValues(typeof(T)).Cast<T>())
            {
                if (item.ToString() == enumString)
                    return item;
            }
            return default;
        }

    }
}
