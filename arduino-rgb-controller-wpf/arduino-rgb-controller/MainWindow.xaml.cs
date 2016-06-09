using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace arduino_rgb_controller {
    public partial class MainWindow : Window {
        string selectedPort = "";
        SerialPort serialPort = null;
        Thread serialPortReaderThread = null;
        Thread serialPortUpdateThread = null;
        Thread rainbowThread = null;
        int dataSpeed = 50;

        public MainWindow() {
            InitializeComponent();
            cloneMe.Visibility = Visibility.Collapsed;
            cloneMeLog.Visibility = Visibility.Collapsed;

            Closing += (e, sender) => {
                if (serialPort != null) {
                    serialPort.Write("000r000g000b");
                    serialPort.Close();
                }
                if (serialPortReaderThread != null) serialPortReaderThread.Abort();
                if (serialPortUpdateThread != null) serialPortUpdateThread.Abort();
                if (rainbowThread != null) rainbowThread.Abort();
            };

            String[] ports = SerialPort.GetPortNames();
            foreach (String port in ports) {
                Label label = clone(cloneMe);
                label.Content = port;
                label.Background = Brushes.White;
                label.Visibility = System.Windows.Visibility.Visible;
                label.MouseLeftButtonUp += (e, sender) => {
                    foreach (UIElement uiElement in stackPanel.Children) {
                        Label uiLabel = (Label)uiElement;
                        uiLabel.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0068FF"));
                    }
                    selectedPort = (string)((Label)e).Content;
                };
                stackPanel.Children.Add(label);
            }

            if (stackPanel.Children.Count >= 2) {
                Label uiLabel = (Label)stackPanel.Children[1];
                uiLabel.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0068FF"));
                selectedPort = (string)uiLabel.Content;
            }
        }

        public T clone<T>(T obj) {
            string xaml = XamlWriter.Save(obj);
            StringReader stringReader = new StringReader(xaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            return (T)XamlReader.Load(xmlReader);
        }

        private void connect_Click(object sender, RoutedEventArgs e) {
            connect.IsEnabled = false;
            if (selectedPort == "") {
                MessageBox.Show("Select a valid serial port");
                connect.IsEnabled = true;
                return;
            }

            if (serialPort != null) {
                serialPort.Close();
            }

            serialPort = new SerialPort(selectedPort);
            serialPort.Open();
            serialPortReaderThread = new Thread(new ParameterizedThreadStart(delegate {
                while (true) {
                    string data = serialPort.ReadLine();
                    cloneMeLog.Dispatcher.Invoke(new Action(delegate {
                        Label log = clone(cloneMeLog);
                        log.Visibility = Visibility.Visible;
                        log.Content = data;
                        stackPanel2.Children.Add(log);
                    }));
                }
            }));
            serialPortReaderThread.Start();

            serialPortUpdateThread = new Thread(new ParameterizedThreadStart(delegate {
                int rLast = -1, gLast = -1, bLast = -1;
                while (true) {
                    try {
                        r.Dispatcher.Invoke(new Action(delegate() {
                            if ((int)r.Value != rLast) {
                                serialPort.Write(r.Value.ToString("000") + "r");
                                rLast = (int)r.Value;
                            }
                            if ((int)g.Value != gLast) {
                                serialPort.Write(g.Value.ToString("000") + "g");
                                gLast = (int)g.Value;
                            }
                            if ((int)b.Value != bLast) {
                                serialPort.Write(b.Value.ToString("000") + "b");
                                bLast = (int)b.Value;
                            }
                        }));
                    } catch {

                    }

                    Thread.Sleep(dataSpeed);
                }
            }));
            serialPortUpdateThread.Start();
        }

        private void rainbow_Click(object sender, RoutedEventArgs e) {
            if (rainbowThread != null) {
                rainbowThread.Abort();
                rainbowThread = null;
                rainbow.Content = "Rainbow";
                return;
            }

            rainbowThread = new Thread(new ParameterizedThreadStart(delegate {
                double saniye = 0.0;
                while (true) {
                    r.Dispatcher.Invoke(new Action(delegate() {
                        r.Value = Math.Sin(saniye) * 1000;
                        g.Value = Math.Sin(saniye + 2.04) * 1000;
                        b.Value = Math.Sin(saniye + 4.08) * 1000;
                    }));
                    saniye += 0.001 * dataSpeed;
                    Thread.Sleep(dataSpeed);
                }
            }));
            rainbowThread.Start();
            rainbow.Content = "Rainbow (close)";

        }


    }
}
