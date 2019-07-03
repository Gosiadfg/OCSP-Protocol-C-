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
            StartClient();
            StartAcceptor();
            Console.ReadKey();
        }

        static void StartAcceptor()
        {
            var ipAddress = IPAddress.Parse("127.0.0.1");
            TcpListener serverSocket = new TcpListener(ipAddress, 12342);
            try { serverSocket.Start(); }
            catch { StartAcceptor(); }
            Console.WriteLine("Klient1: Oczekiwanie...");


            var clientSocket = serverSocket.AcceptTcpClient();
            StartServerSocket(clientSocket);
        }

        static void StartServerSocket(TcpClient clientSocket)
        {

            using (NetworkStream networkStream = clientSocket.GetStream())
            {
                var ApplicationOcspResponse = Serializer.DeserializeWithLengthPrefix<OCSPResponse>(networkStream, PrefixStyle.Base128);
                Console.WriteLine("Klient1: Otrzymano " + ApplicationOcspResponse.CertStatus);

                Serializer.SerializeWithLengthPrefix(networkStream, ApplicationOcspResponse, PrefixStyle.Base128);
            }
        }

        static void StartClient()
        {
            var client = new TcpClient();
            string clnt2IP;
            Console.WriteLine("Podaj IP adresata certyfikatu: ");
            clnt2IP = Console.ReadLine();
            Console.WriteLine("Klient1: Podaj nr certyfikatu 1-50");
            client.Connect(IPAddress.Parse(clnt2IP), 12341);

            NetworkStream serverStream = client.GetStream();

            var ApplicationOcspRequest = new OCSPRequest();
            string dane;
            dane = Console.ReadLine();
            ApplicationOcspRequest.CertificateID = dane;
            ApplicationOcspRequest.OCSPRequestData = "";
            ApplicationOcspRequest.Version = 1;
            ApplicationOcspRequest.RequestorList = "";
            ApplicationOcspRequest.HashAlgorithm = "SHA1";
            ApplicationOcspRequest.IssuerNameHash = "M7UBSJK9MTNGFIO96QF1GHFA7UCEHJKTG5UYE5XROBY";
            ApplicationOcspRequest.IssuerKeyHash = "8Z6D03Y285CJOIE97J5V1KXQ47SNECDD29HS35L21NC";
            ApplicationOcspRequest.SerialNumber = "FIL2YF0HT4KW8CWFVPB7I2KK87RER8GWHRP6XFEHAO3";
            Serializer.SerializeWithLengthPrefix(serverStream, ApplicationOcspRequest, PrefixStyle.Base128);
        }
    }
}