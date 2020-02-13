// Copyright © 2020 Tomasz Pućka, Piotr Hełminiak, Marcin Rochowiak, Jakub Wąsik

// This file is part of Assess Extended.

// Assess Extended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.

// Assess Extended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Assess Extended.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DataModel.Annotations;

namespace DataModel.Input
{
    public class Alternative : INotifyPropertyChanged
    {
        private ObservableCollection<CriterionValue> _criteriaValuesList;
        private string _name;
        public string ID;


        public Alternative()
        {
            CriteriaValuesList = new ObservableCollection<CriterionValue>();
        }

        public Alternative(string name, IEnumerable<Criterion> criteriaCollection)
        {
            Name = name;
            CriteriaValuesList = new ObservableCollection<CriterionValue>();
            foreach (var criterion in criteriaCollection) AddCriterionValue(new CriterionValue(criterion.Name, null));
        }


        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        // order of criteria in this list has to be the same as in Criteria.CriteriaCollection
        // otherwise it will break some watchers in Alternatives.cs
        public ObservableCollection<CriterionValue> CriteriaValuesList
        {
            get => _criteriaValuesList;
            set
            {
                _criteriaValuesList = value;
                OnPropertyChanged(nameof(CriteriaValuesList));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void AddCriterionValue(CriterionValue criterionValue)
        {
            CriteriaValuesList.Add(criterionValue);
        }

        public void RemoveCriterionValue(string criterionName)
        {
            var criterionValueToRemove = CriteriaValuesList.First(criterionValue => criterionValue.Name == criterionName);
            CriteriaValuesList.Remove(criterionValueToRemove);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}