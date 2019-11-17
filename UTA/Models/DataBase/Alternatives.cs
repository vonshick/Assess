using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DataModel.Input;

namespace UTA.Models.DataBase
{
    public class Alternatives
    {
        public ObservableCollection<Alternative> AlternativesList { get; set; }

        public Alternatives()
        {
            AlternativesList = new ObservableCollection<Alternative>();
        }

        private bool ValidateInput()
        {
            //todo validate input not empty etc.
            return true;
        }

        public void AddAlternative(string Name, string Description)
        {
            AlternativesList.Add(new Alternative(Name, Description));
        }
    }
}
