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

using System;

namespace DataModel.Results
{
    public class PointToAdd : PartialUtilityValues
    {
        private readonly bool _isProcessedHorizontally;
        private readonly double _max;
        private readonly double _min;


        public PointToAdd(double x, double y, double min, double max, bool isProcessedHorizontally) : base(x, y)
        {
            _min = min;
            _max = max;
            _isProcessedHorizontally = isProcessedHorizontally;
        }


        public override double X
        {
            get => _x;
            set
            {
                if (value.Equals(_x)) return;
                if (_isProcessedHorizontally && (value <= _min || value >= _max))
                    throw new ArgumentException($"Value must be greater than {_min} and lower than {_max}.");
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }

        public override double Y
        {
            get => _y;
            set
            {
                if (value.Equals(_y)) return;
                if (!_isProcessedHorizontally && (value <= _min || value >= _max))
                    throw new ArgumentException($"Value must be greater than {_min} and lower than {_max}.");
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }
    }
}