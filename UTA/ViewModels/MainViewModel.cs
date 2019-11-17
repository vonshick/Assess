using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using DataModel.Input;
using UTA.Models.DataBase;

namespace UTA.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Alternatives = new Alternatives();
            Criteria = new Criteria();
            CriteriaTypeList = Enum.GetNames(typeof(Criterion.Type)).ToList();
        }

        public Alternatives Alternatives { get; set; }

        public Criteria Criteria { get; set; }

        public List<string> CriteriaTypeList { get; set; }

        public void AddCriterion(string typeString, string criterionName, string description, int inputLinearSegments)
        {
            Criteria.AddCriterion(typeString, criterionName, description, inputLinearSegments);
            GenerateCriteriaTable();
            GenerateAlternativesTable();
        }

        public void AddAlternative(string name, string description)
        {
            Alternatives.AddAlternative(name, description);
            GenerateAlternativesTable();
        }

        private bool ValidateInput()
        {
            //todo validate input not empty etc.
            return true;
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
                table.Rows.Add(criterion.Name, criterion.Description, criterion.CriterionType, criterion.LinearSegments);
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