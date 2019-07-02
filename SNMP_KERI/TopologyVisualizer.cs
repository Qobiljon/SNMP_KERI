using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using System;
using System.IO;

namespace SNMP_KERI
{
    // Class for visualizing nodes (topology) on canvas
    class TopologyVisualizer
    {
        internal enum TpClickAction
        {
            NONE,
            MOVE_LOCATION,
            SET_CONNECTIONS,
            SET_NODE_TYPE,
            SET_NODE_IP,
            SET_NODE_MAC
        }

        #region Constants
        internal const short NTITLE_MARGIN = 15; // node<->title margin
        internal const short NODE_WIDTH = 60; // divisible by 6
        internal const short NODE_HEIGHT = 60; // divisible by 6
        internal const short MAX_LOG_LINES_COUNT = 250;
        #endregion

        #region Variables
        PictureBox canvasPictureBox;
        Graphics canvas;
        private Font font;
        private TextBox logTextBox;

        internal TpClickAction clickAction;

        private Dictionary<short, TopologyNode> nodes;
        internal TopologyNode[] Nodes
        {
            get
            {
                TopologyNode[] res = new TopologyNode[nodes.Count];
                nodes.Values.CopyTo(res, 0);
                return res;
            }
        }
        internal int NumOfNodes { get { return nodes.Count; } }
        internal bool IsEmpty { get { return nodes == null || nodes.Count == 0; } }
        private short nextNodeId;
        private bool translateNode;
        private short mouseLocDiffX, mouseLocDiffY;
        private short origNodePosX, origNodePosY;
        private TopologyNode activeNode;
        private TopologyPort activePort;
        private Size pBoxSize;
        private short logLinesCount;

        private ShowDetailsDelegate showDetailsDelegate;
        private HideDetailsDelegate hideDetailsDelegate;
        #endregion

        internal TopologyVisualizer(PictureBox topologyPictureBox, TextBox logTextBox = null, ShowDetailsDelegate showDetailsDelegate = null, HideDetailsDelegate hideDetailsDelegate = null)
        {
            this.translateNode = false;
            this.clickAction = TpClickAction.MOVE_LOCATION;
            this.activePort = null;
            this.activeNode = null;

            this.canvasPictureBox = topologyPictureBox;
            this.canvas = topologyPictureBox.CreateGraphics();
            this.font = topologyPictureBox.Font;
            this.nodes = new Dictionary<short, TopologyNode>();
            this.nextNodeId = 0;
            this.pBoxSize = topologyPictureBox.Size;

            topologyPictureBox.MouseDown += nodeMouseDownHandler;
            topologyPictureBox.MouseUp += nodeMouseUpHandler;
            topologyPictureBox.MouseMove += nodeMouseMoveHandler;

            topologyPictureBox.InitialImage = null;
            this.logTextBox = logTextBox;
            this.showDetailsDelegate = showDetailsDelegate;
            this.hideDetailsDelegate = hideDetailsDelegate;

            clickAction = TpClickAction.NONE;

            clearCanvas();
        }

        internal TopologyNode addNode(short posX, short posY, short rPortConn = TopologyPort.DISCONNECTED, short lPortConn = TopologyPort.DISCONNECTED, short tPortConn = TopologyPort.DISCONNECTED)
        {
            TopologyNode newNode = new TopologyNode(nextNodeId, posX, posY, NODE_WIDTH, NODE_HEIGHT, NTITLE_MARGIN);
            newNode.setRightPortConn(rPortConn);
            newNode.setLeftPortConn(lPortConn);
            newNode.setTopPortConn(tPortConn);
            nodes.Add(newNode.id, newNode);
            nextNodeId++;
            return newNode;
        }

