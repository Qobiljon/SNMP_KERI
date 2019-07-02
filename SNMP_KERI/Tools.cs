using System;
using System.Net;
using System.Threading;
using Lextm.SharpSnmpLib;
using System.Collections.Generic;
using Lextm.SharpSnmpLib.Messaging;
using System.Drawing;
using System.Net.NetworkInformation;
using System.IO;
using System.Text;


namespace SNMP_KERI
{
    class Tools
    {
        internal static void init(TopologyVisualizer visualizer)
        {
            if (!Directory.Exists(EVT_LOGS_DIR))
                Directory.CreateDirectory(EVT_LOGS_DIR);
            if (!Directory.Exists(SNMP_DATA_DIR))
                Directory.CreateDirectory(SNMP_DATA_DIR);

            MIB_Info.setUpIdNameMap();
            vis = visualizer;

            if (channels != null)
                channels.Clear();
            else
                channels = new Dictionary<IPAddress, Thread>();
        }

        #region Constants
        public static readonly string EVT_LOGS_DIR = Path.Combine(Environment.CurrentDirectory, "Event-Logs");
        public static readonly string SNMP_DATA_DIR = Path.Combine(Environment.CurrentDirectory, "Snmp-Values");
        #endregion

        #region Variables
        internal static string openLogStreamStamp;
        private static StreamWriter eventLogsWriter;
        private static StreamWriter snmpDataWriter;
        private static TopologyVisualizer vis;
        private static Thread masterThread;
        private static Dictionary<IPAddress, Thread> channels;
        private static LoggerDelegate logDeleg;
        #endregion

        internal static void start_snmp_master(LoggerDelegate loggerDelegate, ThreadStart threadStartAction)
        {
            DateTime nowTimestamp = DateTime.Now;
            nowTimestamp = new DateTime(year: nowTimestamp.Year, month: nowTimestamp.Month, day: nowTimestamp.Day, hour: nowTimestamp.Hour, minute: 0, second: 0);
            openLogStreamStamp = nowTimestamp.ToString("yyyy-MM-dd HH-mm");
            eventLogsWriter = new StreamWriter(path: Path.Combine(EVT_LOGS_DIR, $"{openLogStreamStamp}.txt"), append: true);
            bool writeTitle = !File.Exists(Path.Combine(SNMP_DATA_DIR, $"{openLogStreamStamp}.csv"));
            snmpDataWriter = new StreamWriter(path: Path.Combine(SNMP_DATA_DIR, $"{openLogStreamStamp}.csv"), append: true);
            if (writeTitle)
                snmpDataWriter.WriteLine($"Time, IPAddress, LreLinkStatusA, LreLinkStatusB, LrePortAdminStateA, LrePortAdminStateB, LreCntTxA, LreCntTxB , LreCntTxC, LreCntRxA, LreCntRxB, LreCntRxC, LreCntErrorsA, LreCntErrorsB, LreCntErrorsC, LreCntOwnRxA, LreCntOwnRxB");
            eventLogsWriter.AutoFlush = true;
            snmpDataWriter.AutoFlush = true;
            logDeleg = loggerDelegate;

            if (masterThread == null || masterThread.ThreadState.HasFlag(ThreadState.Aborted))
            {
                masterThread = new Thread((thrStartAction) => runMasterThreadLifecycleAsync(vis, (ThreadStart)thrStartAction));
                masterThread.IsBackground = true;

                logDeleg(message: "Starting SNMP service");
                masterThread.Start(threadStartAction);
            }
        }

        internal static void stop_snmp_master()
        {
            masterThread.Abort();
            masterThread = null;

            foreach (Thread thread in channels.Values)
                if (thread != null && thread.IsAlive)
                    thread?.Abort();
            channels.Clear();

            eventLogsWriter.Flush();
            eventLogsWriter.Close();

            logDeleg(message: "SNMP service has been stopped");

            foreach (TopologyVisualizer.TopologyNode node in vis.Nodes)
                node.brush = Brushes.LightYellow;
            vis.redrawTopology();
        }

