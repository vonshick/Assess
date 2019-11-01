using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UTA.Models;
using DataModel.Input;

namespace UTA.ViewModels
{
    public class VariantsList
    { 
        public string InputName { get; set; }
        public string InputDescription { get; set; }
        public ICommand SaveCommand { get; }

        public ObservableCollection<Alternative> VList { get; set; }

        public VariantsList()
        {
            SaveCommand = new RelayCommand(
                param => this.AddVariant(),
                param => this.ValidateInput()
            );
            VList = new ObservableCollection<Alternative>
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
            VList.Add(new Alternative(InputName, InputDescription));
            Console.WriteLine(InputName);
        }
    }
}
