using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace XamarinBLE.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        IBluetoothLE ble;
        IAdapter adapter;
        ObservableCollection<IDeviceModel> _listDevices;
        public ObservableCollection<IDeviceModel> ListDevices
        {
            get => _listDevices;
            set
            {
                if (_listDevices != value)
                {
                    _listDevices = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListDevices"));
                }
            }
        }
        public bool IsShowList => ListDevices.Count > 0;
        string _state;
        public string State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("State"));
                }
            }
        }
        bool _running = false;
        public bool Running
        {
            get => _running;
            set
            {
                if (_running != value)
                {
                    _running = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Running"));
                }
            }
        }
        public MainPageViewModel()
        {
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            _listDevices = new ObservableCollection<IDeviceModel>();
            ListDevices = new ObservableCollection<IDeviceModel>();
            State = string.Format("BLE state: {0}", ble.State.ToString());
            BLEStageChanged();
            ScanDeviceCommand = new Command(ScanDevice);
        }
        public ICommand ScanDeviceCommand { get; set; }
        void BLEStageChanged()
        {
            ble.StateChanged += (s, e) =>
            {
                State = string.Format("BLE state: {0}", e.NewState.ToString());
                Debug.WriteLine($"The bluetooth state changed to {e.NewState}");
            };
        }
        async void ScanDevice()
        {
            if (ble.State != BluetoothState.On) {
                await Application.Current.MainPage.DisplayAlert("", "Vui lòng bật bluetooth để tiếp tục", "Đồng ý");
                return; 
            }
            Debug.WriteLine("..............................Scanning...................");
            Running = true;
            ListDevices.Clear();
            
            List<IDeviceModel> list = new List<IDeviceModel>();
            adapter.DeviceDiscovered += (s, a) => {
                
                IDevice device = a.Device as IDevice;
                if (device != null) {
                    
                    Debug.WriteLine($"device {device.Name}");
                    IDeviceModel item = new IDeviceModel();
                    item.NativeDevice = device.NativeDevice;
                    item.Device = device;
                    list.Add(item);
                    ListDevices = new ObservableCollection<IDeviceModel>(list);
                }
                
            };
            await adapter.StartScanningForDevicesAsync();
            Debug.WriteLine("--------------------Scan devices done ------------------");
            Debug.WriteLine($"Find {ListDevices.Count} devices");
            Running = false;
        }
    }
}
public class NativeDeviceModel
{
    public string Address { get; set; }
}
public class IDeviceModel
{
    public object NativeDevice { get; set; }
    public IDevice Device { get; set; }
}
