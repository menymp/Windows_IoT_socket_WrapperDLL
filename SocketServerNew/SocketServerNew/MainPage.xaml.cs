using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking; //nuevo
using System.Diagnostics;  //nuevo
using SocketWinCore;
using System.Threading.Tasks;
using ModuleSOCKET;
// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace SocketServerNew
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        StreamSocketClass SocketManager = new StreamSocketClass(); //la clase streamsocket es propia
        Server_clientRequest Cliente;
        Server_clientRequest Cliente2;
        public MainPage()
        {
            this.InitializeComponent();
            // Declaring IsServer (True = server, False = client)




            SocketTest();
        }

        public async void SocketTest()
        {
            SocketManager.IsServer = true;
            // Declaring HostName of Server
            //HostName ServerAdress = new HostName("10.6.12.101");//DESKTOP-A1SAQ5U
            // Open Listening ports and start listening.
            SocketManager.Bind("1234", 6);
            SocketManager.Listen();
            // Server
            if (SocketManager.IsServer)
            {
                Debug.WriteLine("[SERVER] Ready to receive");
                string recv;
                string recv2;
                Cliente = SocketManager.Accept();
                //Cliente2 = SocketManager.Accept();
                //Task<string> TaskRecepcion  = 
                while (true)
                {

                    //recv = await SocketManager.Receive();
                    recv = await Cliente.Receive();
                    //recv2 = await Cliente2.Receive();
                    Debug.WriteLine("[SERVER] Se recibio : " + recv );
                    await Cliente.Send("blyat");
                    //await Cliente2.Send("blyat");
                    //SocketManager.Send("blyat");
                }
            }
            // Client
            else
            {
                SocketManager.Connect("10.6.12.101", "1234");
                SocketManager.SentResponse("Hello WindowsInstructed");
            }

        }
    }
}

