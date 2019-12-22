using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using DataModel.Input;
using UTA.Models;
using UTA.ViewModels;

// using ImportModule; // TODO vonshick REMOVE IT AFTER TESTING
// using ExportModule; // TODO vonshick REMOVE IT AFTER TESTING

namespace UTA.Views
{
   public partial class MainView
   {
      private readonly MainViewModel _viewmodel = new MainViewModel();
      private RepeatButton _scrollLeftButton;
      private RepeatButton _scrollRightButton;
      private ScrollViewer _tabScrollViewer;

      public MainView()
      {
//         TODO vonshick REMOVE IT AFTER TESTING
//         string dataDirectoryPath = "C:\\Data";
//         DataLoader dataLoader = SampleImport.ProcessSampleData(dataDirectoryPath); // csv
//         SampleExport.exportXMCDA(dataDirectoryPath, dataLoader.CriterionList, dataLoader.AlternativeList);

            InitializeComponent();
            DataContext = _viewmodel;
            SetBindings();
            _viewmodel.Criteria.CriteriaCollection.CollectionChanged += UpdateAlternativesDataGridColumns;
            ButtonEditAlternatives.Content = "Editing is OFF";
            AlternativesListView.GiveFeedback += OnGiveFeedback;
            var tabViewSource = CollectionViewSource.GetDefaultView(TabControl.Items);
            tabViewSource.CollectionChanged += TabsCollectionChanged;
      }

        protected void OnGiveFeedback(object o, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
            Mouse.SetCursor(Cursors.Hand);
            e.Handled = true;
        }

        private void SetBindings()
        {
            TextBoxAlternativeName.SetBinding(TextBox.TextProperty, new Binding("InputAlternativeName") { Source = this });
            TextBoxAlternativeDescription.SetBinding(TextBox.TextProperty,
                new Binding("InputAlternativeDescription") { Source = this });

            EditAlternativesDataGrid.ItemsSource = _viewmodel.Alternatives.AlternativesCollection;
            //EditAlternativesDataGrid.CellEditEnding += RowEditEnding;
            EditAlternativesDataGrid.AddingNewItem += _viewmodel.AddAlternativeFromDataGrid;

            //            AlternativesListView.SetBinding(ListView.ItemsSourceProperty, new Binding("AlternativesTable") { Source = _viewmodel });

            //todo bind new element from datagrid
            AlternativesListView.ItemsSource = _viewmodel.Alternatives.AlternativesNotRankedCollection;

            //            CriteriaListView.SetBinding(ListView.ItemsSourceProperty, new Binding("CriteriaTable") { Source = _viewmodel });
            //            CriteriaListView.ItemsSource = _viewmodel.Criteria.CriteriaCollection;

            //            RankingListView.SetBinding(ListView.ItemsSourceProperty, new Binding("AlternativesTable") { Source = _viewmodel });
            //            RankingListView2.SetBinding(ListView.ItemsSourceProperty, new Binding("AlternativesTable") { Source = _viewmodel });
            RankingListView1.ItemsSource = _viewmodel.Alternatives.ReferenceRanking.RankingsCollection[0];
            RankingListView2.ItemsSource = _viewmodel.Alternatives.ReferenceRanking.RankingsCollection[1];
            RankingListView3.ItemsSource = _viewmodel.Alternatives.ReferenceRanking.RankingsCollection[2];
            RankingListView4.ItemsSource = _viewmodel.Alternatives.ReferenceRanking.RankingsCollection[3];

            EditCriteriaDataGrid.ItemsSource = _viewmodel.Criteria.CriteriaCollection;
        }

        public void ShowAddCriterionDialog(object sender, RoutedEventArgs routedEventArgs)
        {
            foreach (DataGridColumn dataGridColumn in EditAlternativesDataGrid.Columns)
            {
                Console.WriteLine(dataGridColumn.Header);
            }
            _viewmodel.ShowAddCriterionDialog();
        }

      public string InputAlternativeName { get; set; }
      public string InputAlternativeDescription { get; set; }

      public void ViewmodelPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         switch (e.PropertyName)
         {
            case "AlternativesTable":
//               RenderListViews();
               break;
            case "CriteriaTable":
               //CriteriaListView.View = GenerateGridView(_viewmodel.CriteriaTable);
               break;
            case "TabToSelect":
               TabControl.SelectedItem = _viewmodel.TabToSelect;
               break;
            default:
               throw new Exception("error in prop: " + e.PropertyName);
         }
      }

      private void RenderListViews()
        {
            AlternativesListView.View = GenerateGridView(_viewmodel.AlternativesTable);
//            CriteriaListView.View = GenerateGridView(_viewmodel.CriteriaTable);
//            RankingListView.View = GenerateGridView(_viewmodel.AlternativesTable);
//            RankingListView2.View = GenerateGridView(_viewmodel.AlternativesTable);
        }

