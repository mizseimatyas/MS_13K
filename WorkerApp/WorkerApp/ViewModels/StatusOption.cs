using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WorkerApp.ViewModels
{
    public class StatusOption : ViewModelBase
    {
        public string Name { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        private bool _isPast;
        public bool IsPast
        {
            get => _isPast;
            set { _isPast = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsClickable)); }
        }

        public bool IsClickable => !IsPast;

        public Action<string>? OnSelect { get; set; }
        public ICommand SelectCommand { get; }

        public StatusOption(string name)
        {
            Name = name;
            SelectCommand = new Utils.RelayCommand(_ =>
            {
                OnSelect?.Invoke(Name);
                return Task.CompletedTask;
            });
        }
    }
}