        private static void runMasterThreadLifecycleAsync(TopologyVisualizer vis, ThreadStart thrStartAction)
        {
            foreach (TopologyVisualizer.TopologyNode node in vis.Nodes)
            {
                node.brush = Brushes.Yellow;
                if (node.ipAddress == null)
                    continue;

                if (!channels.ContainsKey(node.ipAddress) || channels[node.ipAddress].ThreadState.HasFlag(ThreadState.Aborted))
                {
                    channels[node.ipAddress] = new Thread(() =>
                    {
                        #region Initialize MIB variable ids
                        SnmpValue lreLinkStatusA = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.0.1.0.1.1.9.1"));
                        SnmpValue lreLinkStatusB = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.0.1.0.1.1.10.1"));
                        SnmpValue lrePortAdminStateA = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.0.1.0.1.1.7.1"));
                        SnmpValue lrePortAdminStateB = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.0.1.0.1.1.8.1"));
                        SnmpValue lreCntTxA = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.1.1.0.1.1.2.1"));
                        SnmpValue lreCntTxB = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.1.1.0.1.1.3.1"));
                        SnmpValue lreCntTxC = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.1.1.0.1.1.4.1"));
                        SnmpValue lreCntRxA = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.1.1.0.1.1.8.1"));
                        SnmpValue lreCntRxB = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.1.1.0.1.1.9.1"));
                        SnmpValue lreCntRxC = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.1.1.0.1.1.10.1"));
                        SnmpValue lreCntErrorsA = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.1.1.0.1.1.11.1"));
                        SnmpValue lreCntErrorsB = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.1.1.0.1.1.12.1"));
                        SnmpValue lreCntErrorsC = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.1.1.0.1.1.13.1"));
                        SnmpValue lreCntOwnRxA = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.1.1.0.1.1.25.1"));
                        SnmpValue lreCntOwnRxB = new SnmpValue(new ObjectIdentifier("1.0.62439.2.21.1.1.0.1.1.26.1"));

                        Dictionary<ObjectIdentifier, SnmpValue> snmpIdValueMap = new Dictionary<ObjectIdentifier, SnmpValue>();
                        snmpIdValueMap.Add(lreLinkStatusA.Id, lreLinkStatusA);
                        snmpIdValueMap.Add(lreLinkStatusB.Id, lreLinkStatusB);
                        snmpIdValueMap.Add(lrePortAdminStateA.Id, lrePortAdminStateA);
                        snmpIdValueMap.Add(lrePortAdminStateB.Id, lrePortAdminStateB);
                        snmpIdValueMap.Add(lreCntTxA.Id, lreCntTxA);
                        snmpIdValueMap.Add(lreCntTxB.Id, lreCntTxB);
                        snmpIdValueMap.Add(lreCntTxC.Id, lreCntTxC);
                        snmpIdValueMap.Add(lreCntRxA.Id, lreCntRxA);
                        snmpIdValueMap.Add(lreCntRxB.Id, lreCntRxB);
                        snmpIdValueMap.Add(lreCntRxC.Id, lreCntRxC);
                        snmpIdValueMap.Add(lreCntErrorsA.Id, lreCntErrorsA);
                        snmpIdValueMap.Add(lreCntErrorsB.Id, lreCntErrorsB);
                        snmpIdValueMap.Add(lreCntErrorsC.Id, lreCntErrorsC);
                        snmpIdValueMap.Add(lreCntOwnRxA.Id, lreCntOwnRxA);
                        snmpIdValueMap.Add(lreCntOwnRxB.Id, lreCntOwnRxB);
                        #endregion

                        while (true)
                        {
                            #region Make an SNMP request
                            IList<Variable> result = null;
                            try
                            {
                                Thread.Sleep(1000);
                                result = Messenger.Get(VersionCode.V1, new IPEndPoint(node.ipAddress, 161), new OctetString("public"), MIB_Info.objects, 2000);
                                node.brush = Brushes.Yellow;
                            }
                            catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                            {
                                node.brush = Brushes.Red;
                                node.portA.brush = Brushes.Red;
                                node.portB.brush = Brushes.Red;
                                node.portC.brush = Brushes.Red;

                                lreLinkStatusA.reset();
                                lreLinkStatusB.reset();
                                lrePortAdminStateA.reset();
                                lrePortAdminStateB.reset();
                                lreCntTxA.reset();
                                lreCntTxB.reset();
                                lreCntTxC.reset();
                                lreCntRxA.reset();
                                lreCntRxB.reset();
                                lreCntRxC.reset();
                                lreCntErrorsA.reset();
                                lreCntErrorsB.reset();
                                lreCntErrorsC.reset();
                                lreCntOwnRxA.reset();
                                lreCntOwnRxB.reset();

                                eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tSNMP_REPLY_TIMEOUT\tERROR");
                                logDeleg(node.id, node.ipAddress, "snmp reply timeout", Color.Red);
                                continue;
                            }
                            catch (ThreadAbortException)
                            {
                                return;
                            }
                            #endregion

                            // load the new SNMP value
                            StringBuilder sb = new StringBuilder($"{DateTime.Now.ToString()},{node.ipAddress.ToString()}");
                            foreach (Variable elem in result)
                            {
                                snmpIdValueMap[elem.Id].loadNewValue(int.Parse(elem.Data.ToString()));
                                sb.Append($",{snmpIdValueMap[elem.Id].NewValue}");
                            }
                            snmpDataWriter.WriteLine(sb.ToString());

                            lock (eventLogsWriter)
                            {
                                #region Triggers: warnings and errors on portA
                                if (lreLinkStatusA.NewValue == 2)
                                {
                                    node.portA.brush = Brushes.Red;
                                    eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tA\tLreLinkStatusA\tISEQUAL 2\tERROR");
                                    logDeleg(node.id, node.ipAddress, "LreLinkStatusA ISEQUAL 2", Color.Red);
                                }
                                else if (lrePortAdminStateA.NewValue == 1)
                                {
                                    node.portA.brush = Brushes.Red;
                                    eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tA\tLrePortAdminStateA\tISEQUAL 1\tERROR");
                                    logDeleg(node.id, node.ipAddress, "LrePortAdminStateA ISEQUAL 1", Color.Red);
                                }
                                else if (lreCntRxA.Unchanged)
                                    if (lreCntRxA.UnchangedCounter > 5)
                                    {
                                        node.portA.brush = Brushes.DarkRed;
                                        eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tA\tLreCntRxA\tNO INCREASE\tERROR");
                                        logDeleg(node.id, node.ipAddress, $"LreCntRxA={lreCntRxA.NewValue}, NO INCREASE {lreCntRxA.UnchangedCounter} TIMES", Color.Red);
                                    }
                                    else
                                    {
                                        node.portA.brush = Brushes.Orange;
                                        eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tA\tLreCntRxA\tNO INCREASE\tWARNING");
                                        logDeleg(node.id, node.ipAddress, $"LreCntRxA={lreCntRxA.NewValue}, NO INCREASE {lreCntRxA.UnchangedCounter} TIMES", Color.DarkOrange);
                                    }
                                else
                                    node.portA.brush = Brushes.Green;
                                #endregion

                                #region Triggers: warnings and errors on portB
                                if (lreLinkStatusB.NewValue == 2)
                                {
                                    node.portB.brush = Brushes.Red;
                                    eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tB\tLreLinkStatusB\tISEQUAL 2\tERROR");
                                    logDeleg(node.id, node.ipAddress, "LreLinkStatusB ISEQUAL 2", Color.Red);
                                }
                                else if (lrePortAdminStateB.NewValue == 1)
                                {
                                    node.portB.brush = Brushes.Red;
                                    eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tB\tLrePortAdminStateB\tISEQUAL 1\tERROR");
                                    logDeleg(node.id, node.ipAddress, "LrePortAdminStateB ISEQUAL 1", Color.Red);
                                }
                                else if (lreCntRxB.Unchanged)
                                    if (lreCntRxB.UnchangedCounter > 5)
                                    {
                                        node.portB.brush = Brushes.DarkRed;
                                        eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tB\tLreCntRxB\tNO INCREASE\tERROR");
                                        logDeleg(node.id, node.ipAddress, $"LreCntRxB={lreCntRxB.NewValue}, NO INCREASE {lreCntRxB.UnchangedCounter} TIMES", Color.Red);
                                    }
                                    else
                                    {
                                        node.portB.brush = Brushes.Orange;
                                        eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tB\tLreCntRxB\tNO INCREASE\tWARNING");
                                        logDeleg(node.id, node.ipAddress, $"LreCntRxB={lreCntRxB.NewValue}, NO INCREASE {lreCntRxB.UnchangedCounter} TIMES", Color.DarkOrange);
                                    }
                                else
                                    node.portB.brush = Brushes.Green;
                                #endregion

                                #region Triggers: warnings and errors on portC
                                if (lreCntRxC.Unchanged)
                                {
                                    string cPortNaming = "C";
                                    if (node.type == TopologyVisualizer.TopologyNode.TpNodeType.REDBOXP || node.type == TopologyVisualizer.TopologyNode.TpNodeType.REDBOXH)
                                        cPortNaming = "IL";
                                    else if (node.type == TopologyVisualizer.TopologyNode.TpNodeType.DANP || node.type == TopologyVisualizer.TopologyNode.TpNodeType.DANH || node.type == TopologyVisualizer.TopologyNode.TpNodeType.VDANP || node.type == TopologyVisualizer.TopologyNode.TpNodeType.VDANH)
                                        cPortNaming = "APP";
                                    if (lreCntRxC.UnchangedCounter > 5)
                                    {
                                        node.portC.brush = Brushes.DarkRed;
                                        eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\t{cPortNaming}\tLreCntRxC\tNO INCREASE\tERROR");
                                        logDeleg(node.id, node.ipAddress, $"LreCntRxC={lreCntRxC.NewValue}, NO INCREASE {lreCntRxC.UnchangedCounter} TIMES", Color.Red);
                                    }
                                    else
                                    {
                                        node.portC.brush = Brushes.Orange;
                                        eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\t{cPortNaming}\tLreCntRxC\tNO INCREASE\tWARNING");
                                        logDeleg(node.id, node.ipAddress, $"LreCntRxC={lreCntRxC.NewValue}, NO INCREASE {lreCntRxC.UnchangedCounter} TIMES", Color.DarkOrange);
                                    }
                                }
                                else
                                    node.portC.brush = Brushes.Green;
                                #endregion

                                #region Triggers: errors on MIB
                                bool mibError = false;
                                if (lreLinkStatusA.NewValue != 1 && lreLinkStatusA.NewValue != 2)
                                {
                                    node.mibBrush = Brushes.Red;
                                    eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tA\tLreLinkStatusA\tMIB\tERROR");
                                    mibError = true;
                                }
                                if (lreLinkStatusB.NewValue != 1 && lreLinkStatusB.NewValue != 2)
                                {
                                    node.mibBrush = Brushes.Red;
                                    eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tB\tLreLinkStatusB\tMIB\tERROR");
                                    mibError = true;
                                }
                                if (lrePortAdminStateA.NewValue != 1 && lrePortAdminStateA.NewValue != 2)
                                {
                                    node.mibBrush = Brushes.Red;
                                    eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tA\tLrePortAdminStateA\tMIB\tERROR");
                                    mibError = true;
                                }
                                if (lrePortAdminStateB.NewValue != 1 && lrePortAdminStateB.NewValue != 2)
                                {
                                    node.mibBrush = Brushes.Red;
                                    eventLogsWriter.WriteLine($"{DateTime.Now.ToString()}\t{node.ipAddress.ToString()}\tB\tLrePortAdminStateB\tMIB\tERROR");
                                    mibError = true;
                                }
                                if (!mibError)
                                    node.mibBrush = Brushes.Black;

                                #endregion
                            }
                        }
                    });
                    channels[node.ipAddress].IsBackground = true;
                    channels[node.ipAddress].Start();
                }
            }

            vis.redrawTopology();
            thrStartAction.BeginInvoke(null, null);

            while (true)
            {
                Thread.Sleep(1000);
                vis.redrawTopology();
                lock (eventLogsWriter)
                    lock (snmpDataWriter)
                    {
                        checkUpdateCurrentLogStream();
                    }
            }
        }

