using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
            Loaded += (sender, args) => { _viewmodel.ShowTab(_viewmodel.WelcomeTabViewModel); };
            _menuItemBottomMargin = (Thickness) ShowMenu.FindResource("MenuItemBottomMargin");

            _viewmodel.PartialUtilityTabViewModels.CollectionChanged += PartialUtilityTabsCollectionChanged;

            var tabViewSource = CollectionViewSource.GetDefaultView(TabControl.Items);
            tabViewSource.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add) TabControl.SelectedIndex = _viewmodel.Tabs.Count - 1;
            };

            _viewmodel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewmodel.TabToSelect))
                {
                    if (TabControl.SelectedItem == _viewmodel.TabToSelect) BringCurrentTabIntoView();
                    else TabControl.SelectedItem = _viewmodel.TabToSelect;
                }
                else if (e.PropertyName == nameof(_viewmodel.CoefficientAssessmentTabViewModel))
                {
                    CoefficientAssessmentTabPropertyChanged();
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

        // useful when tab is added, but it's out of visible range
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
                columnsGridSplitter = TabsResultsGridSplitter;
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

        private void PartialUtilityTabsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (ShowMenu.Items[ShowMenu.Items.Count - 1] is MenuItem lastMenuItem)
                {
                    lastMenuItem.Margin = new Thickness(0);
                    if ((string) lastMenuItem.Tag != "Utility") ShowMenu.Items.Add(new Separator());
                }

                var newPartialUtilityTabViewModel = (PartialUtilityTabViewModel) e.NewItems[0];
                var newMenuItem = new MenuItem
                    {Header = newPartialUtilityTabViewModel.Name, Margin = _menuItemBottomMargin, Tag = "Utility"};
                newMenuItem.Click += (s, args) => _viewmodel.ShowTab(newPartialUtilityTabViewModel);
                ShowMenu.Items.Add(newMenuItem);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                var itemsToRemove = new List<MenuItem>();
                foreach (var item in ShowMenu.Items)
                    if (item is MenuItem menuItem && (string) menuItem.Tag == "Utility")
                        itemsToRemove.Add(menuItem);
                foreach (var menuItem in itemsToRemove) ShowMenu.Items.Remove(menuItem);

                if (ShowMenu.Items[ShowMenu.Items.Count - 1] is Separator separator) ShowMenu.Items.Remove(separator);
                ((MenuItem) ShowMenu.Items[ShowMenu.Items.Count - 1]).Margin = _menuItemBottomMargin;
            }
        }

        private void CoefficientAssessmentTabPropertyChanged()
        {
            if (_viewmodel.CoefficientAssessmentTabViewModel != null)
            {
                if (ShowMenu.Items[ShowMenu.Items.Count - 1] is MenuItem lastMenuItem)
                {
                    lastMenuItem.Margin = new Thickness(0);
                    ShowMenu.Items.Add(new Separator());
                }

                var newMenuItem = new MenuItem
                    {Header = _viewmodel.CoefficientAssessmentTabViewModel.Name, Margin = _menuItemBottomMargin, Tag = "Coefficient"};
                newMenuItem.Click += (s, args) => _viewmodel.ShowTab(_viewmodel.CoefficientAssessmentTabViewModel);
                ShowMenu.Items.Add(newMenuItem);
            }
            else
            {
                foreach (var item in ShowMenu.Items)
                    if (item is MenuItem menuItem && (string) menuItem.Tag == "Coefficient")
                    {
                        ShowMenu.Items.Remove(item);
                        break;
                    }

                if (ShowMenu.Items[ShowMenu.Items.Count - 1] is Separator separator) ShowMenu.Items.Remove(separator);
                ((MenuItem) ShowMenu.Items[ShowMenu.Items.Count - 1]).Margin = _menuItemBottomMargin;
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
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
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

        private async void ShowDocumentation(object sender, RoutedEventArgs e)
        {
            try
            {
                // TODO: change to proper filepath
                Process.Start(@"Documentation.pdf");
            }
            catch (Exception exception)
            {
                await this.ShowMessageAsync("Opening file error.",
                    exception.Message != null
                        ? $"Can't open documentation. An error was encountered with a following message:\n\"{exception.Message}\""
                        : "Can't open documentation.",
                    MessageDialogStyle.Affirmative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "OK",
                        AnimateShow = false,
                        AnimateHide = false,
                        DefaultButtonFocus = MessageDialogResult.Affirmative
                    });
            }
        }

        private void ShowAboutDialog(object sender, RoutedEventArgs e)
        {
            new AboutDialogView().ShowDialog();
        }
    }
}