        private void drawNodeRect(TopologyNode node)
        {
            // Port-Boxes' dimensions (inner rectangles)
            short pBoxW = (short)(node.portC.endLoc.x - node.portC.startLoc.x);
            short pBoxH = (short)(node.portC.endLoc.y - node.portC.startLoc.y);

            // Node title
            if (node.ipAddress == null)
                canvas.DrawString(string.Format("ID: {0}", node.id), font, Brushes.Black, node.startLoc.x, node.startLoc.y - node.topMargin);
            else
                canvas.DrawString(string.Format("IP: {0}", node.ipAddress?.ToString()), font, Brushes.Black, node.startLoc.x - 10, node.startLoc.y - node.topMargin);

            // Body
            canvas.DrawRectangle(Pens.Black, node.startLoc.x, node.startLoc.y, NODE_WIDTH, NODE_HEIGHT);
            canvas.FillRectangle(node.brush, node.startLoc.x + 1, node.startLoc.y + 1, NODE_WIDTH - 1, NODE_HEIGHT - 1);

            // Top port
            canvas.DrawRectangle(Pens.Black, node.portC.startLoc.x, node.portC.startLoc.y, pBoxW, pBoxH);
            canvas.FillRectangle(node.portC.brush, node.portC.startLoc.x + 1, node.portC.startLoc.y + 1, pBoxW - 1, pBoxH - 1);

            // Right port
            canvas.DrawRectangle(Pens.Black, node.portB.startLoc.x, node.portB.startLoc.y, pBoxW, pBoxH);
            canvas.FillRectangle(node.portB.brush, node.portB.startLoc.x + 1, node.portB.startLoc.y + 1, pBoxW - 1, pBoxH - 1);

            // Left port
            canvas.DrawRectangle(Pens.Black, node.portA.startLoc.x, node.portA.startLoc.y, pBoxW, pBoxH);
            canvas.FillRectangle(node.portA.brush, node.portA.startLoc.x + 1, node.portA.startLoc.y + 1, pBoxW - 1, pBoxH - 1);
        }

        private void eraseNodeRect(TopologyNode node)
        {
            // Port-Boxes' dimensions (inner rectangles)
            short pBoxW = (short)(node.portC.endLoc.x - node.portC.startLoc.x);
            short pBoxH = (short)(node.portC.endLoc.y - node.portC.startLoc.y);

            canvas.FillRectangle(Brushes.White, node.startLoc.x - 10, node.startLoc.y - node.topMargin, NODE_WIDTH + 60, node.topMargin);

            // Body
            canvas.FillRectangle(Brushes.White, node.startLoc.x, node.startLoc.y, NODE_WIDTH + 1, NODE_HEIGHT + 1);
        }

        private TopologyPort getPortAt(TopologyNode node, int x, int y)
        {
            if (node.isInLocation(x, y))
                if (node.portA.isInLocation(x, y))
                    return node.portA;
                else if (node.portB.isInLocation(x, y))
                    return node.portB;
                else if (node.portC.isInLocation(x, y))
                    return node.portC;
            return null;
        }

        private void clearPortConnections(params TopologyPort[] ports)
        {
            foreach (TopologyPort port in ports)
            {
                if (port.connPortId != TopologyPort.DISCONNECTED)
                {
                    TopologyNode node = nodes[port.connPortId];
                    if (node.portA.connPortId == port.parent.id)
                    {
                        node.portA.connPortId = TopologyPort.DISCONNECTED;
                        node.portA.connPortType = TopologyPort.TpPortType.NULL;
                        node.portA.connLinkDrawn = false;
                    }
                    if (node.portB.connPortId == port.parent.id)
                    {
                        node.portB.connPortId = TopologyPort.DISCONNECTED;
                        node.portB.connPortType = TopologyPort.TpPortType.NULL;
                        node.portB.connLinkDrawn = false;
                    }
                    if (node.portC.connPortId == port.parent.id)
                    {
                        node.portC.connPortId = TopologyPort.DISCONNECTED;
                        node.portC.connPortType = TopologyPort.TpPortType.NULL;
                        node.portC.connLinkDrawn = false;
                    }
                }
                port.connPortId = TopologyPort.DISCONNECTED;
                port.connPortType = TopologyPort.TpPortType.NULL;
                port.connLinkDrawn = false;
            }
        }

        private void attachPorts(TopologyPort port1, TopologyPort port2)
        {
            port1.connPortId = port2.parent.id;
            port1.connPortType = port2.type;
            port1.connLinkDrawn = false;

            port2.connPortId = port1.parent.id;
            port2.connPortType = port1.type;
            port2.connLinkDrawn = false;
        }

