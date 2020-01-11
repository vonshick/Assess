using System.Windows;
using DataModel.Input;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace UTA.Helpers
{
    public class AlternativeDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (!(dropInfo.Data is Alternative)) return;
            dropInfo.Effects = DragDropEffects.Move;
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.SourceCollection.Equals(dropInfo.TargetCollection)) return;

            var droppedAlternative = (Alternative) dropInfo.Data;
            var targetList = dropInfo.TargetCollection.TryGetList();
            if (droppedAlternative.ReferenceRank != null) // from rank
            {
                var sourceList = dropInfo.DragInfo.SourceCollection.TryGetList();
                if (!(((FrameworkElement) dropInfo.VisualTarget).Tag is string targetElementTag)) return; // return if from rank to NewRank
                if (targetElementTag == "Alternatives") // from rank to alternatives
                {
                    droppedAlternative.ReferenceRank = null;
                    sourceList.Remove(droppedAlternative);
                }
                else if (targetElementTag == "Rank") // from rank to other rank
                {
                    sourceList.Remove(droppedAlternative);
                    targetList.Add(droppedAlternative);
                }
            }
            else // from alternatives to rank
            {
                targetList.Add(droppedAlternative);
            }
        }
    }
}