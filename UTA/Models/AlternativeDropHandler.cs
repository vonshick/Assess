using System;
using System.Collections;
using System.Windows;
using DataModel.Input;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace UTA.Models
{
    class AlternativeDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            Alternative sourceItem = dropInfo.Data as Alternative;
//            ExampleItemViewModel targetItem = dropInfo.TargetItem as ExampleItemViewModel;
//            dropInfo.TargetCollection.GetType() == typeof(ListView)

            if (sourceItem != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.SourceCollection.Equals(dropInfo.TargetCollection)) return;

            Alternative sourceItem = dropInfo.Data as Alternative;

            //            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;

            IList sourceList = dropInfo.DragInfo.SourceCollection.TryGetList();
            sourceList.Remove(sourceItem);

            /*if (dropInfo.DragInfo.VisualSource.GetType() == typeof(ListView))
            {
//                Enumerable data = ExtractData(dragInfo.Data);
                
            }
            else if (dropInfo.DragInfo.VisualSource.GetType() == typeof(DataGrid))
            {
                Console.WriteLine(((DataGrid) dropInfo.DragInfo.VisualSource).Name);
            }*/

            IList targetList = dropInfo.TargetCollection.TryGetList();
            targetList.Add(sourceItem);
        }
    }
}
