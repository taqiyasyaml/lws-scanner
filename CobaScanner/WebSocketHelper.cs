using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Diagnostics;
using Fleck;
using System.Net.Sockets;

namespace CobaScanner
{
    internal class WebSocketHelper
    {
        private ConfigHelper Conf;
        private static WebSocketServer? WSServer;
        private static bool IsWSStarted = false;

        private NumericUpDown portInput;
        private Button startStopWSButton;
        private ListBox whiteListHost;
        private Form1 form1;

        private DTWAINHelper DtwainHelper;
        public WebSocketHelper(ConfigHelper conf, Form1 form1, NumericUpDown portInput, Button startStopWSButton, ListBox whiteListHost, DTWAINHelper DtwainHelper)
        {
            this.Conf = conf;
            this.portInput = portInput;
            this.startStopWSButton = startStopWSButton;
            this.whiteListHost = whiteListHost;
            this.form1 = form1;
            this.DtwainHelper = DtwainHelper;
        }

        public void StartWS()
        {
            if (WebSocketHelper.IsWSStarted)
            {
                this.StopWS();
            }
            WebSocketHelper.WSServer = new WebSocketServer("ws://127.0.0.1:" + portInput.Value.ToString());
            WebSocketHelper.WSServer.Start((IWebSocketConnection sock) =>
            {
                sock.OnOpen += () => this.OnOpen(sock);
                sock.OnMessage += (String msg) => this.OnMessage(sock, msg);
                sock.OnClose += () => this.OnClose(sock);
            });
            WebSocketHelper.IsWSStarted = true;
            this.startStopWSButton.Text = "Stop";
            this.portInput.Enabled = false;
        }

        private void StopWS()
        {
            if (WebSocketHelper.WSServer == null)
            {
                return;
            }
            WebSocketHelper.WSServer.ListenerSocket.Close();
            WebSocketHelper.WSServer.ListenerSocket.Dispose();
            WebSocketHelper.WSServer.Dispose();
            WebSocketHelper.IsWSStarted = false;
            this.startStopWSButton.Text = "Start";
            this.portInput.Enabled = true;
        }

        public void StartStopWS()
        {
            if (WebSocketHelper.IsWSStarted)
                this.StopWS();
            else
                this.StartWS();
        }

        private bool CheckWhiteListHost(IWebSocketConnection sock)
        {
            String OriginOrIP = sock.ConnectionInfo.Origin ?? (sock.ConnectionInfo.ClientIpAddress + ":" + sock.ConnectionInfo.ClientPort);
            if (OriginOrIP.Length == 0)
                return false;
            if (this.whiteListHost.FindStringExact(OriginOrIP) < 0)
            {
                bool IsWhiteList = false;
                this.form1.Invoke((MethodInvoker)delegate
                {
                    DialogResult dialogResult = MessageBox.Show(new Form { TopMost = true }, "Do you want to add " + OriginOrIP + " on white list origins?", "Confirmation", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        this.whiteListHost.Items.Add(OriginOrIP);
                        this.Conf.SaveWhiteList();
                        IsWhiteList = true;
                    }
                    else
                        IsWhiteList = false;
                });
                return IsWhiteList;
            }
            return true;
        }

        private void OnOpen(IWebSocketConnection sock)
        {
            this.CheckWhiteListHost(sock);
        }

        private void OnMessage(IWebSocketConnection sock, String message)
        {
            JsonNode MsgJson = JsonNode.Parse(message)!;
            string request = (string)MsgJson!["request"]!;
            if (request == "doScan")
            {
                if (CheckWhiteListHost(sock))
                {
                    DtwainHelper.DoScanSocket(sock);
                }
                else
                {
                    JsonObject result = new JsonObject();
                    result["request"] = "onScanError";
                    result["error"] = "UNAUTHORIZED";
                    sock.Send(result.ToJsonString());
                }
            }
            else if(request == "doStopNextPage")
            {
                JsonObject result = new JsonObject();
                if (DTWAINHelper.socket != sock)
                {
                    result["request"] = "onScanError";
                    result["error"] = "FORBIDDEN";
                }
                else if (DTWAINHelper.socket == sock && DTWAINHelper.KeepScanning)
                {
                    DTWAINHelper.KeepScanning = false;
                    DTWAINHelper.socket = null;
                    result["request"] = "onScanCallback";
                    result["status"] = "STOP_NEXT_PAGE";
                }
                sock.Send(result.ToJsonString());
            }
        }

        private void OnClose(IWebSocketConnection sock)
        {
            if (DTWAINHelper.socket == sock && DTWAINHelper.KeepScanning)
            {
                DTWAINHelper.KeepScanning = false;
                DTWAINHelper.socket = null;
            }
        }
    }
}
