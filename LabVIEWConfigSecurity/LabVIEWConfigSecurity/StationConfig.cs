using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LabVIEWConfigSecurity // 【改命名空间】
{
    // 【完全保留你的逻辑】
    public class StationConfig : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- 秒 ---
        private int _second;
        public int Second
        {
            get => _second;
            set { _second = value; OnPropertyChanged(); }
        }

        // --- 分 ---
        private int _minute;
        public int Minute
        {
            get => _minute;
            set { _minute = value; OnPropertyChanged(); }
        }

        // --- 时 ---
        private int _hour;
        public int Hour
        {
            get => _hour;
            set { _hour = value; OnPropertyChanged(); }
        }

        // --- 日 ---
        private int _day = 1;
        public int Day
        {
            get => _day;
            set { _day = value; OnPropertyChanged(); }
        }

        // --- 月 ---
        private int _month = 1;
        public int Month
        {
            get => _month;
            set { _month = value; OnPropertyChanged(); }
        }

        // --- 年 ---
        private int _year = 2025;
        public int Year
        {
            get => _year;
            set { _year = value; OnPropertyChanged(); }
        }
    }
}