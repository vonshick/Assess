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

using System.Windows;

namespace Assess.Views
{
    public partial class KeeneyRaiffaFunctionDialogView
    {
        public KeeneyRaiffaFunctionDialogView(string formula)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            DataContext = this;
            Formula = formula;
        }

        public string Formula { get; }
    }
}