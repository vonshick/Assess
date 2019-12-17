using UTA.Models.DataBase;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
   public class AlternativesTabViewModel : Tab
   {
      public AlternativesTabViewModel(Criteria criteria, Alternatives alternatives)
      {
         Name = "Alternatives";
         Criteria = criteria;
         Alternatives = alternatives;
      }

      private Criteria Criteria { get; }
      private Alternatives Alternatives { get; }
   }
}