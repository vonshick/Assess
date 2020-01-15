using System.ComponentModel;

namespace DataModel.PropertyChangedExtended
{
    public class PropertyChangedExtendedEventArgs<T> : PropertyChangedEventArgs
    {
        public PropertyChangedExtendedEventArgs(string propertyName, T oldValue, T newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public virtual T OldValue { get; }
        public virtual T NewValue { get; }
    }
}