        private GridView GenerateGridView(DataTable table)
        {
            GridView view = new GridView();
            foreach (DataColumn column in table.Columns)
            {
                view.Columns.Add(new GridViewColumn()
                {
                    Header = column.ColumnName,
                    DisplayMemberBinding = new Binding(column.ColumnName)
                });
            }
            return view;
        }

        private void RemoveAlternativeButtonClicked(object sender, RoutedEventArgs e)
        {
            if (EditAlternativesDataGrid.SelectedItem is Alternative alternative)
            {
                _viewmodel.Alternatives.RemoveAlternative(alternative);
            }
        }

        private void RemoveAlternativeFromRankButtonClicked(object sender, RoutedEventArgs e)
        {
            var sourceBtn = (Button) sender;
            var alternative = (Alternative) sourceBtn.DataContext;
            _viewmodel.Alternatives.RemoveAlternativeFromRank(alternative);
        }


        private void AddAlternativesDataGridColumn(Criterion criterion, int startingIndex)
        {
            DataGridTextColumn textColumn = new DataGridTextColumn { Binding = new Binding() { Path = new PropertyPath("CriteriaValuesList[" + startingIndex + "].Value"), Mode = BindingMode.TwoWay } };
            EditAlternativesDataGrid.Columns.Add(textColumn);

            BindingProxy bindingProxy = new BindingProxy {Data = criterion};

            //todo not working
            string critResName = "criterion[" + startingIndex + "]";
            EditAlternativesDataGrid.Resources.Add(critResName, bindingProxy);
            //            textColumn.DataContext = (Criterion)header.Resources["criterion"];
            //            textColumn.Header = new Binding() { Path = new PropertyPath("Name"), Source = (Criterion)EditAlternativesDataGrid.Resources[critResName] };
            TextBlock header = new TextBlock();
            header.SetBinding(TextBlock.TextProperty, new Binding() { Path = new PropertyPath("Data.Name"), Source = (BindingProxy)EditAlternativesDataGrid.Resources[critResName]});
            textColumn.Header = header;


            //            var template = new DataTemplate();
            //            template.VisualTree = new FrameworkElementFactory(typeof(TextBlock));
            //            template.VisualTree.SetBinding(TextBlock.TextProperty, new Binding() { Path = new PropertyPath("Name"), Source = criterion });
            //            textColumn.HeaderTemplate = template;

        }

      private void AddAlternative(object sender, RoutedEventArgs e)
      {
         if (DataValidation.StringsNotEmpty(InputAlternativeName, InputAlternativeDescription))
         {
            _viewmodel.AddAlternative(InputAlternativeName, InputAlternativeDescription);
            TextBoxAlternativeName.Clear();
            TextBoxAlternativeDescription.Clear();
            InputAlternativeName = "";
            InputAlternativeDescription = "";
         }
         else
         {
            //todo notify user
         }
      }

      private void EditAlternativesSwitch(object sender, RoutedEventArgs e)
      {
          EditAlternativesDataGrid.IsReadOnly = !EditAlternativesDataGrid.IsReadOnly;
          if (EditAlternativesDataGrid.IsReadOnly)
          {
              //_viewmodel.UpdateAlternatives();
              ButtonEditAlternatives.Content = "Editing is OFF";
              EditAlternativesDataGrid.Columns[0].Visibility = Visibility.Collapsed;
          }
          else
          {
              EditAlternativesDataGrid.Columns[0].Visibility = Visibility.Visible;
              ButtonEditAlternatives.Content = "Editing is ON";
          }
      }

      public void UpdateAlternativesDataGridColumns(object sender, NotifyCollectionChangedEventArgs e)
      {
         int startingIndex = e.NewStartingIndex;
         foreach (Criterion criterion in e.NewItems)
         {
            AddAlternativesDataGridColumn(criterion, startingIndex++);
         }
      }

      /*private void RowEditEnding(object sender, DataGridCellEditEndingEventArgs e)
      {
          //event fired right before edit end so no new values are present
          if (e.EditAction == DataGridEditAction.Commit)
          {
              var column = e.Column as DataGridBoundColumn;
              if (column != null)
              {
                  var bindingPath = (column.Binding as Binding).Path.Path;
                  Console.WriteLine("Edited row " + e.Row.GetIndex() + " col " + bindingPath);

//                    _viewmodel.GenerateAlternativesTable();

                  if (bindingPath == "Col2")
                  {
                      int rowIndex = e.Row.GetIndex();
                      var el = e.EditingElement as TextBox;
                      // rowIndex has the row index
                      // bindingPath has the column's binding
                      // el.Text has the new, user-entered value
                  }
              }
          }
      }*/