        internal void redrawTopology(bool bringUpFocusNode = false)
        {
            canvasPictureBox.Invoke((MethodInvoker)delegate
            {
                // Clear out the existing topology
                clearCanvas();

                // Draw nodes
                foreach (TopologyNode node in nodes.Values)
                    if (bringUpFocusNode)
                        if (node != activeNode)
                            drawNodeRect(node);
                        else
                            continue;
                    else
                        drawNodeRect(node);

                if (bringUpFocusNode)
                    drawNodeRect(activeNode);

                // Draw connections
                foreach (TopologyNode srcNode in nodes.Values)
                {
                    if (!srcNode.portA.connLinkDrawn && srcNode.portA.connPortId != TopologyPort.DISCONNECTED) // Connection on the left port
                    {
                        TopologyLoc srcLoc = TopologyLoc.centerOf(srcNode.portA.startLoc, srcNode.portA.endLoc);
                        TopologyNode trgNode = nodes[srcNode.portA.connPortId];
                        TopologyLoc trgLoc;

                        if (srcNode.portA.connPortType == TopologyPort.TpPortType.LEFT)
                        {
                            trgLoc = TopologyLoc.centerOf(trgNode.portA.startLoc, trgNode.portA.endLoc);
                            trgNode.portA.connLinkDrawn = true;
                        }
                        else if (srcNode.portA.connPortType == TopologyPort.TpPortType.RIGHT)
                        {
                            trgLoc = TopologyLoc.centerOf(trgNode.portB.startLoc, trgNode.portB.endLoc);
                            trgNode.portB.connLinkDrawn = true;
                        }
                        else
                        {
                            trgLoc = TopologyLoc.centerOf(trgNode.portC.startLoc, trgNode.portC.endLoc);
                            trgNode.portC.connLinkDrawn = true;
                        }

                        canvas.DrawLine(Pens.Black, (Point)srcLoc, (Point)trgLoc);
                    }

                    if (!srcNode.portB.connLinkDrawn && srcNode.portB.connPortId != TopologyPort.DISCONNECTED) // Connection on the right port
                    {
                        TopologyLoc srcLoc = TopologyLoc.centerOf(srcNode.portB.startLoc, srcNode.portB.endLoc);
                        TopologyNode trgNode = nodes[srcNode.portB.connPortId];
                        TopologyLoc trgLoc;

                        if (srcNode.portB.connPortType == TopologyPort.TpPortType.LEFT)
                        {
                            trgLoc = TopologyLoc.centerOf(trgNode.portA.startLoc, trgNode.portA.endLoc);
                            trgNode.portA.connLinkDrawn = true;
                        }
                        else if (srcNode.portB.connPortType == TopologyPort.TpPortType.RIGHT)
                        {
                            trgLoc = TopologyLoc.centerOf(trgNode.portB.startLoc, trgNode.portB.endLoc);
                            trgNode.portB.connLinkDrawn = true;
                        }
                        else
                        {
                            trgLoc = TopologyLoc.centerOf(trgNode.portC.startLoc, trgNode.portC.endLoc);
                            trgNode.portC.connLinkDrawn = true;
                        }

                        canvas.DrawLine(Pens.Black, (Point)srcLoc, (Point)trgLoc);
                    }

                    if (!srcNode.portC.connLinkDrawn && srcNode.portC.connPortId != TopologyPort.DISCONNECTED) // Connection on the top port
                    {
                        TopologyLoc srcLoc = TopologyLoc.centerOf(srcNode.portC.startLoc, srcNode.portC.endLoc);
                        TopologyNode trgNode = nodes[srcNode.portC.connPortId];
                        TopologyLoc trgLoc;

                        if (srcNode.portC.connPortType == TopologyPort.TpPortType.LEFT)
                        {
                            trgLoc = TopologyLoc.centerOf(trgNode.portA.startLoc, trgNode.portA.endLoc);
                            trgNode.portA.connLinkDrawn = true;
                        }
                        else if (srcNode.portC.connPortType == TopologyPort.TpPortType.RIGHT)
                        {
                            trgLoc = TopologyLoc.centerOf(trgNode.portB.startLoc, trgNode.portB.endLoc);
                            trgNode.portB.connLinkDrawn = true;
                        }
                        else
                        {
                            trgLoc = TopologyLoc.centerOf(trgNode.portC.startLoc, trgNode.portC.endLoc);
                            trgNode.portC.connLinkDrawn = true;
                        }

                        canvas.DrawLine(Pens.Black, (Point)srcLoc, (Point)trgLoc);
                    }
                }

                canvas.Flush();
            });
        }

        private void clearCanvas()
        {
            this.canvas = canvasPictureBox.CreateGraphics();
            this.canvas.Clear(Color.White);
            foreach (TopologyNode node in nodes.Values)
                node.clearConnDrawCache();
        }

        #region Node-Click event handlers
        private void nodeMouseDownHandler(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                if (activePort != null)
                {
                    activePort.brush = Brushes.Green;
                    clearPortConnections(activePort);
                }

                this.activeNode = null;
                this.activePort = null;
                this.translateNode = false;

                // In case user tried to see node details
                if (e.Button == MouseButtons.Right || showDetailsDelegate != null)
                {
                    foreach (TopologyNode _node in nodes.Values)
                        if (_node.isInLocation(e.X, e.Y))
                        {
                            showDetailsDelegate?.Invoke(_node);
                            return;
                        }
                    return;
                }

                //log(value: "actions canceled (action: mouse right click)");
                return;
            }

