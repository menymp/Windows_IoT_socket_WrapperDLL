using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace SocketWinCore
{

    //class StreamSocketClass
    //{
    //    public static bool IsServer { get; set; }
    //    // Change this. True = server, false = client
    //    private string ServerPort ;

    //    private StreamSocket ConnectionSocket;
    //    private StreamSocketListener Socket_input;
    //    private StreamSocketListenerConnectionReceivedEventArgs Instance_connection;

    //    private uint Buffer_Len = 1024;

    //    private bool Init_ok = false;
    //    private bool Buffer_in = false;

    //    private readonly EventWaitHandle RecvHandle = new AutoResetEvent(false);

    //    private string BufferRecev = "";
    //    public void Listen(string ServerPort, uint BuffLen)
    //    {
    //        Buffer_Len = BuffLen;
    //        Socket_input = new StreamSocketListener();
    //        Socket_input.ConnectionReceived += DataListener_ConnectionReceived;
    //        Socket_input.BindServiceNameAsync(ServerPort).AsTask().Wait();
            
    //    }

    //    private async void DataListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    //    {
    //        DataReader DataListener_Reader;
    //        //DataWriter Escritor_datos;
    //        StringBuilder DataListener_StrBuilder;// Escritor_constructor;
    //        string DataReceived;

    //        Init_ok = true;
    //        Instance_connection = args;

    //        using (DataListener_Reader = new DataReader(args.Socket.InputStream))
    //        {
    //            DataListener_StrBuilder = new StringBuilder();
    //            DataListener_Reader.InputStreamOptions = InputStreamOptions.Partial;
    //            DataListener_Reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
    //            DataListener_Reader.ByteOrder = ByteOrder.LittleEndian;
    //            await DataListener_Reader.LoadAsync(Buffer_Len);
    //            while (DataListener_Reader.UnconsumedBufferLength > 0)
    //            {
    //                DataListener_StrBuilder.Append(DataListener_Reader.ReadString(DataListener_Reader.UnconsumedBufferLength));
    //                //await DataListener_Reader.LoadAsync(6);
    //            }
    //            DataListener_Reader.DetachStream();
    //            DataReceived = DataListener_StrBuilder.ToString();
    //            BufferRecev = DataReceived;
    //            Buffer_in = true;
    //            RecvHandle.Set();
    //        }

    //         //Send("hola cliente :v");
    //        /*
    //        using (Escritor_datos = new DataWriter(args.Socket.OutputStream))
    //        {
    //            string content = "hola cliente :v";
    //            byte[] data = Encoding.UTF8.GetBytes(content);
    //            Escritor_datos.WriteBytes(data);
    //            await Escritor_datos.StoreAsync();
    //            Escritor_datos.DetachBuffer();
    //            Escritor_datos.Dispose();
    //        }*/
            
    //        if (DataReceived != null)
    //        {
                
    //            if (IsServer)
    //            {
    //                //Debug.WriteLine("[SERVER] I've received " + DataReceived + " from " + args.Socket.Information.RemoteHostName);

    //            }
                
    //            else
    //            {
    //                //Debug.WriteLine("[CLIENT] I've received " + DataReceived + " from " + args.Socket.Information.RemoteHostName);
                    
    //            }
    //        }
    //        else
    //        {
    //            //Debug.WriteLine("Received data was empty. Check if you sent data.");
    //        }

    //    }

    //    private TaskCompletionSource<object> continueClicked;
    //    public  string Receive()
    //    {
    //        //TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
    //        //continueClicked = new TaskCompletionSource<object>();
    //        // RecvHandle.WaitOne();
    //        //await continueClicked.Task;
    //        //Buffer_in = false;
    //        //return BufferRecev;
    //        //return Task.Run(() =>
    //        //{
    //        //    while (Buffer_in)
    //        //    {
    //        //    }
    //        //    Buffer_in = false;
    //        //    return BufferRecev;
    //        //});
    //        RecvHandle.WaitOne();
    //        Buffer_in = false;
    //        return BufferRecev;
    //    }

    //    public async void Send(string Msg)
    //    {
    //        if (Init_ok)
    //        {
    //            DataWriter Escritor_datos;
    //            using (Escritor_datos = new DataWriter(Instance_connection.Socket.OutputStream))
    //            {

    //                //Escritor_datos.WriteString("Hola cliente");
    //                string content = Msg;
    //                byte[] data = Encoding.UTF8.GetBytes(content);
    //                Escritor_datos.WriteBytes(data);
    //                await Escritor_datos.StoreAsync();
    //                Escritor_datos.DetachBuffer();
    //                Escritor_datos.Dispose();
    //            }
    //        }
    //    }

    //    public async void SentResponse(HostName Adress, string MessageToSent)
    //    {
    //        try
    //        {
    //            // Try connect
    //            Debug.WriteLine("Attempting to connect. " + Environment.NewLine);
    //            ConnectionSocket = new StreamSocket();
    //            // Wait on connection
    //            await ConnectionSocket.ConnectAsync(Adress, ServerPort);
    //            // Create a DataWriter
    //            DataWriter SentResponse_Writer = new DataWriter(ConnectionSocket.OutputStream);
    //            string content = MessageToSent;
    //            byte[] data = Encoding.UTF8.GetBytes(content);
    //            // Write the bytes
    //            SentResponse_Writer.WriteBytes(data);
    //            // Store the written data
    //            await SentResponse_Writer.StoreAsync();
    //            SentResponse_Writer.DetachStream();
    //            // Dispose the data
    //            SentResponse_Writer.Dispose();
    //            Debug.WriteLine("Connection has been made and your message " + MessageToSent + " has been sent." + Environment.NewLine);
    //            // Dispose the connection.
    //            ConnectionSocket.Dispose();
    //            ConnectionSocket = new StreamSocket();
    //        }
    //        catch (Exception exception)
    //        {
    //            Debug.WriteLine("Failed to connect " + exception.Message);
    //            ConnectionSocket.Dispose();
    //            ConnectionSocket = null;

    //        }
    //    }
    //}
}