      private void TabsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if (e.Action == NotifyCollectionChangedAction.Add) TabControl.SelectedIndex = _viewmodel.Tabs.Count - 1;
         else if (e.Action == NotifyCollectionChangedAction.Remove)
            TabControl.SelectedIndex = _viewmodel.Tabs.Count == e.OldStartingIndex
               ? e.OldStartingIndex - 1
               : e.OldStartingIndex;
      }

      private void TabControlHeaderSizeChanged(object sender, EventArgs e)
      {
         if (_tabScrollViewer == null)
            _tabScrollViewer = (ScrollViewer) TabControl.Template.FindName("TabScrollViewer", TabControl);
         var tabScrollViewerTemplate = _tabScrollViewer.Template;
         if (_scrollLeftButton == null)
            _scrollLeftButton = (RepeatButton) tabScrollViewerTemplate.FindName("ScrollLeftButton", _tabScrollViewer);
         if (_scrollRightButton == null)
            _scrollRightButton = (RepeatButton) tabScrollViewerTemplate.FindName("ScrollRightButton", _tabScrollViewer);

         if (_tabScrollViewer.ScrollableWidth > 0 && _scrollRightButton.Visibility == Visibility.Collapsed)
            _scrollRightButton.Visibility = _scrollLeftButton.Visibility = Visibility.Visible;
         else if (_tabScrollViewer.ScrollableWidth < _scrollLeftButton.ActualWidth + _scrollRightButton.ActualWidth &&
                  _scrollRightButton.Visibility == Visibility.Visible)
            _scrollRightButton.Visibility = _scrollLeftButton.Visibility = Visibility.Collapsed;
      }

      private void Expander_Toggled(object sender, RoutedEventArgs e)
      {
         var expander = (Expander) sender;
         var panelGrid = (Grid) expander.Parent;
         var expanderIndex = panelGrid.Children.IndexOf((UIElement) sender);
         panelGrid.RowDefinitions[expanderIndex].Height = expander.IsExpanded
            ? new GridLength(expanderIndex == 0 ? 55 : 45, GridUnitType.Star)
            : new GridLength((double) FindResource("ExpanderHeaderHeight") + (expanderIndex == 0 ? 2 : 4));
      }

      private void Tabs_Scrolled(object sender, MouseWheelEventArgs e)
      {
         var scrollViewer = (ScrollViewer) sender;
         const int scrollAmount = 3;
         if (e.Delta < 0)
            for (var i = 0; i < scrollAmount; i++)
               scrollViewer.LineRight();
         else
            for (var i = 0; i < scrollAmount; i++)
               scrollViewer.LineLeft();
      }

      private void InstancePanelListItemClicked(object sender, RoutedEventArgs e)
      {
         var selectedListViewItem = MainViewModel.FindParent<ListViewItem>((DependencyObject) sender);
         var alternativesExpander = (Expander) FindName("AlternativesExpander");
         if (alternativesExpander != null && selectedListViewItem.IsDescendantOf(alternativesExpander))
         {
            _viewmodel.ShowTab(_viewmodel.AlternativesTabViewModel);
            // TODO: navigate to selected item after tab open
            return;
         }

         var criteriaExpander = (Expander) FindName("CriteriaExpander");
         if (criteriaExpander != null && selectedListViewItem.IsDescendantOf(criteriaExpander))
            _viewmodel.ShowTab(_viewmodel.CriteriaTabViewModel);
         // TODO: navigate to selected item after tab open
      }

      private void MenuBarTopbarViewToggled(object sender, RoutedEventArgs e)
      {
         var viewTopbar = (MenuItem) sender;
         MainViewGrid.RowDefinitions[0].Height =
            viewTopbar.IsChecked ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
      }

      private void MenuBarPanelViewToggled(object sender, RoutedEventArgs e)
      {
         var viewPanel = (MenuItem) sender;
         string widthResourceKey;
         string minWidthResourceKey;
         int panelColumnIndex;
         GridSplitter columnsGridSplitter;
         if (viewPanel.Header.ToString().ToLower().Contains("instance"))
         {
            widthResourceKey = "LeftPanelWidth";
            minWidthResourceKey = "LeftPanelMinWidth";
            panelColumnIndex = 0;
            columnsGridSplitter = InstanceTabsGridSplitter;
         }
         else
         {
            widthResourceKey = "RightPanelWidth";
            minWidthResourceKey = "RightPanelMinWidth";
            panelColumnIndex = 4;
            columnsGridSplitter = TabsRankingsGridSplitter;
         }

         if (viewPanel.IsChecked)
         {
            var width = ((GridLength) MainViewGrid.FindResource(widthResourceKey)).Value;
            var minWidth = (double) MainViewGrid.FindResource(minWidthResourceKey);
            MainViewGrid.ColumnDefinitions[panelColumnIndex].Width = new GridLength(width);
            MainViewGrid.ColumnDefinitions[panelColumnIndex].MinWidth = minWidth;
            columnsGridSplitter.Visibility = Visibility.Visible;
         }
         else
         {
            MainViewGrid.ColumnDefinitions[panelColumnIndex].MinWidth = 0;
            MainViewGrid.ColumnDefinitions[panelColumnIndex].Width = new GridLength(0);
            columnsGridSplitter.Visibility = Visibility.Collapsed;
         }
      }
   }
}