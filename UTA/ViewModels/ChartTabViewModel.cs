using UTA.Models.DataBase;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
   public class ChartTabViewModel : Tab
   {
      public ChartTabViewModel(Criteria criteria, Alternatives alternatives)
      {
         Name = "Chart";
         Criteria = criteria;
         Alternatives = alternatives;
      }

      private Criteria Criteria { get; }
      private Alternatives Alternatives { get; }
   }
}