        internal static string phyAddr2VisualString(PhysicalAddress addr)
        {
            string res = addr?.ToString().ToLower();
            for (int n = res.Length - 2; res != null && n > 1; n -= 2)
                res = res.Insert(n, "-");
            return res;
        }

        internal static bool extractThePort(TopologyVisualizer.TopologyNode node, string dbElemName, out TopologyVisualizer.TopologyPort thePort)
        {
            if (dbElemName.StartsWith("rx_") || dbElemName.StartsWith("status_") || dbElemName.StartsWith("port_"))
                return node.getPortByDbName(dbElemName, out thePort);
            thePort = null;
            return false;
        }

        internal delegate void LoggerDelegate(int nodeId = default(int), IPAddress ipAddress = default(IPAddress), string message = default(string), Color color = default(Color));

        private static void checkUpdateCurrentLogStream()
        {
            DateTime nowTimestamp = DateTime.Now;
            nowTimestamp = new DateTime(year: nowTimestamp.Year, month: nowTimestamp.Month, day: nowTimestamp.Day, hour: nowTimestamp.Hour, minute: 0, second: 0);
            string nowStamp = nowTimestamp.ToString("yyyy-MM-dd HH-mm");

            if (!nowStamp.Equals(openLogStreamStamp))
                try
                {
                    eventLogsWriter?.Flush();
                    eventLogsWriter?.Close();
                    snmpDataWriter?.Flush();
                    snmpDataWriter?.Close();
                }
                catch
                {

                }
                finally
                {
                    openLogStreamStamp = nowStamp;
                    eventLogsWriter = new StreamWriter(path: Path.Combine(EVT_LOGS_DIR, $"{nowStamp}.txt"), append: false);
                    snmpDataWriter = new StreamWriter(path: Path.Combine(SNMP_DATA_DIR, $"{nowStamp}.csv"), append: false);
                    snmpDataWriter.WriteLine($"Time, IPAddress, LreLinkStatusA, LreLinkStatusB, LrePortAdminStateA, LrePortAdminStateB, LreCntTxA, LreCntTxB , LreCntTxC, LreCntRxA, LreCntRxB, LreCntRxC, LreCntErrorsA, LreCntErrorsB, LreCntErrorsC, LreCntOwnRxA, LreCntOwnRxB{Environment.NewLine}");
                }
        }

