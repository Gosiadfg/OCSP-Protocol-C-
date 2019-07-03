using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using ProtoBuf;
using System.Net;
using System.IO;

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
            var ApplicationOcspResponse = new OCSPResponse();
            ApplicationOcspResponse = StartAcceptor();
            StartClient(ApplicationOcspResponse);
            Console.ReadKey();
        }

        static void StartClient(OCSPResponse dane)
        {
            var client = new TcpClient();
            string clntIP;
            Console.WriteLine("Podaj adres IP adresata odpowiedzi: ");
            clntIP = Console.ReadLine();
            client.Connect(IPAddress.Parse(clntIP), 12341);

            NetworkStream serverStream = client.GetStream();

            var ApplicationOcspResponse = new OCSPResponse();
            ApplicationOcspResponse = dane;

            Serializer.SerializeWithLengthPrefix(serverStream, ApplicationOcspResponse, PrefixStyle.Base128);
            Console.WriteLine("Serwer: Przesłano " + ApplicationOcspResponse.CertStatus);
        }

        static OCSPResponse StartAcceptor()
        {
            string srvIP;
            Console.WriteLine("Podaj IP serwera: ");
            srvIP = Console.ReadLine();
            TcpListener serverSocket = new TcpListener(IPAddress.Parse(srvIP), 12340);
            serverSocket.Start();
            Console.WriteLine("Serwer: Oczekiwanie...");

            var clientSocket = serverSocket.AcceptTcpClient();
            OCSPResponse ApplicationOcspRequest = StartServerSocket(clientSocket);
            return ApplicationOcspRequest;
        }

        static OCSPResponse StartServerSocket(TcpClient clientSocket)
        {
            NetworkStream networkStream = clientSocket.GetStream();

            var ApplicationOcspRequest = Serializer.DeserializeWithLengthPrefix<OCSPRequest>(networkStream, PrefixStyle.Base128);
            Console.WriteLine("Serwer: Otrzymano " + ApplicationOcspRequest.CertificateID);

            var ApplicationOcspResponse = new OCSPResponse(); // stworzenie response
            string status = "";
            string line = "";
            string local;
            Console.WriteLine("Podaj sciezke do pliku z certyfikatami (C:\\Users\\User\\Desktop\\Foo.txt");
            local = Console.ReadLine();
            StreamReader file = new StreamReader(@local);
            while ((line = file.ReadLine()) != null)
            {
                if (ApplicationOcspRequest.CertificateID.Length == 1)
                {
                    if (line[0] == ApplicationOcspRequest.CertificateID[0])
                    {
                        for (int i = 2; i < line.Length; i++) status = status + line[i];
                        Console.WriteLine(status);
                        ApplicationOcspResponse.CertStatus = status;  //nadanie statusu response
                        break;
                    }
                }

                if (ApplicationOcspRequest.CertificateID.Length == 2)
                {
                    if (line[0] == ApplicationOcspRequest.CertificateID[0])
                    {
                        if (line[1] == ApplicationOcspRequest.CertificateID[1])
                        {
                            for (int i = 3; i < line.Length; i++) status = status + line[i];
                            Console.WriteLine(status);
                            ApplicationOcspResponse.CertStatus = status; //nadanie statusu response
                            break;
                        }
                    }
                }
            }

            file.Close();
            Serializer.SerializeWithLengthPrefix(networkStream, ApplicationOcspRequest, PrefixStyle.Base128);


            DateTime dt = DateTime.Now;
            ApplicationOcspResponse.OCSPResponseData = "";
            ApplicationOcspResponse.OCSPREsponseStatus = "successful";
            ApplicationOcspResponse.ResponseType = "Basic OCSP Response";
            ApplicationOcspResponse.Version = 1;
            ApplicationOcspResponse.ResponderID = "";
            ApplicationOcspResponse.Technologies = "";
            ApplicationOcspResponse.ProducedAt = dt.ToString();
            ApplicationOcspResponse.Responses = "";
            ApplicationOcspResponse.CertificateID = "";
            ApplicationOcspResponse.HashAlgorithm = "SHA1";
            ApplicationOcspRequest.IssuerNameHash = "M7UBSJK9MTNGFIO96QF1GHFA7UCEHJKTG5UYE5XROBY";
            ApplicationOcspRequest.IssuerKeyHash = "8Z6D03Y285CJOIE97J5V1KXQ47SNECDD29HS35L21NC";
            ApplicationOcspRequest.SerialNumber = "FIL2YF0HT4KW8CWFVPB7I2KK87RER8GWHRP6XFEHAO3";
            ApplicationOcspResponse.ThisUpdate = dt.ToString();
            ApplicationOcspResponse.NextUpdate = "1.01.2016";

            return ApplicationOcspResponse;
        }
    }
}