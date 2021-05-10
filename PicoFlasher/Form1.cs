using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicoFlasher {
	public partial class Form1 : Form {

        String flash_file = null;
        String uf2_file_path = null;

		public Form1() {
            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length < 2) {
                MessageBox.Show("Missing file argument, you need to supply the ELF file as an argument");
                System.Environment.Exit(1);
            }
            flash_file = arguments[1];
            if (!File.Exists(flash_file)) {
                MessageBox.Show("The supplied file does not exist");
                System.Environment.Exit(1);
            }

            uf2_file_path = flash_file.Substring(0, flash_file.Length - 3)+"uf2";
            if (!File.Exists(uf2_file_path)) {
                MessageBox.Show(uf2_file_path + " The supplied file does not exist");
                System.Environment.Exit(1);
            }

            InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e) {
            timer1.Enabled = true;
            waitAndFlash();
        }

		private void timer1_Tick(object sender, EventArgs e) {
			waitAndFlash();
		}

		private void waitAndFlash() {
            String drive_letter = null;


            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * FROM Win32_DiskDrive")) {
                collection = searcher.Get();
                foreach (var device in collection) {
                    if(device.GetPropertyValue("Model").ToString().Contains("RPI RP2")) {
                        foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + device.GetPropertyValue("DeviceID").ToString() + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get()) {
                            foreach (ManagementObject disk in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get()) {
                                drive_letter = disk.GetPropertyValue("Name").ToString();
                            }
                        }
                    }
                }
            }

            if(drive_letter != null) {
                label1.Text = "Flashing";
                label2.Text = "Copy U2F file to " + drive_letter;
                label1.Location = new Point(this.Width / 2 - label1.Width / 2, label1.Location.Y);
                label2.Location = new Point(this.Width / 2 - label2.Width / 2, label2.Location.Y);

                File.Copy(uf2_file_path, drive_letter+"\\"+Path.GetFileName(uf2_file_path));
                System.Environment.Exit(1);
            }
        }

        static List<USBDeviceInfo> GetUSBDevices() {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity"))
                collection = searcher.Get();

            foreach (var device in collection) {
                devices.Add(new USBDeviceInfo(
                    (string)device.GetPropertyValue("DeviceID"),
                    (string)device.GetPropertyValue("PNPDeviceID"),
                    (string)device.GetPropertyValue("Manufacturer"),
                    (string)device.GetPropertyValue("Description"),
                    device
                ));
            }

            collection.Dispose();
            return devices;
        }
    }

    class USBDeviceInfo {
        public USBDeviceInfo(string deviceID, string pnpDeviceID, string manufacturer, string description, ManagementBaseObject device) {
            this.DeviceID = deviceID != null ? deviceID : "";
            this.PnpDeviceID = pnpDeviceID != null ? deviceID : "";
            this.Manufacturer = manufacturer != null ? manufacturer : "";
            this.Description = description != null ? description : "";
            this.Device = device;
        }
        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }
        public string Manufacturer { get; private set; }
        public ManagementBaseObject Device { get; private set; }
    }
}