        internal static void releaseStreams()
        {
            eventLogsWriter?.Flush();
            eventLogsWriter?.Close();
            snmpDataWriter?.Flush();
            snmpDataWriter?.Close();
        }
    }

    static class MIB_Info
    {
        #region Variables
        internal static IList<Variable> objects;
        private static Dictionary<ObjectIdentifier, string> objectNames;
        private static bool initialized = false;
        #endregion

        internal static void setUpIdNameMap()
        {
            if (initialized)
                return;

            objects?.Clear();
            objects = new List<Variable>();
            objectNames = new Dictionary<ObjectIdentifier, string>();

            addElem("1.0.62439.2.21.0.1.0.1.1.9.1", "status_a"); // lreLinkStatusA
            addElem("1.0.62439.2.21.0.1.0.1.1.10.1", "status_b"); // lreLinkStatusB
            addElem("1.0.62439.2.21.0.1.0.1.1.7.1", "port_a"); // lrePortAdminStateA
            addElem("1.0.62439.2.21.0.1.0.1.1.8.1", "port_b"); // lrePortAdminStateB
            addElem("1.0.62439.2.21.1.1.0.1.1.2.1", "tx_a"); // lreCntTxA
            addElem("1.0.62439.2.21.1.1.0.1.1.3.1", "tx_b"); // lreCntTxB 
            addElem("1.0.62439.2.21.1.1.0.1.1.4.1", "tx_c"); // lreCntTxC
            addElem("1.0.62439.2.21.1.1.0.1.1.8.1", "rx_a"); // lreCntRxA
            addElem("1.0.62439.2.21.1.1.0.1.1.9.1", "rx_b"); // lreCntRxB
            addElem("1.0.62439.2.21.1.1.0.1.1.10.1", "rx_c"); // lreCntRxC
            addElem("1.0.62439.2.21.1.1.0.1.1.11.1", "error_a"); // lreCntErrorsA
            addElem("1.0.62439.2.21.1.1.0.1.1.12.1", "error_b"); // lreCntErrorsB
            addElem("1.0.62439.2.21.1.1.0.1.1.13.1", "error_c");  // lreCntErrorsC
            addElem("1.0.62439.2.21.1.1.0.1.1.25.1", "o_rx_a"); // lreCntOwnRxA
            addElem("1.0.62439.2.21.1.1.0.1.1.26.1", "o_rx_b"); // lreCntOwnRxB

            initialized = true;
        }

