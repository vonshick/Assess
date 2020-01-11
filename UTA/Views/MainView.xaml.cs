using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using DataModel.Input;
using MahApps.Metro.Controls.Dialogs;
using UTA.ViewModels;

namespace UTA.Views
{
    public partial class MainView
    {
        private readonly Thickness _menuItemBottomMargin;
        private readonly MainViewModel _viewmodel = new MainViewModel(DialogCoordinator.Instance);
        private RepeatButton _scrollLeftButton;
        private RepeatButton _scrollRightButton;
        private ScrollViewer _tabScrollViewer;
        private StackPanel _tabStackPanel;

        public MainView()
        {
            InitializeComponent();
            DataContext = _viewmodel;

            Loaded += (sender, args) =>
            {
                _viewmodel.ShowTab(_viewmodel.WelcomeTabViewModel);
            };

            _viewmodel.ChartTabViewModels.CollectionChanged += ChartTabsCollectionChanged;
            var tabViewSource = CollectionViewSource.GetDefaultView(TabControl.Items);
            tabViewSource.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add) TabControl.SelectedIndex = _viewmodel.Tabs.Count - 1;
            };

            _menuItemBottomMargin = (Thickness) ShowMenu.FindResource("MenuItemBottomMargin");

            _viewmodel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewmodel.TabToSelect))
                {
                    if (TabControl.SelectedItem == _viewmodel.TabToSelect) BringCurrentTabIntoView();
                    else TabControl.SelectedItem = _viewmodel.TabToSelect;
                }
            };
        }

        private void TabControlHeaderSizeChanged(object sender, EventArgs e)
        {
            if (_tabScrollViewer == null) _tabScrollViewer = (ScrollViewer) TabControl.Template.FindName("TabScrollViewer", TabControl);
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

        // used when tab is selected but only part of it is visible
        private void TabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            BringCurrentTabIntoView();
        }

        private void BringCurrentTabIntoView()
        {
            if (_tabStackPanel == null) _tabStackPanel = (StackPanel) TabControl.Template.FindName("TabStackPanel", TabControl);
            ((FrameworkElement) _tabStackPanel.Children[TabControl.SelectedIndex]).BringIntoView();
        }

        private void Expander_Toggled(object sender, RoutedEventArgs e)
        {
            var expander = (Expander) sender;
            var panelGrid = (Grid) expander.Parent;
            var expanderIndex = panelGrid.Children.IndexOf((UIElement) sender);
            panelGrid.RowDefinitions[expanderIndex].Height = expander.IsExpanded
                ? new GridLength(expanderIndex == 0 ? 61 : 39, GridUnitType.Star)
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

        private void InstancePanelAddButtonClicked(object sender, RoutedEventArgs e)
        {
            var clickedElement = (FrameworkElement) sender;
            if (clickedElement.IsDescendantOf(AlternativesExpander))
            {
                _viewmodel.ShowTab(_viewmodel.AlternativesTabViewModel);
                _viewmodel.AlternativesTabViewModel.NameTextBoxFocusTrigger = !_viewmodel.AlternativesTabViewModel.NameTextBoxFocusTrigger;
            }
            else if (clickedElement.IsDescendantOf(CriteriaExpander))
            {
                _viewmodel.ShowTab(_viewmodel.CriteriaTabViewModel);
                _viewmodel.CriteriaTabViewModel.NameTextBoxFocusTrigger = !_viewmodel.CriteriaTabViewModel.NameTextBoxFocusTrigger;
            }
        }

        private void InstancePanelListItemClicked(object sender, RoutedEventArgs e)
        {
            var selectedElement = (FrameworkElement) sender;
            if (selectedElement.IsDescendantOf(AlternativesExpander))
            {
                var selectedAlternative = (Alternative) selectedElement.DataContext;
                _viewmodel.AlternativesTabViewModel.AlternativeIndexToShow = AlternativesListView.Items.IndexOf(selectedAlternative);
                _viewmodel.ShowTab(_viewmodel.AlternativesTabViewModel);
            }
            else if (selectedElement.IsDescendantOf(CriteriaExpander))
            {
                var selectedCriterion = (Criterion) selectedElement.DataContext;
                _viewmodel.CriteriaTabViewModel.CriterionIndexToShow = CriterionListView.Items.IndexOf(selectedCriterion);
                _viewmodel.ShowTab(_viewmodel.CriteriaTabViewModel);
            }
        }

        private void MenuBarTopbarViewToggled(object sender, RoutedEventArgs e)
        {
            var viewTopbar = (MenuItem) sender;
            MainViewGrid.RowDefinitions[0].Height = viewTopbar.IsChecked ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
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

        // updates Show MenuItem with chart tabs and manages right panel expanders
        private void ChartTabsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                FinalRankingExpander.IsExpanded = false;
                ReferenceRankingExpander.IsExpanded = true;

                var itemsToRemove = new List<MenuItem>();
                foreach (var item in ShowMenu.Items)
                    if (item is MenuItem menuItem && (string) menuItem.Tag == "Chart")
                        itemsToRemove.Add(menuItem);
                foreach (var menuItem in itemsToRemove) ShowMenu.Items.Remove(menuItem);

                if (ShowMenu.Items[ShowMenu.Items.Count - 1] is Separator separator) ShowMenu.Items.Remove(separator);
                ((MenuItem) ShowMenu.Items[ShowMenu.Items.Count - 1]).Margin = _menuItemBottomMargin;
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                FinalRankingExpander.IsExpanded = true;
                ReferenceRankingExpander.IsExpanded = false;

                if (ShowMenu.Items[ShowMenu.Items.Count - 1] is MenuItem lastMenuItem)
                {
                    lastMenuItem.Margin = new Thickness(0);
                    if ((string) lastMenuItem.Tag != "Chart") ShowMenu.Items.Add(new Separator());
                }

                var newChartTabViewModel = (ChartTabViewModel) e.NewItems[0];
                var newMenuItem = new MenuItem {Header = newChartTabViewModel.Name, Margin = _menuItemBottomMargin, Tag = "Chart"};
                newMenuItem.Click += (s, args) => _viewmodel.ShowTab(newChartTabViewModel);
                ShowMenu.Items.Add(newMenuItem);
            }
        }

        private void ExitMenuItemClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ApplicationClosing(object sender, CancelEventArgs e)
        {
            if (!_viewmodel.IsThereAnyApplicationProgress) return;
            // cancel exit, because application doesn't wait for async function and closes anyway
            e.Cancel = true;

            var dialogResult = await this.ShowMessageAsync(
                "Quitting application.",
                "Your progress will be lost. Would you like to proceed without saving?",
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "Save",
                    FirstAuxiliaryButtonText = "Cancel",
                    DialogResultOnCancel = MessageDialogResult.FirstAuxiliary,
                    DefaultButtonFocus = MessageDialogResult.Negative,
                    AnimateShow = false,
                    AnimateHide = false
                });

            if (dialogResult == MessageDialogResult.Negative)
            {
                await _viewmodel.SaveMenuItemClicked();
                Application.Current.Shutdown();
            }
            else if (dialogResult == MessageDialogResult.Affirmative)
            {
                Application.Current.Shutdown();
            }
        }

        private void ShowDocumentationDialog(object sender, RoutedEventArgs e)
        {
            new DocumentationDialogView().ShowDialog();
        }

        private void ShowAboutDialog(object sender, RoutedEventArgs e)
        {
            new AboutDialogView().ShowDialog();
        }
    }
}