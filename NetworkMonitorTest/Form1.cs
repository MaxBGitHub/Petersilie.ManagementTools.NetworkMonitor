using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Petersilie.ManagementTools.NetworkMonitor;
using Petersilie.ManagementTools.NetworkMonitor.Header;

namespace NetworkMonitorTest
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer _viewUpdateTime = new System.Windows.Forms.Timer();
        private void ViewUpdateTimer_PerformUpdate(object sender, EventArgs e)
        {
            if (dgv.InvokeRequired) {
                dgv.Invoke(new Action(() => dgv.Invalidate()));
                dgv.Invoke(new Action(() => dgv.FirstDisplayedScrollingRowIndex = dgv.Rows.Count -1));
            }
            else {
                dgv.Invalidate();
                dgv.FirstDisplayedScrollingRowIndex = dgv.Rows.Count -1 ;
            }            
        }



        private Thread[] _monitorThreads;
        private NetworkMonitor[] _monitors;

        private DataTable _tableSource;

        private void ErrorReceived(object sender, MonitorExceptionEventArgs e)
        {
            var t = new System.Windows.Forms.Timer();
            object[] row = new object[] {
                IPVersion.IPv4,
                Protocol.UNDEFINED,
                System.Net.IPAddress.None,
                System.Net.IPAddress.None,
                (int)e.Error.HResult
            };

            lock (_tableSource)
            {
                _tableSource.Rows.Add(row);
            }
        }


        private void HeaderReceived(object sender, IPHeaderEventArgs e)
        {            
            if (e.Header != null)
            {
                var ipv4Header = e.Header as IPv4Header;
                object[] row = new object[] {
                    ipv4Header.IPVersion,
                    ipv4Header.Protocol,
                    ipv4Header.SourceAddress,
                    ipv4Header.DestinationAddress,
                    ipv4Header.Data.Length
                };

                lock (_tableSource) {
                    _tableSource.Rows.Add(row);
                }
            }
            else
            {
                object[] row = new object[] {
                    IPVersion.IPv4,
                    Protocol.UNDEFINED,
                    e.SocketAddress,
                    System.Net.IPAddress.None,
                    0
                };

                lock (_tableSource) {
                    _tableSource.Rows.Add(row);
                }
            }
        }


        private void StartMonitorThreads()
        {            
            _monitors = NetworkMonitor.BindInterfaces();
            _monitorThreads = new Thread[_monitors.Length];
            for (int i=0; i<_monitors.Length; i++) {
                _monitors[i].IPHeaderReceived += HeaderReceived;
                _monitors[i].OnError += ErrorReceived;
                _monitors[i].Begin();
            }
        }



        public Form1()
        {
            InitializeComponent();

            typeof(DataGridView).GetProperty("DoubleBuffered", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic).SetValue(dgv, true);

            _tableSource = new DataTable("Packets");
            _tableSource.Columns.Add("Version", typeof(IPVersion));
            _tableSource.Columns.Add("Protocol", typeof(Protocol));
            _tableSource.Columns.Add("Source Address", typeof(System.Net.IPAddress));
            _tableSource.Columns.Add("Destination Address", typeof(System.Net.IPAddress));
            _tableSource.Columns.Add("Data length", typeof(int));

            dgv.DataSource = _tableSource;

            _viewUpdateTime.Interval = 200;
            _viewUpdateTime.Tick += ViewUpdateTimer_PerformUpdate;
            _viewUpdateTime.Start();            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartMonitorThreads();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i=0; i<_monitors.Length; i++) {
                _monitors[i].Stop();
                _monitors[i].Dispose();
            }
            textBox2.Text = _tableSource.Rows.Count.ToString();
        }
    }
}
