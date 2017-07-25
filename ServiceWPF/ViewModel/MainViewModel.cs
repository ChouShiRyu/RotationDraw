using CommonEntity;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServiceWPF.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public int point = 4546;
        public Socket CommonSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public ObservableCollection<Socket> SocketList = new ObservableCollection<Socket>();
        public Thread thd;
        public MainViewModel()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Any, point);
            CommonSocket.Bind(ipe);
            CommonSocket.Listen(400);
            thd = new Thread(() =>
            {
                while (true)
                {
                    Socket temp = CommonSocket.Accept();
                    SocketList.Add(temp);
                    var thtemp = new Thread(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                byte[] recvBytes = new byte[65635];
                                int bytes;
                                bytes = temp.Receive(recvBytes, recvBytes.Length, 0); //�ӿͻ��˽�����Ϣ
                                var text = Encoding.UTF8.GetString(recvBytes, 0, bytes);
                                var c = DeserializeJsonToObject<ConnectBase<string>>(text);
                                if (c == null)
                                {
                                    continue;
                                }
                                ToDoThread.Init(c, temp);
                            }
                        }
                        catch
                        {
                            SocketList.Remove(temp);
                        }
                    });
                    thtemp.Start();
                }
            });
            thd.Start();

            //��Client�˷�����Ϣ
            //  Console.WriteLine("server get message:{0}", recvStr);    //�ѿͻ��˴�������Ϣ��ʾ����
            //string sendStr = "ok!Client send message successful!";
            //byte[] bs = Encoding.ASCII.GetBytes(sendStr);
            //temp.Send(bs, bs.Length, 0);  //������Ϣ���ͻ���
            //temp.Close();
            //s.Close();
            //   Console.ReadLine();

        }
        private string m_TextBoxText;

        public string TextBoxText
        {
            get => m_TextBoxText; set
            {
                m_TextBoxText = value;
                this.RaisePropertyChanged(() => TextBoxText);
            }
        }

        /// <summary>
        /// ����JSON�ַ������ɶ���ʵ��
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
        /// <param name="json">json�ַ���(eg.{"ID":"112","Name":"ʯ�Ӷ�"})</param>
        /// <returns>����ʵ��</returns>
        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            T t = o as T;
            return t;
        }
    }
}