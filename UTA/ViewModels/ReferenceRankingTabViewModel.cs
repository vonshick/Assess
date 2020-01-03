using System;
using DataModel.Input;
using UTA.Interactivity;
using UTA.Models.DataBase;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class ReferenceRankingTabViewModel : Tab
    {
        public ReferenceRankingTabViewModel(Criteria criteria, Alternatives alternatives)
        {
            Name = "Reference Ranking";
            Criteria = criteria;
            Alternatives = alternatives;
            RemoveRankCommand = new RelayCommand(rank => RemoveRank((int) rank));
            RemoveAlternativeFromRankCommand = new RelayCommand(alternative => RemoveAlternativeFromRank((Alternative) alternative));
        }

        private Criteria Criteria { get; }
        public Alternatives Alternatives { get; }
        public RelayCommand RemoveAlternativeFromRankCommand { get; }
        public RelayCommand RemoveRankCommand { get; }

        public void AddRank()
        {
            Alternatives.ReferenceRanking.AddRank();
        }

        public void RemoveAlternativeFromRank(Alternative alternative)
        {
            Console.WriteLine("Removing alternative " + alternative.Name + " from rank " + alternative.ReferenceRank);
            Alternatives.RemoveAlternativeFromRank(alternative);
            //            GenerateAlternativesTable();
        }

        public void RemoveRank(int rank)
        {
            Alternatives.RemoveRank(rank);
        }
    }
}