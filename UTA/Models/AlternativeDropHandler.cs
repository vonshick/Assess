using System;
using System.Collections;
using System.Windows;
using DataModel.Input;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace UTA.Models
{
    public class AlternativeDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Alternative)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.SourceCollection.Equals(dropInfo.TargetCollection)) return;

            var sourceItem = (Alternative) dropInfo.Data;

            IList sourceList = dropInfo.DragInfo.SourceCollection.TryGetList();
            sourceList.Remove(sourceItem);

            IList targetList = dropInfo.TargetCollection.TryGetList();
            targetList.Add(sourceItem);
        }
    }
}
