using System.Windows.Controls;
using System.Windows.Data;

namespace UTA.Models.DataValidation
{
    static class DataValidation
    {
        public static bool StringsNotEmpty(params string[] strings)
        {
            if (strings.Length == 0)
            {
                return false;
            }
            foreach (string str in strings)
            {
                if (str is null || str.Equals(""))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
