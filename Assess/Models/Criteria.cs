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
using System.Runtime.CompilerServices;
using Assess.Properties;
using DataModel.Input;

namespace Assess.Models
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

        public event PropertyChangedEventHandler PropertyChanged;


        public List<Criterion> GetDeepCopyOfCriteria()
        {
            var criteriaDeepCopy = new List<Criterion>();
            foreach (var criterion in CriteriaCollection)
                criteriaDeepCopy.Add(new Criterion(criterion.Name, criterion.Description, criterion.CriterionDirection,
                    criterion.Probability, criterion.Method, criterion.Disabled)
                {
                    MinValue = criterion.MinValue,
                    MaxValue = criterion.MaxValue
                });
            return criteriaDeepCopy;
        }

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