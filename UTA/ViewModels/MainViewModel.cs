using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using DataModel.Input;
using UTA.Models.DataBase;
using UTA.Views;

namespace UTA.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Alternatives = new Alternatives();
            Criteria = new Criteria();
        }

        public Alternatives Alternatives { get; set; }

        public Criteria Criteria { get; set; }

        public void ShowAddCriterionDialog()
        {
            AddCriterionViewModel addCriterionViewModel = new AddCriterionViewModel() { MainViewModel = this, Criteria = this.Criteria};
            AddCriterionView addCriterionWindow = new AddCriterionView(addCriterionViewModel);
            addCriterionWindow.ShowDialog();
            //            if (!Criteria.AddCriterion(criterionName, criterionDescription, criterionDirection, linearSegments))
            //                return false;
        }

        public void AddAlternative(string name, string description)
        {
            Alternatives.AddAlternative(name, description);
            GenerateAlternativesTable();
        }

        public void GenerateAlternativesTable()
        {
            DataTable table = new DataTable();
            ReloadColumnsAlternatives(table);
            ReloadRowsAlternatives(table);
            AlternativesTable = table;
        }

        private void ReloadColumnsAlternatives(DataTable table)
        {
            table.Columns.Add("Alternative");
            table.Columns.Add("Description");
            foreach (Criterion criterion in Criteria.CriteriaList)
            {

                //todo verify column does not already exist

                table.Columns.Add(criterion.Name);
            }
        }

        private void ReloadRowsAlternatives(DataTable table)
        {
            foreach (Alternative alternative in Alternatives.AlternativesList)
            {
                table.Rows.Add(alternative.Name, alternative.Description);
            }
        }

        private DataTable _alternativesTable;
        public DataTable AlternativesTable
        {
            get { return _alternativesTable; }
            set
            {
                if (_alternativesTable != value)
                {
                    _alternativesTable = value;
                    OnPropertyChanged("AlternativesTable");
                }
            }
        }

        public void GenerateCriteriaTable()
        {
            DataTable table = new DataTable();
            ReloadRowsCriteria(table);
            CriteriaTable = table;
        }

        private void ReloadRowsCriteria(DataTable table)
        {
            table.Columns.Add("Criterion");
            table.Columns.Add("Description");
            table.Columns.Add("Type");
            table.Columns.Add("Linear Segments");
            foreach (Criterion criterion in Criteria.CriteriaList)
            {
                table.Rows.Add(criterion.Name, criterion.Description, criterion.CriterionDirection, criterion.LinearSegments);
            }
        }

        private DataTable _criteriaTable;
        public DataTable CriteriaTable
        {
            get { return _criteriaTable; }
            set
            {
                if (_criteriaTable != value)
                {
                    _criteriaTable = value;
                    OnPropertyChanged("CriteriaTable");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }

    }
}