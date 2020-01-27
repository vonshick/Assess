using System;
using Assess.Helpers;

namespace Assess.Models.Tab
{
    public class Tab : ITab
    {
        public Tab()
        {
            CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty), _ => IsCloseable);
        }

        public bool IsCloseable { get; set; } = true;

        public string Name { get; set; }
        public RelayCommand CloseCommand { get; }
        public event EventHandler CloseRequested;
    }
}