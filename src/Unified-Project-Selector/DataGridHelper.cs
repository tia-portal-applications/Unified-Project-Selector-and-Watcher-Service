using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataGridHelper
{
    public static class DataGridHelper
    {
        public static bool GetSelectOnClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectOnClickProperty);
        }

        public static void SetSelectOnClick(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectOnClickProperty, value);
        }

        public static readonly DependencyProperty SelectOnClickProperty =
            DependencyProperty.RegisterAttached("SelectOnClick", typeof(bool), typeof(DataGridHelper), new PropertyMetadata(false, OnSelectOnClickChanged));

        private static void OnSelectOnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if ((bool)e.NewValue)
                {
                    dataGrid.PreviewMouseDown += DataGrid_PreviewMouseDown;
                }
                else
                {
                    dataGrid.PreviewMouseDown -= DataGrid_PreviewMouseDown;
                }
            }
        }

        private static void DataGrid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                if (e.OriginalSource is DependencyObject source)
                {
                    // Find the DataGridRow
                    DataGridRow row = null;
                    while (row == null && source != null)
                    {
                        row = source as DataGridRow;
                        source = VisualTreeHelper.GetParent(source);
                    }

                    if (row != null)
                    {
                        // Toggle the IsSelected property of the DataGridRow
                        row.IsSelected = !row.IsSelected;
                    }
                }
            }
        }
    }

}