            TopologyNode node = null;
            foreach (TopologyNode _node in nodes.Values)
                if (_node.isInLocation(e.X, e.Y))
                {
                    node = _node;
                    break;
                }

            if (node != null)
            {
                switch (clickAction)
                {
                    case TpClickAction.MOVE_LOCATION:
                        this.activeNode = node;
                        this.translateNode = true;
                        this.origNodePosX = node.startLoc.x;
                        this.origNodePosY = node.startLoc.y;
                        this.mouseLocDiffX = (short)(e.X - node.startLoc.x);
                        this.mouseLocDiffY = (short)(e.Y - node.startLoc.y);
                        break;
                    case TpClickAction.SET_CONNECTIONS:
                        {
                            TopologyPort port = getPortAt(node, e.X, e.Y);
                            if (port == null)
                                break;
                            else if (activePort != null)
                                if (port == activePort)
                                    break;
                                else if (port.parent == activePort.parent)
                                {
                                    port.brush = Brushes.DarkOrange;
                                    activePort.brush = Brushes.Green;

                                    activePort = port;
                                    redrawTopology();
                                    break;
                                }

                            if (activePort == null)
                            {
                                activePort = port;
                                activePort.brush = Brushes.DarkOrange;
                            }
                            else
                            {
                                port.brush = Brushes.Green;
                                activePort.brush = Brushes.Green;

                                clearPortConnections(port, activePort);
                                attachPorts(port, activePort);
                                redrawTopology();

                                activePort = null;
                            }
                        }
                        break;
                    case TpClickAction.SET_NODE_TYPE:
                        this.activeNode = node;
                        break;
                    case TpClickAction.SET_NODE_IP:
                        this.activeNode = node;
                        break;
                    case TpClickAction.SET_NODE_MAC:
                        this.activeNode = node;
                        break;
                    default:
                        break;
                }
            }
        }

        private void nodeMouseUpHandler(object sender, MouseEventArgs e)
        {
            hideDetailsDelegate?.Invoke();

            switch (clickAction)
            {
                case TpClickAction.MOVE_LOCATION:
                    if (e.Button == MouseButtons.Left)
                        this.translateNode = false;
                    this.activeNode = null;
                    break;
                case TpClickAction.SET_CONNECTIONS:
                    nodeClickHandler(activeNode);
                    break;
                case TpClickAction.SET_NODE_TYPE:
                    nodeClickHandler(activeNode);
                    this.activeNode = null;
                    break;
                case TpClickAction.SET_NODE_IP:
                    nodeClickHandler(activeNode);
                    this.activeNode = null;
                    break;
                case TpClickAction.SET_NODE_MAC:
                    nodeClickHandler(activeNode);
                    this.activeNode = null;
                    break;
                default:
                    break;
            }

            redrawTopology();
        }

