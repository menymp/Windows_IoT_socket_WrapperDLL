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

namespace ModuleSOCKET
{
    /// <summary>
    /// Modulo de comunicacion por comandos Socket 
    /// menymp 2019
    /// </summary>
    public class Wrappersocket
    {
        /// <summary>
        /// Interface de servidor socket
        /// </summary>
        public StreamSocketClass objInterface;
        /// <summary>
        /// Lista de clientes que se han conectado al servidor socket
        /// </summary>
        public List<Server_clientRequest> ClientList = new List<Server_clientRequest>();
        /// <summary>
        /// Virtualizacion de cliente
        /// </summary>
        public Server_clientRequest objClient;


        /// <summary>
        /// Manifiesto de capacidades
        /// </summary>
        public readonly string Manifest = "SOCKET,FUNCT:Accept,";
        /// <summary>
        /// Inicializa una interfaz de comunicacion por Sockets
        /// </summary>
        /// <param name="Interface">IP y puerto</param>
        /// <param name="ID">Ancho de buffer</param>
        /// <returns></returns>
        public async Task Init(string Interface, string ID)
        {

            objInterface = new StreamSocketClass();
            var Iface = Interface.Split(',');
            //throw new System.ArgumentException("Parameter cannot be null", "original");
            //objInterface.Bind()
            if (Iface[0] == "")
            {
                objInterface.IsServer = true;
                objInterface.Bind(Iface[1], Convert.ToUInt32(ID));
                objInterface.Listen();
                objInterface.EventNewClientConnected += EventNewConnection;
                //objInterface.Accept();
            }
            else
            {
                objInterface.Connect(Iface[0], ID);
            }
        }
        /// <summary>
        /// Ejecuta un comando de Socket
        /// </summary>
        /// <param name="CMD">Comando</param>
        /// <param name="Args">Argumentos</param>
        /// <returns>Resultado</returns>
        public async Task<string> ExecuteCMD(string CMD, string Args)
        {
            string Result = "";
            switch (CMD)
            {
                case "Accept":

                    Result = "OK";

                    Accept();
                    //throw Exception();
                    break;

                case "Read":
                    Result = await Read();
                    break;

                case "Write":
                    Write(Args);
                    Result = "OK";
                    break;

                case "Close":
                    Result = "OK";
                    Stop();
                    break;

                case "Set":
                    Result = "ERR";
                    bool IsFound = false;

                    foreach(Server_clientRequest Item in ClientList)
                    {
                        if(Item.Socket.Information.RemoteAddress.RawName == Args)
                        {
                            IsFound = true;
                            objClient = Item;
                        }
                    }

                    if (IsFound == true) Result = "OK";
                    break;

                case "SetFirst":
                    objClient = ClientList.First();
                    break;

                default:
                    Result = "ERR";
                    break;

            }
            return Result;
        }
        /// <summary>
        /// Acepta una nueva conexion al servidor por parte de un cliente
        /// </summary>
        public void Accept()//bloqueante
        {
            objInterface.Accept();
        }
        /// <summary>
        /// Lee datos de un cliente
        /// </summary>
        /// <returns>awaitable datos obtenidos</returns>
        public async Task<string> Read()//no bloqueante
        {
            //return await objInterface.Receive();
            return await objClient.Receive();
        }
        /// <summary>
        /// Escribe datos a un cliente
        /// </summary>
        /// <param name="Data">Datos a escribir</param>
        /// <returns>Task awaitable</returns>
        public async Task Write(string Data)
        {
            //await objInterface.Send(Data);
            await objClient.Send(Data);
        }
        /// <summary>
        /// Detiene la comunicacion por sockets
        /// </summary>
        public void Stop()
        {
            //objInterface.Close();
            objClient.Close();
        }
        /// <summary>
        /// Handler de evento, ocurre cuando un nuevo cliente se ha conectado y añade el cliente a la lista
        /// </summary>
        /// <param name="sender">Informacion de coneccion del cliente y servidor</param>
        /// <param name="Client">Representacion del cliente</param>
        private void EventNewConnection(object sender, Server_clientRequest Client)
        {
            ClientList.Add(Client);
        }
    }
    /// <summary>
    /// Clase para el manejo de interfaces Socket
    /// menymp 2019
    /// </summary>
    public class StreamSocketClass
    {
        /// <summary>
        /// Indicador de Cliente o servidor
        /// </summary>
        public bool IsServer { get; set; }
        // Change this. True = server, false = client
        /// <summary>
        /// Puerto del cliente
        /// </summary>
        private string ServerPort = "";
        /// <summary>
        /// Socket de coneccion
        /// </summary>
        private StreamSocket ConnectionSocket;
        /// <summary>
        /// Lector de cadena de entrada del socket
        /// </summary>
        private StreamSocketListener Socket_input;
        /// <summary>
        /// Evento de coneccion recibida
        /// </summary>
        private StreamSocketListenerConnectionReceivedEventArgs Instance_connection;
        /// <summary>
        /// Ancho de buffer
        /// </summary>
        private uint Buffer_Len = 1024;
        /// <summary>
        /// Indica si se ha inicializado correctamente
        /// </summary>
        private bool Init_ok = false;
        /// <summary>
        /// Indica si hay datos en el buffer de datos de entrada
        /// </summary>
        private bool Buffer_in = false;

