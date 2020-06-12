using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace onerowcollector
{
    public class Traceroute
    {
        private const string DATA = "onerow - http://onerow.io"; //60 Bytes "
        private static readonly byte[] _buffer = Encoding.ASCII.GetBytes(DATA);
        private const int MAX_HOPS = 15;
        private const string STR_REQUEST_TIMEOUT = "Request timed out.";
        private const string STR_REQUEST_TIME_NA = "*";
        private const int REQUEST_TIMEOUT = 4000;

        /// <summary>
        /// Runs traceroute and writes result to console.
        /// </summary>
        public static async Task TryTraceRouteAsync(string hostNameOrAddress)
        {
            EnsureCommonArguments(hostNameOrAddress);
            Contract.EndContractBlock();

            using (var console = Console.OpenStandardOutput())
            using (var sw = new StreamWriter(console))
            {
                await TryTraceRouteAsync(hostNameOrAddress, sw);
            }
        }

        /// <summary>
        /// Runs traceroute and writes result to provided stream.
        /// </summary>
        public static async Task TryTraceRouteAsync(string hostNameOrAddress, StreamWriter outputStreamWriter)
        {
            EnsureCommonArguments(hostNameOrAddress);
            if (outputStreamWriter == null)
            {
                throw new ArgumentNullException(nameof(outputStreamWriter));
            }
            Contract.EndContractBlock();

            await outputStreamWriter.WriteLineAsync($"traceroute to {hostNameOrAddress}, {MAX_HOPS} hops max, {_buffer.Length} byte packets");

            //dispatch parallel tasks for each hop
            var arrTraceRouteTasks = new Task<TraceRouteResult>[MAX_HOPS];
            for (int zeroBasedHop = 0; zeroBasedHop < MAX_HOPS; zeroBasedHop++)
            {
                arrTraceRouteTasks[zeroBasedHop] = TryTraceRouteInternalAsync(hostNameOrAddress, zeroBasedHop);
            }

            //and wait for them to finish
            await Task.WhenAll(arrTraceRouteTasks);

            //now just collect all results and write them to output stream
            for (int hop = 0; hop < MAX_HOPS; hop++)
            {
                var traceTask = arrTraceRouteTasks[hop];
                if (traceTask.Status == TaskStatus.RanToCompletion)
                {
                    var res = traceTask.Result;
                    await outputStreamWriter.WriteLineAsync(res.Message);

                    if (res.IsComplete)
                    {
                        //trace complete
                        break;
                    }
                }
                else
                {
                    await outputStreamWriter.WriteLineAsync($"Could not get result for hop #{hop + 1}");
                }
            }
        }

        private static void EnsureCommonArguments(string hostNameOrAddress)
        {
            if (hostNameOrAddress == null)
            {
                throw new ArgumentNullException(nameof(hostNameOrAddress));
            }

            if (string.IsNullOrWhiteSpace(hostNameOrAddress))
            {
                throw new ArgumentException("Hostname or address is required", nameof(hostNameOrAddress));
            }
        }

        public class TraceRouteResult
        {
            public TraceRouteResult(string message, bool isComplete)
            {
                Message = message;
                IsComplete = isComplete;
            }

            public string Message
            {
                get; private set;
            }

            public bool IsComplete
            {
                get; private set;
            }
        }

        public static async Task<TraceRouteResult> TryTraceRouteInternalAsync(string hostNameOrAddress, int zeroBasedHop)
        {
            using (System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping())
            {
                var hop = zeroBasedHop + 1;

                PingOptions pingOptions = new PingOptions();
                Stopwatch stopWatch = new Stopwatch();
                pingOptions.DontFragment = true;
                pingOptions.Ttl = hop;

                stopWatch.Start();

                PingReply pingReply = await pingSender.SendPingAsync(
                    hostNameOrAddress,
                    REQUEST_TIMEOUT,
                    _buffer,
                    pingOptions
                );

                stopWatch.Stop();

                var elapsedMilliseconds = stopWatch.ElapsedMilliseconds;

                string pingReplyAddress;
                string strElapsedMilliseconds;

                if (pingReply.Status == IPStatus.TimedOut)
                {
                    pingReplyAddress = STR_REQUEST_TIMEOUT;
                    strElapsedMilliseconds = STR_REQUEST_TIME_NA;
                }
                else
                {
                    pingReplyAddress = pingReply.Address.ToString();
                    strElapsedMilliseconds = $"{elapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture)} ms";
                }

                var traceResults = new StringBuilder();
                traceResults.Append(hop.ToString(System.Globalization.CultureInfo.InvariantCulture).PadRight(4, ' '));
                traceResults.Append(strElapsedMilliseconds.PadRight(10, ' '));
                traceResults.Append(pingReplyAddress);

                return new TraceRouteResult(traceResults.ToString(), pingReply.Status == IPStatus.Success);
            }
        }
    }
}
