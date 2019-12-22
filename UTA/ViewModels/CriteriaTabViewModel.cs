using UTA.Models.DataBase;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class CriteriaTabViewModel : Tab
    {
        public CriteriaTabViewModel(Criteria criteria, Alternatives alternatives)
        {
            Name = "Criteria";
            Criteria = criteria;
            Alternatives = alternatives;
        }

        private Criteria Criteria { get; }
        private Alternatives Alternatives { get; }
    }
}