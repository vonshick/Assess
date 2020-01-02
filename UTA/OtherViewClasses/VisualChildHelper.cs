using System.Windows.Media;

namespace UTA.OtherViewClasses
{
    public static class VisualChildHelper
    {
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default;
            var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < numVisuals; i++)
            {
                var v = (Visual) VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) child = GetVisualChild<T>(v);
                if (child != null) break;
            }

            return child;
        }
    }
}