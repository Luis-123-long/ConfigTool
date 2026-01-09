using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LabVIEWConfigSecurity // 【改命名空间】
{
    
    public class CompensationItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _modelName;
        public string ModelName
        {
            get => _modelName;
            set { _modelName = value; OnPropertyChanged(); }
        }

        private double _value1;
        public double Value1
        {
            get => _value1;
            set { _value1 = value; OnPropertyChanged(); }
        }

        private double _value2;
        public double Value2
        {
            get => _value2;
            set { _value2 = value; OnPropertyChanged(); }
        }
    }
}