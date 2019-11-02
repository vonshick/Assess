using System.Collections.Generic;
using System.Windows.Input;
using UTA.Models;
using UTA.Models.DataBase;

namespace UTA.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            SaveCommand = new RelayCommand(
                param => this.AddCriterion(),
                param => this.ValidateInput()
            );
            Alternatives = new Alternatives();
            Criteria = new Criteria();
            CriteriaTypeList = new List<string>{"Cost", "Gain", "Ordinal"};
        }
        public ICommand SaveCommand { get; }


        public Alternatives Alternatives { get; set; }

        public Criteria Criteria { get; set; }

        public List<string> CriteriaTypeList { get; set; }

        public string InputCriterionType { get; set; }

        public void AddCriterion()
        {
            Criteria.AddCriterion(InputCriterionType);
        }

        private bool ValidateInput()
        {
            //todo validate input not empty etc.
            return true;
        }
    }
}