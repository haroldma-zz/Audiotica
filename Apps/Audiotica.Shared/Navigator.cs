#region

using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.View;
using GalaSoft.MvvmLight.Threading;

#endregion

namespace Audiotica
{
    public sealed class Navigator
    {
        private readonly Dictionary<Type, PageBase> _pages = new Dictionary<Type, PageBase>();
        private readonly Canvas _rootContainer;
        private readonly Page _rootPage;

        private readonly Stack<Action> _stack = new Stack<Action>();

        public Navigator(Page rootPage, Canvas rootContainer)
        {
            _rootPage = rootPage;
            _rootContainer = rootContainer;
           _rootContainer.SizeChanged += rootContainer_SizeChanged;
            rootContainer_SizeChanged(null, null);
            CurrentPage = null;
        }

        public PageBase CurrentPage { get; private set; }

        public void AddPage<T>(T page)
            where T : PageBase
        {
            if (_pages.ContainsKey(typeof (T)))
            {
                throw new InvalidOperationException("Pages can only be registered once.");
            }
            _pages.Add(typeof (T), page);

            page.IsHitTestVisible = false;
            page.Width = _rootContainer.ActualWidth;
            page.Height = _rootContainer.ActualHeight;
            Canvas.SetTop(page, 0);
            Canvas.SetLeft(page, 0);
            page.Opacity = 0;
            _rootContainer.Children.Add(page);
        }

        public void Commit(Action action)
        {
            _stack.Push(action);
        }

        public T GetPage<T>()
            where T : PageBase
        {
            if (!_pages.ContainsKey(typeof (T)))
            {
                AddPage(Activator.CreateInstance<T>());
            }
            return _pages[typeof (T)] as T;
        }

        public Boolean GoBack()
        {
            if (_stack.Count == 0)
            {
                return false;
            }
            _stack.Pop().Invoke();
            return true;
        }

        public void GoTo<TPage, TTransition>(Object parameter)
            where TPage : PageBase
            where TTransition : PageTransition, new()
        {
            PageBase page;
            if (CurrentPage != null && CurrentPage.GetType() == typeof (TPage))
            {
                return;
            }
            OnNavigating();
            if (CurrentPage != null)
            {
                CurrentPage.IsHitTestVisible = false;
                CurrentPage.NavigatedFrom(NavigationMode.Forward);
                page = GetPage<TPage>();
                page.BeforeNavigateTo();
                var transition = Activator.CreateInstance<TTransition>();
                transition.FromPage = CurrentPage;
                transition.ToPage = page;
                transition.Play(() =>
                {
                    CurrentPage = page;
                    page.IsHitTestVisible = true;
                    var from = _rootContainer.Children.IndexOf(page);
                    var to = _rootContainer.Children.Count - 1;

                    _rootContainer.Children.Move((uint) from, (uint) to);

                    page.NavigatedTo(parameter);
                    _rootPage.BottomAppBar = page.Bar;
                    OnNavigated();
                });

                if (!(transition.FromPage is FirstRunPage || transition.FromPage is RestorePage))
                {
                    Commit(() => transition.PlayReverse(() =>
                    {
                        CurrentPage = transition.FromPage;

                        var from = _rootContainer.Children.IndexOf(transition.FromPage);
                        var to = _rootContainer.Children.Count - 1;

                        _rootContainer.Children.Move((uint) from, (uint) to);

                        _rootPage.BottomAppBar = transition.FromPage.Bar;
                        transition.ToPage.NavigatedFrom(NavigationMode.Back);
                        transition.ToPage.IsHitTestVisible = false;
                        transition.FromPage.NavigatedTo(null);
                        transition.FromPage.IsHitTestVisible = true;
                    }));
                    return;
                }
            }
            page = GetPage<TPage>();
            page.IsHitTestVisible = true;
            page.BeforeNavigateTo();
            TransitionHelper.Show(page);
            page.NavigatedTo(parameter);
            CurrentPage = page;
            _rootPage.BottomAppBar = page.Bar;

            var fromMain = _rootContainer.Children.IndexOf(page);
            var toMain = _rootContainer.Children.Count - 1;

            _rootContainer.Children.Move((uint) fromMain, (uint) toMain);
            OnNavigated();
        }

        private void OnNavigated()
        {
            var eventHandler = Navigated;
            if (eventHandler != null)
            {
                eventHandler.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnNavigating()
        {
            var eventHandler = Navigating;
            if (eventHandler != null)
            {
                eventHandler.Invoke(this, EventArgs.Empty);
            }
        }

        private void rootContainer_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            foreach (var page in _pages)
            {
                page.Value.SetSize(new Size(_rootContainer.ActualWidth, _rootContainer.ActualHeight));
            }
        }

        public event EventHandler Navigated;

        public event EventHandler Navigating;
    }
}