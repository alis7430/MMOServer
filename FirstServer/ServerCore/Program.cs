using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            //DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            //문지기
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //문지기 교육
            listenSocket.Bind(endPoint);

            // 영업 시작
            // backlog : 최대 대기수
            listenSocket.Listen(10);

            while(true)
            {
                Console.WriteLine("Listening....");

                //손님을 입장시킨다
                Socket clientSocket = listenSocket.Accept();

                //받는다
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);
                Encoding.UTF8

            }

        }
    }
}