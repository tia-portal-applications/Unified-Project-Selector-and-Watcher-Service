using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unified_Project_Selector
{
    public class ProjectData : INotifyPropertyChanged
    {
        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected)); // Notify that the 'IsSelected' property has changed
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string ProjectNumber { get; set; }
        public string ProjectName { get; set; }
        public string DeviceName { get; set; }
        public string ProjectType { get; set; }
        public string ProjectID { get; set; }
        public string IsAutostart { get; set; }

        public ProjectData() { }
    }
}
