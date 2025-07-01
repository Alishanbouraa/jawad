using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace JawadContractingApp.ViewModels
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _hasErrors;

        private readonly Dictionary<string, Queue<string>> _errorMessages;
        private readonly object _errorLock;

        protected BaseViewModel()
        {
            _errorMessages = new Dictionary<string, Queue<string>>();
            _errorLock = new object();
        }

        protected void SetError(string property, string message)
        {
            lock (_errorLock)
            {
                if (!_errorMessages.ContainsKey(property))
                {
                    _errorMessages[property] = new Queue<string>();
                }

                _errorMessages[property].Clear();
                _errorMessages[property].Enqueue(message);
                HasErrors = _errorMessages.Any(kvp => kvp.Value.Count > 0);
            }
        }

        protected void ClearError(string property)
        {
            lock (_errorLock)
            {
                if (_errorMessages.ContainsKey(property))
                {
                    _errorMessages[property].Clear();
                }
                HasErrors = _errorMessages.Any(kvp => kvp.Value.Count > 0);
            }
        }

        protected void ClearAllErrors()
        {
            lock (_errorLock)
            {
                _errorMessages.Clear();
                HasErrors = false;
            }
        }

        protected string GetError(string property)
        {
            lock (_errorLock)
            {
                if (_errorMessages.ContainsKey(property) && _errorMessages[property].Count > 0)
                {
                    return _errorMessages[property].Peek();
                }
                return string.Empty;
            }
        }

        protected async Task ExecuteAsync(Func<Task> operation, string operationName = "")
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = $"جاري {operationName}...";
                ClearAllErrors();

                await operation();

                StatusMessage = "تم بنجاح";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
                SetError("General", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void Execute(Action operation, string operationName = "")
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = $"جاري {operationName}...";
                ClearAllErrors();

                operation();

                StatusMessage = "تم بنجاح";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
                SetError("General", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public virtual void ClearStatus()
        {
            StatusMessage = string.Empty;
            ClearAllErrors();
        }
    }
}