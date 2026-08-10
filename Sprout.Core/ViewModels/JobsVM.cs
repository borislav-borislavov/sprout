using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.Jobs;
using Sprout.Core.Services.Navigation;
using System.Collections.ObjectModel;
using System.Windows;

namespace Sprout.Core.ViewModels
{
    public partial class JobsVM : ObservableObject, IDisposable
    {
        private const string DefaultScript = """
            public override async Task ExecuteAsync(CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
            }
            """;

        private readonly IConfigurationService _configurationService;
        private readonly IJobScheduler _scheduler;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        public string Title => "Jobs";
        public ObservableCollection<JobItemVM> Jobs { get; } = [];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        [NotifyCanExecuteChangedFor(nameof(EditScriptCommand))]
        [NotifyCanExecuteChangedFor(nameof(RunCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopCommand))]
        private JobItemVM? _selectedJob;

        public JobsVM(
            IConfigurationService configurationService,
            IJobScheduler scheduler,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _configurationService = configurationService;
            _scheduler = scheduler;
            _navigationService = navigationService;
            _dialogService = dialogService;

            _scheduler.StatusChanged += Scheduler_StatusChanged;
            Load();
        }

        private bool HasSelection() => SelectedJob is not null;

        private void Load(Guid? selectedID = null)
        {
            Jobs.Clear();
            foreach (var job in _configurationService.Load().Jobs.OrderBy(j => j.Name))
            {
                var item = new JobItemVM(job);
                item.ApplyStatus(_scheduler.GetStatus(job.ID));
                Jobs.Add(item);
            }

            SelectedJob = selectedID.HasValue
                ? Jobs.FirstOrDefault(j => j.ID == selectedID.Value)
                : Jobs.FirstOrDefault();
        }

        [RelayCommand]
        private void Add()
        {
            var config = _configurationService.Load();
            var job = new SproutJobConfiguration
            {
                Name = GetUniqueName(config.Jobs),
                Script = DefaultScript
            };
            config.Jobs.Add(job);

            if (!_configurationService.Save(config))
            {
                _dialogService.ShowError("The job could not be created.");
                return;
            }

            _scheduler.RefreshSchedules();
            Load(job.ID);
        }

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void Save()
        {
            if (SelectedJob is null)
                return;
            if (string.IsNullOrWhiteSpace(SelectedJob.Name))
            {
                _dialogService.ShowWarning("Job name is required.");
                return;
            }
            if (SelectedJob.IsScheduleEnabled && !Quartz.CronExpression.IsValidExpression(SelectedJob.CronExpression))
            {
                _dialogService.ShowWarning("The cron expression is not valid. Example: '0 0/5 * * * ?' runs every 5 minutes.");
                return;
            }

            var config = _configurationService.Load();
            if (config.Jobs.Any(j => j.ID != SelectedJob.ID &&
                string.Equals(j.Name, SelectedJob.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                _dialogService.ShowWarning("Job names must be unique.");
                return;
            }

            var index = config.Jobs.FindIndex(j => j.ID == SelectedJob.ID);
            if (index < 0)
            {
                _dialogService.ShowError("The selected job no longer exists.");
                Load();
                return;
            }

            SelectedJob.ApplyTo(config.Jobs[index]);
            if (!_configurationService.Save(config))
            {
                _dialogService.ShowError("The job could not be saved.");
                return;
            }

            _scheduler.RefreshSchedules();
            _dialogService.ShowMessage("Job saved.", "Jobs");
        }

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void Delete()
        {
            if (SelectedJob is null)
                return;

            var result = _dialogService.ShowMessage(
                $"Delete job '{SelectedJob.Name}'?",
                "Delete job",
                DialogButton.YesNo,
                DialogImage.Warning);
            if (result != DialogResult.Yes)
                return;

            var jobID = SelectedJob.ID;
            var config = _configurationService.Load();
            config.Jobs.RemoveAll(j => j.ID == jobID);
            if (!_configurationService.Save(config))
            {
                _dialogService.ShowError("The job could not be deleted.");
                return;
            }

            _scheduler.Stop(jobID);
            _scheduler.RefreshSchedules();
            Load();
        }

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void EditScript()
        {
            if (SelectedJob is null)
                return;

            var config = _configurationService.Load();
            var job = config.Jobs.FirstOrDefault(j => j.ID == SelectedJob.ID);
            if (job is null)
                return;

            _navigationService.ShowScriptEditor(new JobCompiler(job, _configurationService));
            _scheduler.RefreshSchedules();
            Load(job.ID);
        }

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private async Task RunAsync()
        {
            if (SelectedJob is null)
                return;

            try
            {
                await _scheduler.RunAsync(SelectedJob.ID);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void Stop()
        {
            if (SelectedJob is not null)
                _scheduler.Stop(SelectedJob.ID);
        }

        private void Scheduler_StatusChanged(object? sender, JobStatusChangedEventArgs e)
        {
            void Apply() => Jobs.FirstOrDefault(j => j.ID == e.Status.JobID)?.ApplyStatus(e.Status);

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null || dispatcher.CheckAccess())
                Apply();
            else
                dispatcher.BeginInvoke(Apply);
        }

        private static string GetUniqueName(IEnumerable<SproutJobConfiguration> jobs)
        {
            var names = jobs.Select(j => j.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var number = 1;
            var name = "New job";
            while (names.Contains(name))
                name = $"New job {++number}";
            return name;
        }

        public void Dispose() => _scheduler.StatusChanged -= Scheduler_StatusChanged;
    }

    public partial class JobItemVM : ObservableObject
    {
        public Guid ID { get; }
        public List<string> Usings { get; }
        public string Script { get; }

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private bool _isScheduleEnabled;

        [ObservableProperty]
        private string _cronExpression;

        [ObservableProperty]
        private string _status = "Idle";

        [ObservableProperty]
        private DateTimeOffset? _lastStartedAt;

        [ObservableProperty]
        private DateTimeOffset? _nextRunAt;

        [ObservableProperty]
        private string? _lastError;

        public JobItemVM(SproutJobConfiguration configuration)
        {
            ID = configuration.ID;
            _name = configuration.Name;
            Script = configuration.Script;
            Usings = configuration.Usings;
            _isScheduleEnabled = configuration.IsScheduleEnabled;
            _cronExpression = configuration.CronExpression;
        }

        public void ApplyTo(SproutJobConfiguration configuration)
        {
            configuration.Name = Name.Trim();
            configuration.IsScheduleEnabled = IsScheduleEnabled;
            configuration.CronExpression = CronExpression.Trim();
        }

        public void ApplyStatus(JobRuntimeStatus status)
        {
            Status = status.State.ToString();
            LastStartedAt = status.LastStartedAt;
            NextRunAt = status.NextRunAt;
            LastError = status.LastError;
        }
    }
}