        /// <summary>
        /// Indica si se ha recibido informacion
        /// </summary>
        private readonly EventWaitHandle RecvHandle = new AutoResetEvent(false);
        /// <summary>
        /// Indica que al menos un cliente se ha conectado
        /// </summary>
        private readonly EventWaitHandle NoNullClient = new AutoResetEvent(false);
        /// <summary>
        /// Buffer de recepcion
        /// </summary>
        private string BufferRecev = "";

        /// <summary>
        /// Escucha una cadena de entrada
        /// </summary>
        private StreamSocketListener SocketListener_Obj;
        /// <summary>
        /// Objeto socket de cliente
        /// </summary>
        private StreamSocket Socket_Obj;
        /// <summary>
        /// Escritor de datos de salida
        /// </summary>
        private DataWriter Escritor_datos;
        /// <summary>
        /// Objeto socket de cliente
        /// </summary>
        private Server_clientRequest Temp_client;
        /// <summary>
        /// Delegado de evento de nuevo cliente
        /// </summary>
        /// <param name="sender">Informacion de coneccion del cliente y servidor</param>
        /// <param name="e">Representacion del cliente</param>
        public delegate void NewClientHandler(object sender, Server_clientRequest e);
        /// <summary>
        /// Evento que se dispara cuando un nuevo cliente se ha conectado
        /// </summary>
        public event NewClientHandler EventNewClientConnected;
        /// <summary>
        /// Indica el puerto y el ancho de buffer del servidor
        /// </summary>
        /// <param name="Port">Puerto</param>
        /// <param name="BuffLen">Ancho de buffer</param>
        public void Bind(string Port, uint BuffLen)
        {
            Buffer_Len = BuffLen;
            ServerPort = Port;
        }
        /// <summary>
        /// Inicia la operacion del servidor con los parametros de inicializacion
        /// </summary>
        public void Listen()
        {
            Socket_input = new StreamSocketListener();
            Socket_input.ConnectionReceived += DataListener_ConnectionReceived;

            Socket_input.BindServiceNameAsync(ServerPort).AsTask().Wait();
        }
        /// <summary>
        /// Acepta un nuevo cliente
        /// </summary>
        /// <returns>Representacion del cliente</returns>
        public Server_clientRequest Accept()
        {
            NoNullClient.WaitOne();
            return Temp_client;
        }
        /// <summary>
        /// Funcion de evento, ocurre cuando un nuevo cliente se ha conectado
        /// </summary>
        /// <param name="sender">Informacion de coneccion del cliente y servidor</param>
        /// <param name="args">Representacion del cliente</param>
        private async void DataListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Socket_Obj = args.Socket;
            SocketListener_Obj = sender;

