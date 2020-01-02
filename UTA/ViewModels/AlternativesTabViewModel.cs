using System.Collections.Specialized;
using System.Windows.Controls;
using DataModel.Input;
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
            Alternatives.AlternativesCollection.CollectionChanged += AlternativesCollectionChanged;
        }

        public Criteria Criteria { get; }
        public Alternatives Alternatives { get; }

        public void AddAlternative(string name, string description)
        {
            Alternatives.AddAlternative(name, description);
        }

        public void AddAlternativeFromDataGrid(object sender, AddingNewItemEventArgs e)
        {
            var alternative = new Alternative("initName", "initDesc", Criteria.CriteriaCollection);
            e.NewItem = alternative;
        }

        public void AddPlaceholder()
        {
            //avoid new alternative processing i.e. assignment to ranking
            Alternatives.AlternativesCollection.CollectionChanged -= AlternativesCollectionChanged;
            Alternatives.AddPlaceholder();
            Alternatives.AlternativesCollection.CollectionChanged += AlternativesCollectionChanged;
        }

        public void SaveCurrentPlaceholder()
        {
            //remove and add will regenearte row and remove it's styling as placeholder
            RemovePlaceholder();
            Alternatives.SaveCurrentPlaceholder();
            //add new placeholder
            AddPlaceholder();
        }

        public void RemovePlaceholder()
        {
            Alternatives.RemovePlaceholder();
        }

        private void AlternativesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Alternative alternative in e.NewItems)
                    Alternatives.HandleNewAlternativeRanking(alternative);
        }
    }
}