using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DataModel.Input;

namespace UTA.Models.DataBase
{
    public class Alternatives
    {
        public string InputName { get; set; }
        public string InputDescription { get; set; }
        public ICommand AddButtonCommand { get; }

        public ObservableCollection<Alternative> AlternativesList { get; set; }

        public Alternatives()
        {
            AddButtonCommand = new RelayCommand(
                param => this.AddVariant(),
                param => this.ValidateInput()
            );
            AlternativesList = new ObservableCollection<Alternative>
            {
                new Alternative("variant 1", "desc of var 1"),
                new Alternative("variant 2", "desc of var 2")
            };
        }

        private bool ValidateInput()
        {
            //todo validate input not empty etc.
            return true;
        }

        public void AddVariant()
        {
            AlternativesList.Add(new Alternative(InputName, InputDescription));
            Console.WriteLine(InputName);
        }
    }
}
