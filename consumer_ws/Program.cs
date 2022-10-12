using System.Configuration;
using System.Collections.Specialized;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace consumer_ws
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (Socket ws = new Socket(SocketType.Stream, ProtocolType.Unspecified))
            {
                var ip = ConfigurationManager.AppSettings["server"];
                var port = Int32.Parse(ConfigurationManager.AppSettings["port"]);
                Console.WriteLine("Connecting to {0}:{1}", ip, port);
                
                ws.Connect(ip, port);
                Console.Write("\t1. Upload\n\t2. Download\nSelect an option: ");
                var opt = Console.ReadLine();
                switch (opt)
                {
                    case "1":
                        ws.Send(Encoding.ASCII.GetBytes("uplo"));
                        Console.Clear();
                        Console.Write("Enter file name to upload: ");
                        String fileName = Console.ReadLine();
                        try
                        {
                            ws.Send(File.ReadAllBytes(ConfigurationManager.AppSettings["upload_dir"] + fileName));
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ERROR: " + e.Message);
                        }
                        break;
                    case "2":
                        ws.Send(Encoding.ASCII.GetBytes("down"));
                        byte[] rec = new byte[256];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            Console.Clear();
                            Console.Write("Enter id of the file: ");
                            var id = Console.ReadLine();
                            ws.Send(Encoding.ASCII.GetBytes(id));
                            while (ws.Receive(rec) != 0)
                            {
                                ms.Write(rec, 0, rec.Length);
                            }
                            if (ms.Length == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("ERROR: No data received.");
                            }else if (Encoding.ASCII.GetString(ms.ToArray()).Contains("ERROR:"))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(Encoding.ASCII.GetString(ms.ToArray()));
                            }
                            else
                            {
                                try
                                {

                                    File.WriteAllBytes(ConfigurationManager.AppSettings["download_dir"] + "/result_client.pdf", ms.ToArray());
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    var guid = Guid.NewGuid();
                                    Console.WriteLine("File successfuly written to: " + ConfigurationManager.AppSettings["download_dir"] + "\n" + guid + ".pdf");
                                }
                                catch(Exception e)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("ERROR: {0}", e.Message);
                                }
                            }
                        }
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: {0} is not an option.", opt);
                        break;
                }
            }
        }
    }
}
