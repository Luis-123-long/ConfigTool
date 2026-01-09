using System;
using System.Collections.ObjectModel; // 【必须引用这个】
using System.ComponentModel;

namespace LabVIEWConfigSecurity // 【改命名空间】
{
    public class MachineConfig
    {
        public MachineConfig()
        {
            // 初始化！防止空引用
            RightStation = new StationConfig();
            LeftStation = new StationConfig();
            CompensationList = new ObservableCollection<CompensationItem>();
        }

        public StationConfig RightStation { get; set; }
        public StationConfig LeftStation { get; set; }

        // 补偿参数列表 (注意：CompensationItem 必须已经在第一步添加进 DLL 了)
        public ObservableCollection<CompensationItem> CompensationList { get; set; }
    }
}