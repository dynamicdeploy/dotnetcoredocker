using System;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Text;
using System.Net;
namespace onerowcollector
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string host = args[0];

                try
                {
                    Console.WriteLine("Resolving host....");
                    var iph = Dns.GetHostEntry(host);

                    if (iph != null)
                    {
                        string ip = (iph != null && iph.AddressList != null && iph.AddressList.Length > 0) ? iph.AddressList[0].ToString() : string.Empty;
                        Console.WriteLine($"Resolved {host} to {iph.HostName} and ipaddress {ip} ");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
              
                    try
                    {
                        Console.WriteLine("Running ping...");
                        for (int i = 0; i < 10; i++)
                        {
                            Ping pingSender = new Ping();
                            PingOptions options = new PingOptions();

                            // Use the default Ttl value which is 128,
                            // but change the fragmentation behavior.
                            options.DontFragment = true;

                            // Create a buffer of 32 bytes of data to be transmitted.
                            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                            byte[] buffer = Encoding.ASCII.GetBytes(data);
                            int timeout = 120;
                            PingReply reply = pingSender.Send(host, timeout, buffer, options);
                            if (reply.Status == IPStatus.Success)
                            {
                                Console.WriteLine("Address: {0}", reply.Address.ToString());
                                Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                                //Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                                //Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                                //Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
               

               
                    Console.WriteLine("Running traceroute...");
                    try
                    {
                        Traceroute.TryTraceRouteAsync(host).Wait();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                }
                else
                {

                    Console.WriteLine("Usage dotnet onerowcollector.dll [hostorip]");
                }

                Console.ReadLine();
            
        }
    }
}