            //DataReader DataListener_Reader;
            //DataWriter Escritor_datos;
            //StringBuilder DataListener_StrBuilder;// Escritor_constructor;
            //string DataReceived;

            Init_ok = true;
            Instance_connection = args;
            Escritor_datos = new DataWriter(Instance_connection.Socket.OutputStream);
            
            Temp_client = new Server_clientRequest();
            Temp_client.Buffer_Len = Buffer_Len;
            Temp_client.Socket = Socket_Obj;
            Temp_client.SocketListener = SocketListener_Obj;
            Temp_client.StreamOut = Escritor_datos;

            NoNullClient.Set();
            if (EventNewClientConnected != null) EventNewClientConnected(this, Temp_client);
            //using (DataListener_Reader = new DataReader(Socket_Obj.InputStream))
            //{
            //    DataListener_StrBuilder = new StringBuilder();
            //    DataListener_Reader.InputStreamOptions = InputStreamOptions.Partial;
            //    DataListener_Reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            //    DataListener_Reader.ByteOrder = ByteOrder.LittleEndian;
            //    await DataListener_Reader.LoadAsync(Buffer_Len);
            //    while (DataListener_Reader.UnconsumedBufferLength > 0)
            //    {
            //        DataListener_StrBuilder.Append(DataListener_Reader.ReadString(DataListener_Reader.UnconsumedBufferLength));
            //        //await DataListener_Reader.LoadAsync(6);
            //    }
            //    DataListener_Reader.DetachStream();
            //    DataReceived = DataListener_StrBuilder.ToString();
            //    BufferRecev = DataReceived;
            //    Buffer_in = true;
            //    RecvHandle.Set();
            //}

            ////Send("hola cliente :v");
            ///*
            //using (Escritor_datos = new DataWriter(args.Socket.OutputStream))
            //{
            //    string content = "hola cliente :v";
            //    byte[] data = Encoding.UTF8.GetBytes(content);
            //    Escritor_datos.WriteBytes(data);
            //    await Escritor_datos.StoreAsync();
            //    Escritor_datos.DetachBuffer();
            //    Escritor_datos.Dispose();
            //}*/

            //if (DataReceived != null)
            //{

            //    if (IsServer)
            //    {
            //        //Debug.WriteLine("[SERVER] I've received " + DataReceived + " from " + args.Socket.Information.RemoteHostName);

            //    }

            //    else
            //    {
            //        //Debug.WriteLine("[CLIENT] I've received " + DataReceived + " from " + args.Socket.Information.RemoteHostName);

