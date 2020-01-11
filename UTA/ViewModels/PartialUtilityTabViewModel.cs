using DataModel.Input;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class PartialUtilityTabViewModel : Tab
    {
        public PartialUtilityTabViewModel(Criterion criterion)
        {
            Criterion = criterion;
            Name = "Dialogue - " + criterion.Name;
        }

        public Criterion Criterion { get; }
    }
}