        private void nodeMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (clickAction == TpClickAction.MOVE_LOCATION)
                if (translateNode)
                {
                    int toX = e.X - mouseLocDiffX, toY = e.Y - mouseLocDiffY;
                    bool inside = toX >= 0 & toY >= 0;
                    inside = inside & (toX <= this.pBoxSize.Width - this.activeNode.endLoc.x + this.activeNode.startLoc.x & toY <= this.pBoxSize.Height - this.activeNode.endLoc.y + this.activeNode.startLoc.y);

                    if (inside)
                        this.activeNode.translateToLoc((short)(e.X - mouseLocDiffX), (short)(e.Y - mouseLocDiffY), this);
                }
        }

        private void nodeClickHandler(TopologyNode node)
        {
            if (node == null)
                return;

            string input;
            switch (clickAction)
            {
                case TpClickAction.MOVE_LOCATION:
                    break;
                case TpClickAction.SET_CONNECTIONS:
                    break;
                case TpClickAction.SET_NODE_TYPE:
                    input = Interaction.InputBox("Please enter:\n-1 to unset node type\n0 for DANP\n1 for DANH\n2 for REDBOXP\n3 for REDBOXH\n4 for VDANH\n5 for VDANP", "Node type", "-1");
                    int type;
                    if (int.TryParse(input, out type))
                        switch ((TopologyNode.TpNodeType)type)
                        {
                            case TopologyNode.TpNodeType.DANP:
                                node.type = TopologyNode.TpNodeType.DANP;
                                log(node, "type", "DANP");
                                break;
                            case TopologyNode.TpNodeType.DANH:
                                node.type = TopologyNode.TpNodeType.DANH;
                                log(node, "type", "DANH");
                                break;
                            case TopologyNode.TpNodeType.REDBOXP:
                                node.type = TopologyNode.TpNodeType.REDBOXP;
                                log(node, "type", "REDBOXP");
                                break;
                            case TopologyNode.TpNodeType.REDBOXH:
                                node.type = TopologyNode.TpNodeType.REDBOXH;
                                log(node, "type", "REDBOXH");
                                break;
                            case TopologyNode.TpNodeType.VDANH:
                                node.type = TopologyNode.TpNodeType.VDANH;
                                log(node, "type", "VDANH");
                                break;
                            case TopologyNode.TpNodeType.VDANP:
                                node.type = TopologyNode.TpNodeType.VDANP;
                                log(node, "type", "VDANP");
                                break;
                            default:
                                node.type = TopologyNode.TpNodeType.DEFAULT;
                                log(node, "type", "UNSET");
                                break;
                        }
                    else
                        MessageBox.Show("Input is invalid, please read instructions more carefully!", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case TpClickAction.SET_NODE_IP:
                    input = Interaction.InputBox("Please enter IP Address of the node\n(e.g. 192.168.43.4)", "Node IP Address", "");
                    if (Regex.IsMatch(input, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$"))
                    {
                        node.ipAddress = IPAddress.Parse(input);
                        log(node, "IP address", node.ipAddress.ToString());
                    }
                    else
                        MessageBox.Show("IP Address is invalid, please trin!", "Invalid IP Address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case TpClickAction.SET_NODE_MAC:
                    input = Interaction.InputBox("Please enter Physical (MAC) Address of the node\n(e.g. 00-0a-95-9d-68-16)", "Node Physical (MAC) Address", "");
                    if (Regex.IsMatch(input, "^([0-9A-Fa-f]{2}-){5}([0-9A-Fa-f]{2})$"))
                    {
                        node.phyAddress = PhysicalAddress.Parse(input.ToUpper());
                        log(node, "type", node.phyAddress.ToString());
                    }
                    else
                        MessageBox.Show("Physical Address is invalid, please try again!", "Invalid Physical Address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                default:
                    MessageBox.Show(string.Format("Node {0} has been clicked!", node.id));
                    break;
            }
        }
        #endregion

        #region Topology elements
        internal class TopologyNode
        {
            internal enum TpNodeType
            {
                DEFAULT = -1,
                DANP = 0,
                DANH = 1,
                REDBOXP = 2,
                REDBOXH = 3,
                VDANH = 4,
                VDANP = 5
            }

            internal TopologyNode(short id, short posX, short posY, short w, short h, short topMargin, short rPortConnId = TopologyPort.DISCONNECTED, short lPortConnId = TopologyPort.DISCONNECTED, short tPortConnId = TopologyPort.DISCONNECTED)
            {
                this.id = id;
                this.startLoc = new TopologyLoc(posX, posY);
                this.endLoc = new TopologyLoc((short)(posX + w), (short)(posY + h));

                // Calculate and set port locations
                short pBoxW = NODE_WIDTH / 3;
                short pBoxH = NODE_HEIGHT / 3;

                TopologyLoc tPortStartLoc = new TopologyLoc((short)(this.startLoc.x + pBoxW), this.startLoc.y);
                TopologyLoc tPortEndLoc = new TopologyLoc((short)(tPortStartLoc.x + pBoxW), (short)(tPortStartLoc.y + pBoxH));
                TopologyLoc rPortStartLoc = new TopologyLoc((short)(this.startLoc.x + 2 * pBoxW), (short)(this.startLoc.y + 2 * pBoxH));
                TopologyLoc rPortEndLoc = new TopologyLoc((short)(rPortStartLoc.x + pBoxW), (short)(rPortStartLoc.y + pBoxH));
                TopologyLoc lPortStartLoc = new TopologyLoc(this.startLoc.x, (short)(this.startLoc.y + 2 * pBoxH));
                TopologyLoc lPortEndLoc = new TopologyLoc((short)(lPortStartLoc.x + pBoxW), (short)(lPortStartLoc.y + pBoxH));

                // Assign values to global variables
                this.portA = new TopologyPort(TopologyPort.TpPortType.LEFT, lPortStartLoc, lPortEndLoc, this);
                this.portB = new TopologyPort(TopologyPort.TpPortType.RIGHT, rPortStartLoc, rPortEndLoc, this);
                this.portC = new TopologyPort(TopologyPort.TpPortType.TOP, tPortStartLoc, tPortEndLoc, this);
                this.portA.connLinkDrawn = false;
                this.portA.connPortId = lPortConnId;
                this.portB.connLinkDrawn = false;
                this.portB.connPortId = rPortConnId;
                this.portC.connLinkDrawn = false;
                this.portC.connPortId = tPortConnId;
                this.type = TpNodeType.DEFAULT;
                this.topMargin = topMargin;

                this.ipAddress = default(IPAddress);
                this.phyAddress = default(PhysicalAddress);

                this.brush = Brushes.LightYellow;
                this.mibBrush = Brushes.Black;
            }

            internal TopologyNode(short id, TopologyLoc startLoc, TopologyLoc endLoc, TopologyPort tPort, TopologyPort rPort, TopologyPort lPort, TpNodeType type, short topMargin)
            {
                setBody(id, startLoc, endLoc, tPort, rPort, lPort, type, topMargin);
            }

            internal TopologyNode()
            {

            }

            internal void setBody(short id, TopologyLoc startLoc, TopologyLoc endLoc, TopologyPort tPort, TopologyPort rPort, TopologyPort lPort, TpNodeType type, short topMargin)
            {
                this.id = id;
                this.startLoc = startLoc;
                this.endLoc = endLoc;

                this.portC = tPort;
                this.portB = rPort;
                this.portA = lPort;
                this.portB.connLinkDrawn = false;
                this.portA.connLinkDrawn = false;
                this.portC.connLinkDrawn = false;
                this.type = type;
                this.topMargin = topMargin;

                this.ipAddress = default(IPAddress);
                this.phyAddress = default(PhysicalAddress);

                this.brush = Brushes.LightYellow;
                this.mibBrush = Brushes.Black;
            }

            #region Variables
            internal short id; // incremental number
            internal TopologyLoc startLoc; // start-location
            internal TopologyLoc endLoc; // end-location
            internal short topMargin; // node<->title margin

            internal TopologyPort portC;
            internal TopologyPort portB;
            internal TopologyPort portA;

            internal Brush brush;
            internal Brush mibBrush;
            internal TpNodeType type = TpNodeType.DEFAULT;
            internal IPAddress ipAddress;
            internal PhysicalAddress phyAddress;
            #endregion

            internal bool getPortByDbName(string dbName, out TopologyPort thePort)
            {
                switch (dbName[dbName.Length - 1])
                {
                    case 'a':
                        thePort = portA;
                        return true;
                    case 'b':
                        thePort = portB;
                        return true;
                    case 'c':
                        thePort = portC;
                        return true;
                    default:
                        thePort = null;
                        return false;
                }
            }

            internal void clearConnDrawCache()
            {
                this.portC.connLinkDrawn = false;
                this.portB.connLinkDrawn = false;
                this.portA.connLinkDrawn = false;
            }

            internal void setRightPortConn(short rPortConnId)
            {
                this.portB.connPortId = rPortConnId;
            }

            internal void setLeftPortConn(short lPortConnId)
            {
                this.portA.connPortId = lPortConnId;
            }

            internal void setTopPortConn(short tPortConnId)
            {
                this.portC.connPortId = tPortConnId;
            }

            internal void translateToLoc(short newX, short newY, TopologyVisualizer redrawVis = null)
            {
                short deltaX = (short)(newX - this.startLoc.x);
                short deltaY = (short)(newY - this.startLoc.y);

                if (redrawVis != null)
                {
                    //redrawVis.canvas.DrawRectangle(Pens.White, this.startLoc.x, startLoc.y, this.endLoc.x - this.startLoc.x, this.endLoc.y - this.startLoc.y);
                    redrawVis.eraseNodeRect(this);
                }

                this.startLoc.x = newX;
                this.startLoc.y = newY;
                this.endLoc.x += deltaX;
                this.endLoc.y += deltaY;

                this.portC.startLoc.x += deltaX;
                this.portC.startLoc.y += deltaY;
                this.portC.endLoc.x += deltaX;
                this.portC.endLoc.y += deltaY;

                this.portB.startLoc.x += deltaX;
                this.portB.startLoc.y += deltaY;
                this.portB.endLoc.x += deltaX;
                this.portB.endLoc.y += deltaY;

                this.portA.startLoc.x += deltaX;
                this.portA.startLoc.y += deltaY;
                this.portA.endLoc.x += deltaX;
                this.portA.endLoc.y += deltaY;

                if (redrawVis != null)
                {
                    //redrawVis.canvas.DrawRectangle(Pens.Red, this.startLoc.x, startLoc.y, this.endLoc.x - this.startLoc.x, this.endLoc.y - this.startLoc.y);
                    redrawVis.drawNodeRect(this);
                }
            }

            internal bool isInLocation(int x, int y)
            {
                return (startLoc.x <= x & x <= endLoc.x) && (startLoc.y <= y & y <= endLoc.y);
            }

            public XElement toXmlElement(string descr = "")
            {
                XElement res = new XElement("TpNode", new XAttribute("descr", descr));

                XElement id = new XElement("id");
                id.Value = this.id.ToString();
                res.Add(id);

                res.Add(this.startLoc.toXmlElement("Starting Location"));
                res.Add(this.endLoc.toXmlElement("Ending Location"));

                XElement topMargin = new XElement("topMargin");
                topMargin.Value = this.topMargin.ToString();
                res.Add(topMargin);

                res.Add(this.portC.toXmlElement("Top Port"));
                res.Add(this.portB.toXmlElement("Right Port"));
                res.Add(this.portA.toXmlElement("Left Port"));

                XElement type = new XElement("type");
                type.Value = this.type.ToString("d");
                res.Add(type);

                XElement ipAddress = new XElement("ipAddress");
                if (this.ipAddress != default(IPAddress))
                    ipAddress.Value = this.ipAddress.ToString();
                res.Add(ipAddress);

                XElement phyAddress = new XElement("phyAddress");
                if (this.phyAddress != default(PhysicalAddress))
                    phyAddress.Value = this.phyAddress.ToString();
                res.Add(phyAddress);

                return res;
            }

            public static TopologyNode parseXml(XElement tpLocXml)
            {
                TopologyNode res = new TopologyNode();

                XNode node = tpLocXml.FirstNode;
                short id = short.Parse(((XElement)node).Value);
                node = node.NextNode;
                TopologyLoc startLoc = TopologyLoc.parseXml((XElement)node);
                node = node.NextNode;
                TopologyLoc endLoc = TopologyLoc.parseXml((XElement)node);
                node = node.NextNode;
                short topMargin = short.Parse(((XElement)node).Value);
                node = node.NextNode;
                TopologyPort tPort = TopologyPort.parseXml(res, (XElement)node);
                node = node.NextNode;
                TopologyPort rPort = TopologyPort.parseXml(res, (XElement)node);
                node = node.NextNode;
                TopologyPort lPort = TopologyPort.parseXml(res, (XElement)node);
                node = node.NextNode;
                TpNodeType type = (TpNodeType)int.Parse(((XElement)node).Value);

                res.setBody(id, startLoc, endLoc, tPort, rPort, lPort, type, topMargin);

                node = node.NextNode;
                string ipAddrString = ((XElement)node).Value;
                if (Regex.IsMatch(ipAddrString, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$"))
                    res.ipAddress = IPAddress.Parse(ipAddrString);
                node = node.NextNode;
                string phyAddrString = ((XElement)node).Value;
                //if (Regex.IsMatch(phyAddrString, "^([0-9A-Fa-f]{2}-){5}([0-9A-Fa-f]{2})$"))
                if (Regex.IsMatch(phyAddrString, "^([0-9A-Fa-f]{2}){6}$"))
                    res.phyAddress = PhysicalAddress.Parse(phyAddrString);

                return res;
            }
        }

        internal class TopologyPort
        {
            internal enum TpPortType
            {
                NULL = -1,
                LEFT,
                RIGHT,
                TOP
            }

            #region Variables
            internal const short DISCONNECTED = -1;
            internal int tag = -1;
            internal TpPortType type;
            internal TopologyLoc startLoc;
            internal TopologyLoc endLoc;
            internal TopologyNode parent;
            internal Brush brush;
            internal short connPortId;
            internal TpPortType connPortType;
            internal bool connLinkDrawn;
            #endregion

            internal TopologyPort(TpPortType type, TopologyLoc startLoc, TopologyLoc endLoc, TopologyNode parent, Brush brush = null)
            {
                this.type = type;
                this.startLoc = startLoc;
                this.endLoc = endLoc;
                this.parent = parent;
                this.connPortId = DISCONNECTED;
                this.connPortType = TpPortType.NULL;
                this.brush = brush ?? Brushes.Green;
            }

            internal bool isInLocation(int x, int y)
            {
                return (startLoc.x <= x & x <= endLoc.x) && (startLoc.y <= y & y <= endLoc.y);
            }

            public XElement toXmlElement(string descr = "")
            {
                XElement res = new XElement("TpPort", new XAttribute("descr", descr));

                XElement type = new XElement("type");
                type.Value = this.type.ToString("d");
                res.Add(type);

                res.Add(this.startLoc.toXmlElement("Starting Location"));
                res.Add(this.endLoc.toXmlElement("Ending Location"));

                XElement connId = new XElement("connId");
                connId.Value = this.connPortId.ToString();
                res.Add(connId);

                XElement connPortType = new XElement("connPortType");
                connPortType.Value = ((int)this.connPortType).ToString();
                res.Add(connPortType);

                return res;
            }

            public static TopologyPort parseXml(TopologyNode parent, XElement tpLocXml)
            {
                XNode node = tpLocXml.FirstNode;
                TpPortType type = (TpPortType)int.Parse(((XElement)node).Value);
                node = node.NextNode;
                TopologyLoc startLoc = TopologyLoc.parseXml((XElement)node);
                node = node.NextNode;
                TopologyLoc endLoc = TopologyLoc.parseXml((XElement)node);
                node = node.NextNode;
                short connId = short.Parse(((XElement)node).Value);
                node = node.NextNode;
                TpPortType connPortType = (TpPortType)int.Parse(((XElement)node).Value);

                TopologyPort res = new TopologyPort(type, startLoc, endLoc, parent);
                res.connPortId = connId;
                res.connPortType = connPortType;

                return res;
            }
        }

        internal struct TopologyLoc
        {
            internal TopologyLoc(short x, short y)
            {
                this.x = x;
                this.y = y;
            }

            internal short x;
            internal short y;

            internal static TopologyLoc centerOf(TopologyLoc startLoc, TopologyLoc endLoc)
            {
                return new TopologyLoc((short)(startLoc.x + (endLoc.x - startLoc.x) / 2), (short)(startLoc.y + (endLoc.y - startLoc.y) / 2));
            }

            public static explicit operator Point(TopologyLoc loc)
            {
                return new Point(loc.x, loc.y);
            }

            public XElement toXmlElement(string descr = "")
            {
                XElement res = new XElement("TpLoc", new XAttribute("descr", descr));

                XElement x = new XElement("x");
                x.Value = this.x.ToString();
                res.Add(x);

                XElement y = new XElement("y");
                y.Value = this.y.ToString();
                res.Add(y);

                return res;
            }

            public static TopologyLoc parseXml(XElement tpLocXml)
            {
                XNode node = tpLocXml.FirstNode;

                short x = short.Parse(((XElement)node).Value);
                node = node.NextNode;
                short y = short.Parse(((XElement)node).Value);

                return new TopologyLoc(x, y);
            }
        }
        #endregion

        internal void exportNodeXmls(string xmlFilePath, string timestamp = "")
        {
            XElement root = new XElement("TpNodes", new XAttribute("timestamp", timestamp));
            foreach (TopologyNode node in nodes.Values)
                root.Add(node.toXmlElement(string.Format("Node {0}", node.id)));
            using (StreamWriter file = new StreamWriter(xmlFilePath, false))
                file.WriteLine(root.ToString());
        }

        internal void importNodeXmls(string xmlFilePath)
        {
            nodes.Clear();

            XElement root = XElement.Load(xmlFilePath);

            foreach (XNode xNode in root.Nodes())
            {
                TopologyNode tpNode = TopologyNode.parseXml((XElement)xNode);
                nodes.Add(tpNode.id, tpNode);
            }
        }

        private void log(TopologyNode node = null, string key = null, string value = null)
        {
            if (this.logTextBox != null)
            {
                if (node == null && key == null && value != null)
                    this.logTextBox.AppendText(string.Format("[LOG MSG]:\t{0}\n", value));
                else
                    this.logTextBox.AppendText(string.Format("[Node {0}]:\t{1} was set to {2}\n", node.id, key, value));
                logLinesCount++;

                if (logLinesCount == MAX_LOG_LINES_COUNT)
                    this.logTextBox.Text = logTextBox.Text.Remove(0, this.logTextBox.Lines[0].Length + Environment.NewLine.Length);
            }
        }

        internal delegate void HideDetailsDelegate();

        internal delegate void ShowDetailsDelegate(TopologyNode node);
    }
}