            //    }
            //}
            //else
            //{
            //    //Debug.WriteLine("Received data was empty. Check if you sent data.");
            //}

        }
        //private TaskCompletionSource<object> continueClicked;
        /// <summary>
        /// Inicia una lectura de datos de manera asincrona
        /// </summary>
        /// <returns>task awaitable</returns>
        private async Task ReceiveData()
        {
            DataReader DataListener_Reader;
            //DataWriter Escritor_datos;
            StringBuilder DataListener_StrBuilder;// Escritor_constructor;
            string DataReceived;

            Init_ok = true;
            //Instance_connection = args;

            using (DataListener_Reader = new DataReader(Socket_Obj.InputStream))
            {
                DataListener_StrBuilder = new StringBuilder();
                DataListener_Reader.InputStreamOptions = InputStreamOptions.Partial;
                DataListener_Reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                DataListener_Reader.ByteOrder = ByteOrder.LittleEndian;
                await DataListener_Reader.LoadAsync(Buffer_Len);
                while (DataListener_Reader.UnconsumedBufferLength > 0)
                {
                    DataListener_StrBuilder.Append(DataListener_Reader.ReadString(DataListener_Reader.UnconsumedBufferLength));
                    //await DataListener_Reader.LoadAsync(6);
                }
                DataListener_Reader.DetachStream();
                DataReceived = DataListener_StrBuilder.ToString();
                BufferRecev = DataReceived;
                Buffer_in = true;
                RecvHandle.Set();
            }
        }
        /// <summary>
        /// Recibe informacion del cliente Depreciated...
        /// </summary>
        /// <returns>Task awaitable</returns>
        public async Task<string> Receive()
        {
            await ReceiveData();
            RecvHandle.WaitOne();
            Buffer_in = false;
            return BufferRecev;
        }
        /// <summary>
        /// Envia informacion al cliente
        /// </summary>
        /// <param name="Msg">Datos a enviar al cliente Depreciated</param>
        /// <returns>Task awaitable</returns>
        public async Task Send(string Msg)
        {
            if (IsServer)
            {
                if (Init_ok)
                {
                    //DataWriter Escritor_datos;
                    //using (Escritor_datos = new DataWriter(Instance_connection.Socket.OutputStream))

                    //using (Escritor_datos)
                    //{

                    //Escritor_datos.WriteString("Hola cliente");
                    string content = Msg;
                    byte[] data = Encoding.UTF8.GetBytes(content);
                    Escritor_datos.WriteBytes(data);
                    //Debugger.Break();
                    await Escritor_datos.StoreAsync();
                    //Escritor_datos.DetachBuffer();
                    //Escritor_datos.Dispose();

                    //}
                }
            }
            else
            {
                DataWriter SentResponse_Writer = new DataWriter(ConnectionSocket.OutputStream);
                //string content = MessageToSent;
                byte[] data = Encoding.UTF8.GetBytes(Msg);
                // Write the bytes
                SentResponse_Writer.WriteBytes(data);
                // Store the written data
                await SentResponse_Writer.StoreAsync();
                SentResponse_Writer.DetachStream();
                // Dispose the data
                SentResponse_Writer.Dispose();
                // Dispose the connection.
                //ConnectionSocket.Dispose();
                //ConnectionSocket = new StreamSocket();
            }

        }
        /// <summary>
        /// Conecta el cliente a un nuevo servidor
        /// </summary>
        /// <param name="Address">Direccion de host</param>
        /// <param name="Port">Puerto</param>
        public async void Connect(string Address, string Port)
        {
            ServerPort = Port;
            HostName ServerAdress = new HostName(Address);
            ConnectionSocket = new StreamSocket();
            await ConnectionSocket.ConnectAsync(ServerAdress, ServerPort);
        }
        /// <summary>
        /// Envia una respuesta del cliente a un servidor
        /// </summary>
        /// <param name="MessageToSent">Mensaje a enviar</param>
        public async void SentResponse(string MessageToSent)
        {
            try
            {
                // Try connect
                //Debug.WriteLine("Attempting to connect. " + Environment.NewLine);
                //ConnectionSocket = new StreamSocket();
                // Wait on connection
                //await ConnectionSocket.ConnectAsync(Adress, ServerPort);
                // Create a DataWriter
                DataWriter SentResponse_Writer = new DataWriter(ConnectionSocket.OutputStream);
                string content = MessageToSent;
                byte[] data = Encoding.UTF8.GetBytes(content);
                // Write the bytes
                SentResponse_Writer.WriteBytes(data);
                // Store the written data
                await SentResponse_Writer.StoreAsync();
                SentResponse_Writer.DetachStream();
                // Dispose the data
                SentResponse_Writer.Dispose();
                //Debug.WriteLine("Connection has been made and your message " + MessageToSent + " has been sent." + Environment.NewLine);
                // Dispose the connection.
                ConnectionSocket.Dispose();
                ConnectionSocket = new StreamSocket();
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Failed to connect " + exception.Message);
                ConnectionSocket.Dispose();
                ConnectionSocket = null;

            }
        }
        /// <summary>
        /// Cierra la coleccion de cliente 
        /// </summary>
        public async void Close()
        {
            if (IsServer)
            {

            }
            else
            {
                ConnectionSocket.Dispose();
            }

        }
    }
    /// <summary>
    /// referencia un cliente que se ha conectado al servidor socket
    /// </summary>
    public class Server_clientRequest
    {
        private StreamSocketListener _SocketListener_obj;
        /// <summary>
        /// Obtiene o establece la referencia de conexion de entrada
        /// </summary>
        public StreamSocketListener SocketListener
        {
            get
            {
                return _SocketListener_obj;
            }
            set
            {
                _SocketListener_obj = value;
            }
        }

        private StreamSocket _Socket_Obj;
        /// <summary>
        /// Obtiene o establece la referencia al objeto socket
        /// </summary>
        public StreamSocket Socket
        {
            get
            {
                return _Socket_Obj;
            }
            set
            {
                _Socket_Obj = value;
            }
        }

        private DataWriter _Escritor_datos;
        /// <summary>
        /// Obtiene o establece el escritor de datos de salida
        /// </summary>
        public DataWriter StreamOut
        {
            get
            {
                return _Escritor_datos;
            }
            set
            {
                _Escritor_datos = value;
            }
        }

        /// <summary>
        /// Indica datos en el buffer de entrada
        /// </summary>
        private bool Buffer_in = false;
        /// <summary>
        /// Buffer de entrada
        /// </summary>
        private string BufferRecev = "";
        /// <summary>
        /// Ancho de buffer
        /// </summary>
        private uint _Buffer_Len = 1024;
        /// <summary>
        /// Evento de recepcion de datos por parte del cliente
        /// </summary>
        private readonly EventWaitHandle RecvHandle = new AutoResetEvent(false);
        /// <summary>
        /// Obtiene o establece el ancho de buffer de salida
        /// </summary>
        public uint Buffer_Len
        {
            get
            {
                return _Buffer_Len;
            }
            set
            {
                _Buffer_Len = value;
            }
        }
        /// <summary>
        /// Envia contenido al cliente
        /// </summary>
        /// <param name="Msg">Contenido en formato string a enviar</param>
        /// <returns>awaitable task object</returns>
        public async Task Send(string Msg)
        {
                    string content = Msg;
                    byte[] data = Encoding.UTF8.GetBytes(content);
                    _Escritor_datos.WriteBytes(data);
                    //Debugger.Break();
                    await _Escritor_datos.StoreAsync();
        }
        /// <summary>
        /// Concluye la sesion de envio de datos
        /// </summary>
        public void Close()
        {
            _Escritor_datos.Dispose();
        }
        /// <summary>
        /// Recibe los datos de entrada en una sesion
        /// </summary>
        /// <returns>awaitable task object</returns>
        private async Task ReceiveData()
        {
            DataReader DataListener_Reader;
            //DataWriter Escritor_datos;
            StringBuilder DataListener_StrBuilder;// Escritor_constructor;
            string DataReceived;

            
            //Instance_connection = args;

            using (DataListener_Reader = new DataReader(_Socket_Obj.InputStream))
            {
                DataListener_StrBuilder = new StringBuilder();
                DataListener_Reader.InputStreamOptions = InputStreamOptions.Partial;
                DataListener_Reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                DataListener_Reader.ByteOrder = ByteOrder.LittleEndian;
                await DataListener_Reader.LoadAsync(_Buffer_Len);
                while (DataListener_Reader.UnconsumedBufferLength > 0)
                {
                    DataListener_StrBuilder.Append(DataListener_Reader.ReadString(DataListener_Reader.UnconsumedBufferLength));
                    //await DataListener_Reader.LoadAsync(6);
                }
                DataListener_Reader.DetachStream();
                DataReceived = DataListener_StrBuilder.ToString();
                BufferRecev = DataReceived;
                Buffer_in = true;
                RecvHandle.Set();
            }
        }
        /// <summary>
        /// Retorna los datos recibidos desde el cliente
        /// </summary>
        /// <returns>awaitable task object string Buffer out</returns>
        public async Task<string> Receive()
        {
            await ReceiveData();
            RecvHandle.WaitOne();
            Buffer_in = false;
            return BufferRecev;
        }

    }

}
