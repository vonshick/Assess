using System.ComponentModel;

namespace DataModel.PropertyChangedExtended
{
    public interface INotifyPropertyChangedExtended<T>
    {
        event PropertyChangedEventHandler PropertyChanged;
    }

    public delegate void PropertyChangedExtendedEventHandler<T>(object sender, PropertyChangedExtendedEventArgs<T> e);
}
