using System;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.iOS.TableViewSources;
using CoreGraphics;
using System.Collections.Generic;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Views;
using CodeHub.iOS.Transitions;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class RepositoriesTrendingViewController : BaseTableViewController<RepositoriesTrendingViewModel>
    {
        private readonly TrendingTitleButton _trendingTitleButton = new TrendingTitleButton { Frame = new CGRect(0, 0, 200f, 32f) };

        public RepositoriesTrendingViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Pulse.ToEmptyListImage(), "There are no repositories."));

            NavigationItem.TitleView = _trendingTitleButton;

            OnActivation(d => {
                d(_trendingTitleButton.GetClickedObservable().InvokeCommand(ViewModel.GoToLanguages));
                d(this.WhenAnyValue(x => x.ViewModel.SelectedLanguage)
                    .Select(x => x?.Name ?? "Languages")
                    .BindTo(_trendingTitleButton, x => x.Text));
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
  
            var source = new RepositoryTableViewSource(TableView);
            TableView.Source = source;

            OnActivation(d => 
                d(this.WhenAnyValue(x => x.ViewModel.Repositories)
                .Select(x => x ?? new List<GroupedCollection<RepositoryItemViewModel>>())
                .Select(x => x.Select(g => new TableSectionInformation<RepositoryItemViewModel, RepositoryCellView>(g.Items, RepositoryCellView.Key, (float)UITableView.AutomaticDimension) {
                    Header = new TableSectionHeader(() => CreateHeaderView(g.Name), 26f)
                }))
                .Subscribe(x => source.Data = x.ToList())));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (NavigationController != null)
            {
                NavigationController.NavigationBar.ShadowImage = new UIImage();
                _trendingTitleButton.TintColor = NavigationController.NavigationBar.TintColor;
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (NavigationController != null)
                NavigationController.NavigationBar.ShadowImage = null;
        }

        protected override void HandleNavigation(CodeHub.Core.ViewModels.IBaseViewModel viewModel, UIViewController view)
        {
            if (view is LanguagesViewController)
            {
                var ctrlToPresent = new ThemedNavigationController(view);
                ctrlToPresent.TransitioningDelegate = new SlideDownTransition();
                PresentViewController(ctrlToPresent, true, null);
                viewModel.RequestDismiss.Subscribe(_ => DismissViewController(true, null));
            }
            else
            {
                base.HandleNavigation(viewModel, view);
            }
        }

        private static UILabel CreateHeaderView(string name)
        {
            return new UILabel(new CGRect(0, 0, 320f, 26f)) 
            {
                BackgroundColor = Theme.PrimaryNavigationBarColor,
                Text = name,
                Font = UIFont.BoldSystemFontOfSize(14f),
                TextColor = Theme.PrimaryNavigationBarTextColor,
                TextAlignment = UITextAlignment.Center
            };
        }
    }
}

