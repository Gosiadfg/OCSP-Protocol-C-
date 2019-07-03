using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using ProtoBuf;
using System.Net;

namespace SocketServer
{
    [ProtoContract]
    public class OCSPRequest
    {
        [ProtoMember(1)]
        public string OCSPRequestData { get; set; }
        [ProtoMember(2)]
        public int Version { get; set; }
        [ProtoMember(3)]
        public string RequestorList { get; set; }
        [ProtoMember(4)]
        public string CertificateID { get; set; }
        [ProtoMember(5)]
        public string HashAlgorithm { get; set; }
        [ProtoMember(6)]
        public string IssuerNameHash { get; set; }
        [ProtoMember(7)]
        public string IssuerKeyHash { get; set; }
        [ProtoMember(8)]
        public string SerialNumber { get; set; }
    }

    [ProtoContract]
    public class OCSPResponse
    {
        [ProtoMember(1)]
        public string OCSPResponseData;
        [ProtoMember(2)]
        public string OCSPREsponseStatus;
        [ProtoMember(3)]
        public string ResponseType;
        [ProtoMember(4)]
        public int Version;
        [ProtoMember(5)]
        public string ResponderID;
        [ProtoMember(6)]
        public string Technologies;
        [ProtoMember(7)]
        public string ProducedAt;
        [ProtoMember(8)]
        public string Responses;
        [ProtoMember(9)]
        public string CertificateID;
        [ProtoMember(10)]
        public string HashAlgorithm;
        [ProtoMember(11)]
        public string IssuerNameHash;
        [ProtoMember(12)]
        public string IssuerKEyHash;
        [ProtoMember(13)]
        public string SerialNumber;
        [ProtoMember(14)]
        public string CertStatus;
        [ProtoMember(15)]
        public string ThisUpdate;
        [ProtoMember(16)]
        public string NextUpdate;
    }

    class Program
    {
        static void Main(string[] args)
        {
            var ApplicationOcspRequest = new OCSPRequest();
            ApplicationOcspRequest = StartAcceptor();
            StartClient(ApplicationOcspRequest);
            var ApplicationOcspResponse = new OCSPResponse();
            ApplicationOcspResponse = StartAcceptor1();
            StartClient1(ApplicationOcspResponse);
            Console.ReadKey();
        }

        static void StartClient(OCSPRequest dane)
        {
            var client = new TcpClient();
            string srvIP;
            Console.WriteLine("Podaj IP serwera: ");
            srvIP = Console.ReadLine();
            client.Connect(IPAddress.Parse(srvIP), 12340);

            NetworkStream serverStream = client.GetStream();

            var ApplicationOcspRequest = new OCSPRequest();
            ApplicationOcspRequest = dane;

            Serializer.SerializeWithLengthPrefix(serverStream, ApplicationOcspRequest, PrefixStyle.Base128);
            Console.WriteLine("Klient2: Przesłano " + ApplicationOcspRequest.CertificateID);
        }

        static void StartClient1(OCSPResponse dane)
        {
            var client = new TcpClient();
            string clntIP;
            Console.WriteLine("Podaj adres IP Klienta1");
            clntIP = Console.ReadLine();
            client.Connect(IPAddress.Parse(clntIP), 12342);

            NetworkStream serverStream = client.GetStream();

            var ApplicationOcspResponse = new OCSPResponse();
            ApplicationOcspResponse = dane;

            Serializer.SerializeWithLengthPrefix(serverStream, ApplicationOcspResponse, PrefixStyle.Base128);
            Console.WriteLine("Klient2: Przesłano " + ApplicationOcspResponse.CertStatus);
        }

        static OCSPResponse StartAcceptor1()
        {
            string clnt2IP;
            Console.WriteLine("Podaj IP nasłuchu: ");
            clnt2IP = Console.ReadLine();
            var ipAddress = IPAddress.Parse(clnt2IP);
            TcpListener serverSocket = new TcpListener(ipAddress, 12341);
            serverSocket.Start();
            Console.WriteLine("Klient2: Oczekiwanie...");

            var clientSocket = serverSocket.AcceptTcpClient();
            OCSPResponse ApplicationOcspResponse = StartServerSocket1(clientSocket);
            return ApplicationOcspResponse;
        }

        static OCSPRequest StartAcceptor()
        {
            var ipAddress = IPAddress.Parse("127.0.0.1");
            TcpListener serverSocket = new TcpListener(ipAddress, 12341);
            serverSocket.Start();
            Console.WriteLine("Klient2: Oczekiwanie...");

            var clientSocket = serverSocket.AcceptTcpClient();
            OCSPRequest ApplicationOcspRequest = StartServerSocket(clientSocket);

            serverSocket.Stop();
            return ApplicationOcspRequest;
        }

        static OCSPRequest StartServerSocket(TcpClient clientSocket)
        {
            NetworkStream networkStream = clientSocket.GetStream();

            var ApplicationOcspRequest = Serializer.DeserializeWithLengthPrefix<OCSPRequest>(networkStream, PrefixStyle.Base128);
            Console.WriteLine("Klient2: Otrzymano " + ApplicationOcspRequest.CertificateID);

            Serializer.SerializeWithLengthPrefix(networkStream, ApplicationOcspRequest, PrefixStyle.Base128);
            return ApplicationOcspRequest;
        }

        static OCSPResponse StartServerSocket1(TcpClient clientSocket)
        {
            NetworkStream networkStream = clientSocket.GetStream();

            var ApplicationOcspResponse = Serializer.DeserializeWithLengthPrefix<OCSPResponse>(networkStream, PrefixStyle.Base128);
            Console.WriteLine("Klient2: Otrzymano " + ApplicationOcspResponse.CertStatus);

            Serializer.SerializeWithLengthPrefix(networkStream, ApplicationOcspResponse, PrefixStyle.Base128);
            return ApplicationOcspResponse;
        }
    }
}