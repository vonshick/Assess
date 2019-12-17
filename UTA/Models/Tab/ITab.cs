using System;
using System.Windows.Input;
using UTA.Interactivity;

namespace UTA.Models.Tab
{
   public interface ITab
   {
      string Name { get; set; }
      RelayCommand CloseCommand { get; }
      event EventHandler CloseRequested;
   }
}