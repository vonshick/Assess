using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DataModel.PropertyChangedExtended;
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

        public Criteria Criteria { get; }
        private Alternatives Alternatives { get; }

        public void CriterionRenamed(object sender, PropertyChangedEventArgs e)
        {
            var eExtended = (PropertyChangedExtendedEventArgs<string>) e;
            Alternatives.UpdateCriteriaValueName(eExtended.OldValue, eExtended.NewValue);
        }

        public void DataGridUnloaded(object sender, RoutedEventArgs e)
        {
            var grid = (DataGrid) sender;
            if (!grid.IsReadOnly)
                RemovePlaceholder();
            grid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        public void AddPlaceholder()
        {
            Criteria.AddNewPlaceholderToCollection();
        }

        public void SaveCurrentPlaceholder()
        {
            //remove and save again will regenearte row and remove it's styling as placeholder
            RemovePlaceholder();
            var savedCriterion = Criteria.SaveCurrentPlaceholderToCollection();
            Alternatives.AddNewCriterionToAlternatives(savedCriterion.Name, null); //add crit values
            savedCriterion.PropertyChanged += CriterionRenamed;
            //add new placeholder
            AddPlaceholder();
        }

        public void RemovePlaceholder()
        {
            Criteria.RemovePlaceholderFromCollection();
        }
    }
}