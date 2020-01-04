using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Input;
using UTA.Annotations;

namespace UTA.Models.DataBase
{
    public class Criteria : INotifyPropertyChanged
    {
        private ObservableCollection<Criterion> _criteriaCollection;


        public Criteria()
        {
            CriteriaCollection = new ObservableCollection<Criterion>();
        }


        public ObservableCollection<Criterion> CriteriaCollection
        {
            get => _criteriaCollection;
            set
            {
                if (Equals(value, _criteriaCollection)) return;
                _criteriaCollection = value;
                OnPropertyChanged(nameof(CriteriaCollection));
            }
        }

        public Criterion Placeholder { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public Criterion AddCriterion(string criterionName, string criterionDescription, string criterionDirection, int linearSegments)
        {
            Console.WriteLine("Added criterion params " + criterionName);
            var criterion = new Criterion(criterionName, criterionDescription, criterionDirection, linearSegments);
//            if (CriteriaCollection.Contains(criterion)) return null;
            CriteriaCollection.Add(criterion);
            return criterion;
        }

        public void RemoveCriterion(Criterion criterion)
        {
            //todo update alternatives!
            CriteriaCollection.Remove(criterion);
        }

        public Criterion SaveCurrentPlaceholderToCollection()
        {
            CriteriaCollection.Add(Placeholder);
            return Placeholder;
        }

        public void AddNewPlaceholderToCollection()
        {
            Console.WriteLine("Added crit placeholder");
            Placeholder = AddCriterion("name", "description", "Gain", 1);
        }

        public void RemovePlaceholderFromCollection()
        {
            Console.WriteLine("Removed crit placeholder");
            CriteriaCollection.Remove(Placeholder);
        }

        //todo handle name changes: change column bindings and headers, CriteriaValues in alternatives, etc

        public void Reset()
        {
            CriteriaCollection.Clear();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}