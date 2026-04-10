using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TeklaMcp.Server.Logging;

namespace TeklaMcp.Server
{
    public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private McpHostedServer? _server;
        private bool _isRunning;
        private string _statusText = "Stopped";
        private string _connectionStatus = "—";

        public ObservableCollection<string> LogEntries { get; } = new ObservableCollection<string>();

        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotRunning));
            }
        }

        public bool IsNotRunning => !_isRunning;

        public string StatusText
        {
            get => _statusText;
            private set { _statusText = value; OnPropertyChanged(); }
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            private set { _connectionStatus = value; OnPropertyChanged(); }
        }

        public string ServerUrl => "http://127.0.0.1:51234";

        public ICommand StartCommand   { get; }
        public ICommand StopCommand    { get; }
        public ICommand ClearLogCommand { get; }

        public MainViewModel()
        {
            StartCommand    = new RelayCommand(_ => Start(),        _ => !_isRunning);
            StopCommand     = new RelayCommand(_ => Stop(),         _ => _isRunning);
            ClearLogCommand = new RelayCommand(_ => LogEntries.Clear());

            McpAuditLog.EntryWritten += OnAuditEntry;

            AppendLog("Ready. Click Start to launch the MCP server.");
        }

        private void Start()
        {
            try
            {
                StatusText = "Connecting to Tekla…";
                ConnectionStatus = "Connecting…";

                _server = new McpHostedServer();
                _server.Start();

                IsRunning = true;
                StatusText = "Running";
                ConnectionStatus = "Connected";
                AppendLog($"[{Ts}] Server started → {ServerUrl}");
            }
            catch (Exception ex)
            {
                StatusText = "Error";
                ConnectionStatus = "Failed";
                IsRunning = false;
                AppendLog($"[{Ts}] ERROR: {ex.Message}");
                MessageBox.Show(ex.Message, "Start failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Stop()
        {
            try
            {
                _server?.Stop();
                _server = null;
                IsRunning = false;
                StatusText = "Stopped";
                ConnectionStatus = "—";
                AppendLog($"[{Ts}] Server stopped.");
            }
            catch (Exception ex)
            {
                AppendLog($"[{Ts}] ERROR during stop: {ex.Message}");
            }
        }

        private void OnAuditEntry(string json)
        {
            Application.Current?.Dispatcher.BeginInvoke(
                new Action(() => AppendLog("[TOOL] " + json)));
        }

        private void AppendLog(string line)
        {
            if (LogEntries.Count > 500) LogEntries.RemoveAt(0);
            LogEntries.Add(line);
        }

        private static string Ts => DateTime.Now.ToString("HH:mm:ss");

        public void Dispose()
        {
            McpAuditLog.EntryWritten -= OnAuditEntry;
            _server?.Stop();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    internal sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute    = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? p) => _canExecute?.Invoke(p) ?? true;
        public void Execute(object? p)    => _execute(p);

        public event EventHandler? CanExecuteChanged
        {
            add    => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
