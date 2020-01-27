using System;
using Assess.Helpers;

namespace Assess.Models.Tab
{
    public interface ITab
    {
        string Name { get; set; }
        RelayCommand CloseCommand { get; }
        event EventHandler CloseRequested;
    }
}