using System;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Client_VoiceChat
{
    public partial class Form_ChatClient : Form
    {
        const int _defaultPORT = 8080; //Порт используемый по умолчанию
        private Thread _thread;
        private DirectSoundHelper sound;
        private Socket ClientSocket;
        private byte[] _buffer = new byte[2205];
        private bool _isTalk = false;
        

        public Form_ChatClient()
        {
            
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            //Проверять наличие нелегальных перекрестных потоков
        }

        /// <summary>
        /// Подключиться к серверу
        /// </summary>
        /// <param name="ServerIP">IP адрес сервера тип IPAddres</param>
        /// <param name="Port">Номер порта сервера тип int</param>
        void Connect(IPAddress ServerIP, int Port)//(string ServerIP, int Port)
        {
            try
            {
                if (ClientSocket != null && ClientSocket.Connected)
                {
                    //В следующем примере кода Shutdown Отключение Socket.
                    ClientSocket.Shutdown(SocketShutdown.Both);
                    System.Threading.Thread.Sleep(10);
                    ClientSocket.Close();
                }

                ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint Server_EndPoint = new IPEndPoint(ServerIP, Port);
                ClientSocket.Blocking = false;

                ClientSocket.BeginConnect(Server_EndPoint, new AsyncCallback(OnConnect), ClientSocket);

                _thread = new Thread(new ThreadStart(sound.StartCapturing));
                _thread.IsBackground = true;
                _thread.Start();
            }
            catch (Exception) { }
        }

        /// <summary>
        ///  Интерфейс реализован с помощью классов, содержащих методы, которые могут работать асинхронно.
        /// </summary>
        /// <param name="ar">содержит сведения об асинхронной операции</param>
        public void OnConnect(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;

            try
            {
                if (sock.Connected)
                {
                    SetupRecieveCallback(sock);
                    btnTalk.Enabled = true;
                    btnDisconnect.Enabled = true;
                    btnJoin.Enabled = false;
                    
                }
                else
                {
                    Disconncet();
                    btnJoin.Enabled = true;
                    btnTalk.Enabled = false;
                    btnDisconnect.Enabled = false;
                    MessageBox.Show("Не удалось подсоединиться");
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        ///  Послать информацию на сервер
        /// </summary>
        /// <param name="buffer">Буфер для хранения информации, тип byte</param>
        void SendBuffer(byte[] buffer)
        {
            ClientSocket.Send(buffer, SocketFlags.None);
        }

        /// <summary>
        ///  Интерфейс реализован с помощью классов, содержащих методы, которые могут работать асинхронно.
        /// </summary>
        /// <param name="ar">содержит сведения об асинхронной операции</param>
        public void OnRecievedData(IAsyncResult ar)
        {
            Socket secket = (Socket)ar.AsyncState;

            try
            {
                int LenRec = secket.EndReceive(ar);
                if (LenRec > 0)
                {
                    sound.PlayReceivedVoice(_buffer);

                    SetupRecieveCallback(secket);
                }
                else
                {
                    secket.Shutdown(SocketShutdown.Both);
                    secket.Close();
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// устанавливает каллбэк. 
        /// начинает передачу данных.
        /// </summary>
        /// <param name="socket">для сетевых взаимодействий</param>
        public void SetupRecieveCallback(Socket socket)
        {
            try
            {
                AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, recieveData, socket);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Вызывает метод, для отключения от сервера
        /// </summary>
        void Disconncet()
        {
            try
            {
                if (ClientSocket != null & ClientSocket.Connected)
                {
                    ClientSocket.Close();
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// содсоединиться к сети.
        /// </summary>
        /// <param name="sender">Объект вызвавщий событие</param>
        /// <param name="e">аргумент не используется</param>
        private void btnJoin_Click(object sender, EventArgs e)
        {
            int thePort;
            thePort = _defaultPORT;
            IPAddress theIP; 

            if( int.TryParse(tbPORT.Text, out thePort) && IPAddress.TryParse(tbServerIP.Text, out theIP))
            {

                Connect(theIP, thePort);
                //MessageBox.Show(theIP.ToString(), thePort.ToString());
            }

            
        }

        /// <summary>
        ///  Послать голосовой буфер
        ///  Событие происходить
        ///  когда голосовой буфер заполняется
        /// </summary>
        /// <param name="VoiceBuffer">Объект вызвыший событие. (голосовой буфер)</param>
        /// <param name="e">аргумент не используется</param>
        void SendVoiceBuffer(object VoiceBuffer, EventArgs e)
        {
            byte[] Buffer = (byte[])VoiceBuffer;
            SendBuffer(Buffer);
        }

        /// <summary>
        ///  Начать передачу данных.
        /// </summary>
        /// <param name="sender">бъект вызвыший событие</param>
        /// <param name="e">аргумент не используется</param>
        private void btnTalk_Click(object sender, EventArgs e)
        {
            if(_isTalk == false)
            {
                sound._StopLoop = false;
                btnTalk.Text = "Откл";
                btnDisconnect.Enabled = false;
            }
            else
            {
                sound._StopLoop = true;
                btnTalk.Text = "Говорить";
                btnDisconnect.Enabled = true;
            }
           _isTalk =!_isTalk;
        }

       
        private void Form_ChatClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconncet();
        }

        /// <summary>
        /// срабатывает при загрузке формы
        /// </summary>
        /// <param name="sender">объект выввавщий событие</param>
        /// <param name="e">аргумент не используется</param>
        private void Form_ChatClient_Load(object sender, EventArgs e)
        {
            sound = new DirectSoundHelper();
            sound.OnBufferFulfill += new EventHandler(SendVoiceBuffer);
        }

        /// <summary>
        /// Отключиться от сети 
        /// </summary>
        /// <param name="sender">объект выввавщий событие</param>
        /// <param name="e">аргумент не используется</param>
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            Disconncet();
            btnJoin.Enabled = true;
            btnTalk.Enabled = false;
        }


        /// <summary>
        /// ограничивает ввод символов в textBox
        /// и заменяет введённую запятую на точку
        /// </summary>
        /// <param name="sender">объект выввавщий событие</param>
        /// <param name="e">аргумент не используется</param>
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0') && (e.KeyChar <= '9') ||
                (e.KeyChar == (char)Keys.Back) || (e.KeyChar == '.'))
            return;
            if (e.KeyChar == ',')
            {
                e.KeyChar = '.';
                return;
            }
            
            e.Handled = true;
        }

        /// <summary>
        /// ограничивает ввод символов в textBox
        /// </summary>
        /// <param name="sender">объект выввавщий событие</param>
        /// <param name="e">аргумент не используется</param>
        private void textBoxPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0') && (e.KeyChar <= '9') || (e.KeyChar == (char)Keys.Back))  
               return;

            e.Handled = true;
        }

        
    }
}
