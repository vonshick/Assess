namespace UTA.Models.DataValidation
{
    internal static class DataValidation
    {
        public static bool StringsNotEmpty(params string[] strings)
        {
            if (strings.Length == 0) return false;
            foreach (var str in strings)
                if (str is null || str.Equals(""))
                    return false;
            return true;
        }
    }
}