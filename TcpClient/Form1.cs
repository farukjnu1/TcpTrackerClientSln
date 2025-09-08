using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Data.SqlClient;

namespace TcpTrackerClient
{
    public partial class Form1 : Form
    {
        TcpClient tcpClient;
        bool isConnectedToServer = false;
        private StreamWriter strWritter;
        private StreamReader strReader;
        private Thread incomingMessageHandler;

        public Form1()
        {
            InitializeComponent();
        }

       

        void getResponse()
        {
            Stream stm = tcpClient.GetStream();
            byte[] bb = new byte[100];
            int k = stm.Read(bb, 0, 100);

            string msg = "";

            for (int i = 0; i < k; i++)
            {
                msg += Convert.ToChar(bb[i]).ToString();

            }

            setClientMessage(msg);
        }


        void setClientMessage(string msg)
        {

            if (!InvokeRequired)
            {
                listBox1.Items.Add(msg.ToString());


            }
            else
            {
                Invoke(new Action<string>(setClientMessage), msg);
            }
        }

        private void SendMsgButton_Click(object sender, EventArgs e)
        {
            string message;
            message = "SDx;" + totextbox.Text + ";" + messagebodytextbox.Text.Trim();
            strWritter.WriteLine(message);
            strWritter.Flush();

        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (!isConnectedToServer)
            {
                string connectionEstablish;

                connectionEstablish = "ATx;" + userNameTextbox.Text.Trim();

                tcpClient = new TcpClient();
                tcpClient.Connect(serveripTextbox.Text.Trim(), int.Parse(portTextbox.Text.Trim()));
                this.isConnectedToServer = true;

                strWritter = new StreamWriter(tcpClient.GetStream());
                strWritter.WriteLine(connectionEstablish);
                strWritter.Flush();

                incomingMessageHandler = new Thread(() => ReceiveMessages());
                incomingMessageHandler.IsBackground = true;
                incomingMessageHandler.Start();

                ConnectButton.Text = "Connected";
            }
            else
            {
                MessageBox.Show("Already connected");
            }

        }

        
        private void ReceiveMessages()
        {
            

            strReader = new StreamReader(this.tcpClient.GetStream());

            // While we are successfully connected, read incoming lines from the server
            while (this.isConnectedToServer)
            {
                string serverResponse = strReader.ReadLine();
                string[] data = serverResponse.Split(';');

                if (data[0].Equals("RDx"))
                {
                    string source = data[1];
                    string message = data[2];

                    setClientMessage(source + " Says:" + " " + message);
                    if (message.LastIndexOf("FF") > 0 && message.Contains(','))
                    {
                        // save in database
                        if (message.Split(',').Count() == 11)
                        {
                            SqlConnection con;
                            SqlCommand cmd;
                            string conStr = "Data Source=localhost;Initial Catalog=TwoStepDb;User ID=sa;Password=123";

                            // delete
                            con = new SqlConnection(conStr);
                            cmd = new SqlCommand();
                            cmd.Connection = con;
                            con.Open();
                            cmd.CommandText = "DELETE FROM [dbo].[Tbl_TrackLast] WHERE Tid = @Tid";
                            cmd.Parameters.Add(new SqlParameter("Tid", source));
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            con.Close();

                            //insert
                            con = new SqlConnection(conStr);
                            cmd = new SqlCommand();
                            cmd.Connection = con;
                            con.Open();
                            cmd.CommandText = "INSERT INTO [dbo].[Tbl_TrackLast] VALUES(@Tid, @Lat, @Lon, @Direction, @GpsSignal, @Speed, @Temperature, @GsmSignal, @ReceiveTime, @CarState, @TeState, @AlarmState, @Live)";
                            cmd.Parameters.Add(new SqlParameter("Tid", source));
                            cmd.Parameters.Add(new SqlParameter("Lat", message.Split(',')[0]));
                            cmd.Parameters.Add(new SqlParameter("Lon", message.Split(',')[1]));
                            cmd.Parameters.Add(new SqlParameter("Direction", message.Split(',')[2]));
                            cmd.Parameters.Add(new SqlParameter("GpsSignal", message.Split(',')[3]));
                            cmd.Parameters.Add(new SqlParameter("Speed", message.Split(',')[4]));
                            cmd.Parameters.Add(new SqlParameter("Temperature", message.Split(',')[5]));
                            cmd.Parameters.Add(new SqlParameter("GsmSignal", message.Split(',')[6]));
                            cmd.Parameters.Add(new SqlParameter("ReceiveTime", DateTime.Now));
                            cmd.Parameters.Add(new SqlParameter("CarState", message.Split(',')[7]));
                            cmd.Parameters.Add(new SqlParameter("TeState", message.Split(',')[8]));
                            cmd.Parameters.Add(new SqlParameter("AlarmState", message.Split(',')[9]));
                            cmd.Parameters.Add(new SqlParameter("Live", true));
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            con.Close();

                            //insert
                            if (source.Contains('-'))
                            {
                                source = "Track__" + source;
                            }
                            else
                            {
                                source = "Track_" + source;
                            }

                            //select
                            double lat = 0;
                            double lon = 0;
                            int calMileage = 0; 
                            int prevMilage = 0;
                            SqlDataReader rd;
                            con = new SqlConnection(conStr);
                            cmd = new SqlCommand();
                            cmd.Connection = con;
                            con.Open();
                            cmd.CommandText = "SELECT TOP 1 Id, Lat, Lon, Mileage FROM " + source + " ORDER BY Id DESC";
                            rd = cmd.ExecuteReader();
                            rd.Read();
                            if (rd.HasRows)
                            {
                                lat = Convert.ToDouble(rd.GetValue(1));
                                lon = Convert.ToDouble(rd.GetValue(2));
                                prevMilage = rd.GetInt32(3);
                            }
                            cmd.Dispose();
                            rd.Close();
                            con.Close();

                            calMileage = (int)DirectDistance(lat, lon, Convert.ToDouble(message.Split(',')[0]), Convert.ToDouble(message.Split(',')[1]));
                            if (calMileage > -1)
                            {
                                calMileage = calMileage + prevMilage;

                                con = new SqlConnection(conStr);
                                cmd = new SqlCommand();
                                cmd.Connection = con;
                                con.Open();
                                cmd.CommandText = "INSERT INTO " + source + " VALUES (@Lat, @Lon, @Direction, @GpsSignal, @Speed, @Temperature, @GsmSignal, @ReceiveTime, @CarState, @TeState, @AlarmState, @Mileage);";
                                cmd.Parameters.Add(new SqlParameter("Lat", message.Split(',')[0]));
                                cmd.Parameters.Add(new SqlParameter("Lon", message.Split(',')[1]));
                                cmd.Parameters.Add(new SqlParameter("Direction", message.Split(',')[2]));
                                cmd.Parameters.Add(new SqlParameter("GpsSignal", message.Split(',')[3]));
                                cmd.Parameters.Add(new SqlParameter("Speed", message.Split(',')[4]));
                                cmd.Parameters.Add(new SqlParameter("Temperature", message.Split(',')[5]));
                                cmd.Parameters.Add(new SqlParameter("GsmSignal", message.Split(',')[6]));
                                cmd.Parameters.Add(new SqlParameter("ReceiveTime", DateTime.Now));
                                cmd.Parameters.Add(new SqlParameter("CarState", message.Split(',')[7]));
                                cmd.Parameters.Add(new SqlParameter("TeState", message.Split(',')[8]));
                                cmd.Parameters.Add(new SqlParameter("AlarmState", message.Split(',')[9]));
                                cmd.Parameters.Add(new SqlParameter("Mileage", calMileage));
                                cmd.ExecuteNonQuery();
                                cmd.Dispose();
                                con.Close();
                            }
                            
                        }
                    }
                }

            }
        }

        double DirectDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double earthRadius = 3959.00;
            double dLat = ToRadians(lat2 - lat1);
            double dLng = ToRadians(lng2 - lng1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double dist = earthRadius * c;
            double meterConversion = 1609.334;
            return Math.Ceiling(dist * meterConversion);
        }

        double ToRadians(double degrees)
        {
            double radians = degrees * 3.14159265 / 180;
            return radians;
        }

    }
}

//ATx;id;
//SDx;remote id;msg
//RDx;remote id;msg


