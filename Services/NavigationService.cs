using System.Windows;
using System.Windows.Controls;

namespace JawadContractingApp.Services
{
    public interface INavigationService
    {
        void NavigateTo(string viewName);
        void SetMainFrame(Frame frame);
        bool CanGoBack { get; }
        void GoBack();
        void ClearHistory();
    }

    public class NavigationService : INavigationService
    {
        private Frame? _mainFrame;
        private readonly Dictionary<string, Type> _viewRegistry;
        private readonly Stack<string> _navigationHistory;
        private readonly object _navigationLock;

        public NavigationService()
        {
            _viewRegistry = new Dictionary<string, Type>();
            _navigationHistory = new Stack<string>();
            _navigationLock = new object();
            RegisterViews();
        }

        public bool CanGoBack => _navigationHistory.Count > 1;

        public void SetMainFrame(Frame frame)
        {
            _mainFrame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public void NavigateTo(string viewName)
        {
            lock (_navigationLock)
            {
                if (_mainFrame == null)
                    throw new InvalidOperationException("Navigation frame not initialized");

                if (!_viewRegistry.ContainsKey(viewName))
                    throw new ArgumentException($"View '{viewName}' not registered");

                var viewType = _viewRegistry[viewName];
                var viewInstance = Activator.CreateInstance(viewType);

                if (viewInstance is UserControl userControl)
                {
                    _mainFrame.Navigate(userControl);
                    _navigationHistory.Push(viewName);
                }
                else
                {
                    throw new InvalidOperationException($"View '{viewName}' must inherit from UserControl");
                }
            }
        }

        public void GoBack()
        {
            lock (_navigationLock)
            {
                if (!CanGoBack) return;

                _navigationHistory.Pop();
                var previousView = _navigationHistory.Peek();

                if (_viewRegistry.ContainsKey(previousView))
                {
                    var viewType = _viewRegistry[previousView];
                    var viewInstance = Activator.CreateInstance(viewType);

                    if (viewInstance is UserControl userControl && _mainFrame != null)
                    {
                        _mainFrame.Navigate(userControl);
                    }
                }
            }
        }

        public void ClearHistory()
        {
            lock (_navigationLock)
            {
                _navigationHistory.Clear();
            }
        }

        private void RegisterViews()
        {
            _viewRegistry["CustomerView"] = typeof(Views.CustomerView);
            _viewRegistry["AccountingView"] = typeof(Views.AccountingView);
        }
    }
}