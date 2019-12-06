using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DataModel.Input;

namespace UTA.Models.DataBase
{
    public class Alternatives
    {
        public ObservableCollection<Alternative> AlternativesCollection { get; set; }

        public Alternatives()
        {
            AlternativesCollection = new ObservableCollection<Alternative>();
        }

        private bool ValidateInput()
        {
            //todo validate input not empty etc.
            return true;
        }

        public Alternative AddAlternative(string Name, string Description)
        {
            Alternative alternative = new Alternative(Name, Description);
            AlternativesCollection.Add(alternative);
            return alternative;
        }
    }
}
