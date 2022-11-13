using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangFromTextWinApp.Helpers
{
    public class NamedPipeTools
    {
        public const byte COMM_SHOWWINDOW = 0xAA;

        public void StartNamedPipeServer(Action<int> OnDataReceived)
        {
            Task.Run(() =>
               {
                   while (true)
                   {
                       using (NamedPipeServerStream namedPipeServer = new NamedPipeServerStream(FEConstants.PRODUCT_NAME_Unique))
                       {
                           // POZOR: Ak nejde ako admin neda sa DEBUGOVAT - v DEbug mode sa druhemu klientovi nepodari namedPipeClient.Connect kvoli Access denied
                           namedPipeServer.WaitForConnection();

                           int command = namedPipeServer.ReadByte();
                           OnDataReceived?.Invoke(command);
                       }
                   }
               });
        }

        public static void ClientSendData()
        {
            using (NamedPipeClientStream namedPipeClient = new NamedPipeClientStream(FEConstants.PRODUCT_NAME_Unique))
            {
                namedPipeClient.Connect(500);
                namedPipeClient.WriteByte(COMM_SHOWWINDOW);
            }
        }
    }
}