        private static void addElem(string idStr, string db_name)
        {
            ObjectIdentifier id = new ObjectIdentifier(idStr);
            objectNames.Add(id, db_name);
            objects.Add(new Variable(id));
        }

        internal static string getName(ObjectIdentifier objectId)
        {
            return objectNames[objectId];
        }
    }

    internal class SnmpValue
    {
        internal SnmpValue(ObjectIdentifier id)
        {
            Id = id;
            Name = MIB_Info.getName(id);
            OldValue = -1;
            NewValue = -1;

            Unchanged = false;
            UnchangedCounter = 0;
            Increased = false;
            IncreasedCounter = 0;
            Decreased = false;
            DecreasedCounter = 0;
        }

        #region Variables
        public ObjectIdentifier Id { get; }
        public string Name { get; }
        public int OldValue { get; set; }
        public int NewValue { get; set; }

        internal bool Unchanged { get; set; }
        internal int UnchangedCounter { get; set; }
        internal bool Increased { get; set; }
        internal int IncreasedCounter { get; set; }
        internal bool Decreased { get; set; }
        internal int DecreasedCounter { get; set; }
        #endregion

        internal void reset()
        {
            OldValue = -1;
            NewValue = -1;

            Unchanged = false;
            UnchangedCounter = 0;
            Increased = false;
            IncreasedCounter = 0;
            Decreased = false;
            DecreasedCounter = 0;
        }

        internal void loadNewValue(int newValue)
        {
            OldValue = NewValue;
            NewValue = newValue;

            if (OldValue == -1)
            {
                Unchanged = false;
                Increased = false;
                Decreased = false;
            }
            else
            {
                Unchanged = NewValue == OldValue;
                Increased = NewValue > OldValue;
                Decreased = NewValue < OldValue;
            }

            if (Unchanged)
            {
                UnchangedCounter++;
                IncreasedCounter = 0;
                DecreasedCounter = 0;
            }
            else if (Increased)
            {
                IncreasedCounter++;
                UnchangedCounter = 0;
                DecreasedCounter = 0;
            }
            else if (Decreased)
            {
                DecreasedCounter++;
                UnchangedCounter = 0;
                IncreasedCounter = 0;
            }
        }
    }
}
