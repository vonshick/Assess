using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataModel.Input;
using UTA.Annotations;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
    public class UserDialogueTabViewModel : Tab, INotifyPropertyChanged
    {
        public UserDialogueTabViewModel(Criterion criterion)
        {
            Criterion = criterion;
            Name = "Dialogue - " + criterion.Name;
        }

        public Criterion Criterion { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}