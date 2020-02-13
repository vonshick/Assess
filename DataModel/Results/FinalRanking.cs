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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Annotations;

namespace DataModel.Results
{
    public class FinalRanking : INotifyPropertyChanged
    {
        private ObservableCollection<FinalRankingEntry> _finalRankingCollection;


        public FinalRanking()
        {
            FinalRankingCollection = new ObservableCollection<FinalRankingEntry>();
        }

        public FinalRanking(ObservableCollection<FinalRankingEntry> finalRankingCollection)
        {
            FinalRankingCollection = finalRankingCollection;
        }

        public ObservableCollection<FinalRankingEntry> FinalRankingCollection
        {
            get => _finalRankingCollection;
            set
            {
                _finalRankingCollection = value;
                OnPropertyChanged(nameof(FinalRankingCollection));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}