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
        internal const short SWITCH_WIDTH = NODE_WIDTH / 3;
        internal const short SWITCH_HEIGHT = NODE_HEIGHT / 3;
        internal const short MAX_LOG_LINES_COUNT = 250;
        #endregion

        #region Variables
        internal PictureBox canvasPictureBox;
        internal Graphics canvas;
        private readonly Font font;
        private TextBox logTextBox;

        internal TpClickAction clickAction;

        private Dictionary<short, TopologyNode> nodes;
        private Dictionary<short, TopologySwitchNode> switchNodes;
        internal TopologyNode[] Nodes
        {
            get
            {
                TopologyNode[] res = new TopologyNode[nodes.Count];
                nodes.Values.CopyTo(res, 0);
                return res;
            }
        }
        internal TopologySwitchNode[] SwitchNodes
        {
            get
            {
                TopologySwitchNode[] res = new TopologySwitchNode[switchNodes.Count];
                switchNodes.Values.CopyTo(res, 0);
                return res;
            }
        }
        internal int NumOfNodes { get { return nodes.Count; } }
        internal bool IsEmpty { get { return nodes == null || nodes.Count == 0; } }
        private short nextNodeId;
        private short nextSwitchNodeId;
        private bool translateNode;
        private short mouseLocDiffX, mouseLocDiffY;
        private short origNodePosX, origNodePosY;
        private TopologyNode activeNode;
        private TopologyPort activePort;
        private TopologySwitchNode activeSwitchNode;
        private Size canvasSize;
        private short logLinesCount;

        private readonly NodeNonLeftMouseDownDelegate nodeNonLeftMouseDownDelegate;
        private readonly NodeNonLeftMouseUpDelegate nodeNonLeftMouseUpDelegate;
        #endregion

        internal TopologyVisualizer(PictureBox topologyPictureBox, TextBox logTextBox = null, NodeNonLeftMouseDownDelegate nodeNonLeftMouseDownDelegate = null, NodeNonLeftMouseUpDelegate nodeNonLeftMouseUpDelegate = null)
        {
            this.translateNode = false;
            this.clickAction = TpClickAction.MOVE_LOCATION;
            this.activePort = null;
            this.activeNode = null;

            this.canvasPictureBox = topologyPictureBox;
            this.canvas = topologyPictureBox.CreateGraphics();
            this.font = topologyPictureBox.Font;
            this.nodes = new Dictionary<short, TopologyNode>();
            this.switchNodes = new Dictionary<short, TopologySwitchNode>();
            this.nextNodeId = 0;
            this.nextSwitchNodeId = short.MaxValue;
            this.canvasSize = topologyPictureBox.Size;

            topologyPictureBox.MouseDown += NodeMouseDownHandler;
            topologyPictureBox.MouseUp += NodeMouseUpHandler;
            topologyPictureBox.MouseMove += NodeMouseMoveHandler;
            topologyPictureBox.PreviewKeyDown += NodePreviewKeyDownHandler;

            topologyPictureBox.InitialImage = null;
            this.logTextBox = logTextBox;
            this.nodeNonLeftMouseDownDelegate = nodeNonLeftMouseDownDelegate;
            this.nodeNonLeftMouseUpDelegate = nodeNonLeftMouseUpDelegate;

            clickAction = TpClickAction.NONE;

            ClearCanvas();
        }

        internal TopologyNode AddNode(short posX, short posY, short rPortConn = TopologyPort.DISCONNECTED, short lPortConn = TopologyPort.DISCONNECTED, short tPortConn = TopologyPort.DISCONNECTED)
        {
            TopologyNode newNode = new TopologyNode(nextNodeId, posX, posY, NODE_WIDTH, NODE_HEIGHT, NTITLE_MARGIN);
            newNode.SetRightPortConn(rPortConn);
            newNode.SetLeftPortConn(lPortConn);
            newNode.SetTopPortConn(tPortConn);
            nodes.Add(newNode.id, newNode);
            nextNodeId++;
            return newNode;
        }

        internal TopologySwitchNode AddSwitchNode(short posX, short posY)
        {
            TopologySwitchNode switchNode = new TopologySwitchNode(nextSwitchNodeId, posX, posY, SWITCH_WIDTH, SWITCH_HEIGHT);
            switchNodes.Add(switchNode.id, switchNode);
            nextSwitchNodeId--;
            return switchNode;
        }

        private void DrawNodeRect(TopologyNode node)
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

            // Top port - C
            canvas.DrawRectangle(Pens.Black, node.portC.startLoc.x, node.portC.startLoc.y, pBoxW, pBoxH);
            canvas.FillRectangle(node.portC.brush, node.portC.startLoc.x + 1, node.portC.startLoc.y + 1, pBoxW - 1, pBoxH - 1);
            TopologyLoc center = node.portC.GetCenter();
            string cPortNaming = "C";
            Font cPortFont = font;
            if (node.type == TopologyNode.TpNodeType.REDBOXP || node.type == TopologyNode.TpNodeType.REDBOXH)
                cPortNaming = "I";
            else if (node.type == TopologyNode.TpNodeType.DANP || node.type == TopologyNode.TpNodeType.DANH || node.type == TopologyNode.TpNodeType.VDANP || node.type == TopologyNode.TpNodeType.VDANH)
            {
                cPortFont = new Font(font.FontFamily, (int)(font.Size * 0.7));
                cPortNaming = "L";
            }
            canvas.DrawString(cPortNaming, cPortFont, Brushes.White, center.x - 8, center.y - 5);

            // Right port - B
            canvas.DrawRectangle(Pens.Black, node.portB.startLoc.x, node.portB.startLoc.y, pBoxW, pBoxH);
            canvas.FillRectangle(node.portB.brush, node.portB.startLoc.x + 1, node.portB.startLoc.y + 1, pBoxW - 1, pBoxH - 1);
            center = node.portB.GetCenter();
            canvas.DrawString("B", font, Brushes.White, center.x - 6, center.y - 5);

            // Left port - A
            canvas.DrawRectangle(Pens.Black, node.portA.startLoc.x, node.portA.startLoc.y, pBoxW, pBoxH);
            canvas.FillRectangle(node.portA.brush, node.portA.startLoc.x + 1, node.portA.startLoc.y + 1, pBoxW - 1, pBoxH - 1);
            center = node.portA.GetCenter();
            canvas.DrawString("A", font, Brushes.White, center.x - 6, center.y - 5);
        }

        private void DrawSwitchNodeRect(TopologySwitchNode switchNode)
        {
            // Body
            canvas.DrawRectangle(Pens.Black, switchNode.startLoc.x, switchNode.startLoc.y, SWITCH_WIDTH, SWITCH_HEIGHT);
            canvas.FillRectangle(switchNode.brush, switchNode.startLoc.x + 1, switchNode.startLoc.y + 1, SWITCH_WIDTH - 1, SWITCH_HEIGHT - 1);
        }

        private void EraseNodeRect(TopologyElement element)
        {
            if (element is TopologyNode)
            {
                // Title
                canvas.FillRectangle(
                    Brushes.White,
                    element.startLoc.x - 10,
                    element.startLoc.y - (element as TopologyNode).topMargin,
                    NODE_WIDTH + 60,
                    (element as TopologyNode).topMargin
                );
                // Body
                canvas.FillRectangle(
                    Brushes.White,
                    element.startLoc.x,
                    element.startLoc.y,
                    NODE_WIDTH + 1,
                    NODE_HEIGHT + 1
                );
            }
            else if (element is TopologySwitchNode)
            {
                // Body
                canvas.FillRectangle(
                    Brushes.White,
                    element.startLoc.x,
                    element.startLoc.y,
                    SWITCH_WIDTH + 1,
                    SWITCH_HEIGHT + 1
                );
            }
        }

        private TopologyPort GetPortAt(TopologyNode node, int x, int y)
        {
            if (node.IsInLocation(x, y))
                if (node.portA.IsInLocation(x, y))
                    return node.portA;
                else if (node.portB.IsInLocation(x, y))
                    return node.portB;
                else if (node.portC.IsInLocation(x, y))
                    return node.portC;
            return null;
        }

        private TopologySwitchNode GetSwitchNodeAt(int x, int y)
        {
            foreach (TopologySwitchNode switchNode in switchNodes.Values)
                if (switchNode.IsInLocation(x, y))
                    return switchNode;
            return null;
        }

        private TopologyNode GetNodeAt(int x, int y)
        {
            foreach (TopologyNode node in nodes.Values)
                if (node.IsInLocation(x, y))
                    return node;
            return null;
        }

        private void ClearPortConnections(params TopologyPort[] ports)
        {
            foreach (TopologyPort port in ports)
            {
                if (!(port is TopologySwitchNode) && !switchNodes.ContainsKey(port.connPortId) && port.connPortId != TopologyPort.DISCONNECTED)
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

        private void AttachPorts(TopologyPort port1, TopologyPort port2)
        {
            if (port1 is TopologySwitchNode || port2 is TopologySwitchNode)
            {
                TopologySwitchNode switchNode = (port1 is TopologySwitchNode ? port1 : port2) as TopologySwitchNode;
                TopologyPort port = port1 == switchNode ? port2 : port1;

                port.connPortId = switchNode.id;
                port.connPortType = TopologyPort.TpPortType.NULL;
                port.connLinkDrawn = false;
            }
            else
            {
                port1.connPortId = port2.parent.id;
                port1.connPortType = port2.type;
                port1.connLinkDrawn = false;

                port2.connPortId = port1.parent.id;
                port2.connPortType = port1.type;
                port2.connLinkDrawn = false;
            }
        }

        internal void RedrawTopology(bool bringUpFocusNode = false)
        {
            canvasPictureBox.Invoke((MethodInvoker)delegate
            {
                // Clear out the existing topology
                ClearCanvas();

                // Draw switches
                foreach (TopologySwitchNode switchNode in switchNodes.Values)
                    DrawSwitchNodeRect(switchNode);

                // Draw nodes
                foreach (TopologyNode node in nodes.Values)
                    if (bringUpFocusNode)
                        if (node != activeNode)
                            DrawNodeRect(node);
                        else
                            continue;
                    else
                        DrawNodeRect(node);

                if (bringUpFocusNode)
                    DrawNodeRect(activeNode);

                // Draw connections
                foreach (TopologyNode srcNode in nodes.Values)
                {
                    if (!srcNode.portA.connLinkDrawn && srcNode.portA.connPortId != TopologyPort.DISCONNECTED) // Connection on the left port
                    {
                        TopologyLoc srcLoc = TopologyLoc.CenterOf(srcNode.portA.startLoc, srcNode.portA.endLoc);
                        TopologyNode trgNode = nodes.ContainsKey(srcNode.portA.connPortId) ? nodes[srcNode.portA.connPortId] : null;
                        TopologySwitchNode trgSwitch = switchNodes.ContainsKey(srcNode.portA.connPortId) ? switchNodes[srcNode.portA.connPortId] : null;
                        TopologyLoc trgLoc = new TopologyLoc(-1, -1);

                        if (srcNode.portA.connPortType == TopologyPort.TpPortType.LEFT)
                        {
                            if (trgNode != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgNode.portA.startLoc, trgNode.portA.endLoc);
                                trgNode.portA.connLinkDrawn = true;
                            }
                            else if (trgSwitch != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgSwitch.startLoc, trgSwitch.endLoc);
                                trgSwitch.connLinkDrawn = true;
                            }
                        }
                        else if (srcNode.portA.connPortType == TopologyPort.TpPortType.RIGHT)
                        {
                            if (trgNode != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgNode.portB.startLoc, trgNode.portB.endLoc);
                                trgNode.portB.connLinkDrawn = true;
                            }
                            else if (trgSwitch != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgSwitch.startLoc, trgSwitch.endLoc);
                                trgSwitch.connLinkDrawn = true;
                            }
                        }
                        else
                        {
                            if (trgNode != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgNode.portC.startLoc, trgNode.portC.endLoc);
                                trgNode.portC.connLinkDrawn = true;
                            }
                            else if (trgSwitch != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgSwitch.startLoc, trgSwitch.endLoc);
                                trgSwitch.connLinkDrawn = true;
                            }
                        }

                        if (!trgLoc.Is(-1, -1))
                            canvas.DrawLine(Pens.Black, (Point)srcLoc, (Point)trgLoc);
                    }

                    if (!srcNode.portB.connLinkDrawn && srcNode.portB.connPortId != TopologyPort.DISCONNECTED) // Connection on the right port
                    {
                        TopologyLoc srcLoc = TopologyLoc.CenterOf(srcNode.portB.startLoc, srcNode.portB.endLoc);
                        TopologyNode trgNode = nodes.ContainsKey(srcNode.portB.connPortId) ? nodes[srcNode.portB.connPortId] : null;
                        TopologySwitchNode trgSwitch = switchNodes.ContainsKey(srcNode.portB.connPortId) ? switchNodes[srcNode.portB.connPortId] : null;
                        TopologyLoc trgLoc = new TopologyLoc(-1, -1);

                        if (srcNode.portB.connPortType == TopologyPort.TpPortType.LEFT)
                        {
                            if (trgNode != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgNode.portA.startLoc, trgNode.portA.endLoc);
                                trgNode.portA.connLinkDrawn = true;
                            }
                            else if (trgSwitch != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgSwitch.startLoc, trgSwitch.endLoc);
                                trgSwitch.connLinkDrawn = true;
                            }
                        }
                        else if (srcNode.portB.connPortType == TopologyPort.TpPortType.RIGHT)
                        {
                            if (trgNode != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgNode.portB.startLoc, trgNode.portB.endLoc);
                                trgNode.portB.connLinkDrawn = true;
                            }
                            else if (trgSwitch != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgSwitch.startLoc, trgSwitch.endLoc);
                                trgSwitch.connLinkDrawn = true;
                            }
                        }
                        else
                        {
                            if (trgNode != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgNode.portC.startLoc, trgNode.portC.endLoc);
                                trgNode.portC.connLinkDrawn = true;
                            }
                            else if (trgSwitch != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgSwitch.startLoc, trgSwitch.endLoc);
                                trgSwitch.connLinkDrawn = true;
                            }
                        }

                        if (!trgLoc.Is(-1, -1))
                            canvas.DrawLine(Pens.Black, (Point)srcLoc, (Point)trgLoc);
                    }

                    if (!srcNode.portC.connLinkDrawn && srcNode.portC.connPortId != TopologyPort.DISCONNECTED) // Connection on the top port
                    {
                        TopologyLoc srcLoc = TopologyLoc.CenterOf(srcNode.portC.startLoc, srcNode.portC.endLoc);
                        TopologyNode trgNode = nodes.ContainsKey(srcNode.portC.connPortId) ? nodes[srcNode.portC.connPortId] : null;
                        TopologySwitchNode trgSwitch = switchNodes.ContainsKey(srcNode.portC.connPortId) ? switchNodes[srcNode.portC.connPortId] : null;
                        TopologyLoc trgLoc = new TopologyLoc(-1, -1);

                        if (srcNode.portC.connPortType == TopologyPort.TpPortType.LEFT)
                        {
                            if (trgNode != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgNode.portA.startLoc, trgNode.portA.endLoc);
                                trgNode.portA.connLinkDrawn = true;
                            }
                            else if (trgSwitch != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgSwitch.startLoc, trgSwitch.endLoc);
                                trgSwitch.connLinkDrawn = true;
                            }
                        }
                        else if (srcNode.portC.connPortType == TopologyPort.TpPortType.RIGHT)
                        {
                            if (trgNode != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgNode.portB.startLoc, trgNode.portB.endLoc);
                                trgNode.portB.connLinkDrawn = true;
                            }
                            else if (trgSwitch != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgSwitch.startLoc, trgSwitch.endLoc);
                                trgSwitch.connLinkDrawn = true;
                            }
                        }
                        else
                        {
                            if (trgNode != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgNode.portC.startLoc, trgNode.portC.endLoc);
                                trgNode.portC.connLinkDrawn = true;
                            }
                            else if (trgSwitch != null)
                            {
                                trgLoc = TopologyLoc.CenterOf(trgSwitch.startLoc, trgSwitch.endLoc);
                                trgSwitch.connLinkDrawn = true;
                            }
                        }

                        if (!trgLoc.Is(-1, -1))
                            canvas.DrawLine(Pens.Black, (Point)srcLoc, (Point)trgLoc);
                    }
                }

                canvas.Flush();
            });
        }

        private void ClearCanvas()
        {
            this.canvas = canvasPictureBox.CreateGraphics();
            this.canvas.Clear(Color.White);
            foreach (TopologyNode node in nodes.Values)
                node.ClearConnDrawCache();
        }

        #region Node-Click event handlers
        private void NodeMouseDownHandler(object sender, MouseEventArgs e)
        {
            TopologyNode _node = GetNodeAt(e.X, e.Y);
            TopologySwitchNode _switchNode = GetSwitchNodeAt(e.X, e.Y);
            if (e.Button == MouseButtons.Left)
            {
                if (_node != null)
                    switch (clickAction)
                    {
                        case TpClickAction.MOVE_LOCATION:
                            this.activeNode = _node;
                            this.translateNode = true;
                            this.origNodePosX = _node.startLoc.x;
                            this.origNodePosY = _node.startLoc.y;
                            this.mouseLocDiffX = (short)(e.X - _node.startLoc.x);
                            this.mouseLocDiffY = (short)(e.Y - _node.startLoc.y);
                            break;
                        case TpClickAction.SET_CONNECTIONS:
                            {
                                TopologyPort port = GetPortAt(_node, e.X, e.Y);
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
                                        RedrawTopology();
                                        break;
                                    }

                                if (activePort == null)
                                {
                                    if (this.activeSwitchNode != null)
                                    {
                                        port.brush = Brushes.Green;
                                        this.activeSwitchNode.brush = Brushes.SlateBlue;

                                        ClearPortConnections(port);
                                        AttachPorts(port, activeSwitchNode);
                                        RedrawTopology();

                                        this.activeSwitchNode = null;
                                    }
                                    else
                                    {
                                        activePort = port;
                                        activePort.brush = Brushes.DarkOrange;
                                    }
                                }
                                else
                                {
                                    port.brush = Brushes.Green;
                                    activePort.brush = Brushes.Green;

                                    ClearPortConnections(port, activePort);
                                    AttachPorts(port, activePort);
                                    RedrawTopology();

                                    activePort = null;
                                }
                            }
                            break;
                        case TpClickAction.SET_NODE_TYPE:
                            this.activeNode = _node;
                            break;
                        case TpClickAction.SET_NODE_IP:
                            this.activeNode = _node;
                            break;
                        case TpClickAction.SET_NODE_MAC:
                            this.activeNode = _node;
                            break;
                        default:
                            break;
                    }
                else if (_switchNode != null)
                    switch (clickAction)
                    {
                        case TpClickAction.MOVE_LOCATION:
                            this.activeSwitchNode = _switchNode;
                            this.translateNode = true;
                            this.origNodePosX = _switchNode.startLoc.x;
                            this.origNodePosY = _switchNode.startLoc.y;
                            this.mouseLocDiffX = (short)(e.X - _switchNode.startLoc.x);
                            this.mouseLocDiffY = (short)(e.Y - _switchNode.startLoc.y);
                            break;
                        case TpClickAction.SET_CONNECTIONS:
                            if (activePort == null)
                                break;
                            else
                            {
                                _switchNode.brush = Brushes.SlateBlue;
                                this.activePort.brush = Brushes.Green;

                                ClearPortConnections(activePort);
                                AttachPorts(_switchNode, activePort);
                                RedrawTopology();

                                this.activeNode = null;
                                this.activePort = null;
                                this.activeSwitchNode = null;
                            }
                            break;
                        default:
                            break;
                    }
            }
            else
            {
                if (e.Button == MouseButtons.Right)
                {
                    if (activePort != null)
                    {
                        activePort.brush = Brushes.Green;
                        ClearPortConnections(activePort);
                    }
                    if (activeSwitchNode != null)
                        activeSwitchNode.brush = Brushes.SlateBlue;

                    this.activeNode = null;
                    this.activePort = null;
                    this.activeSwitchNode = null;
                    this.translateNode = false;
                    //log(value: "actions canceled (action: mouse right click)");
                }

                // In case user tried to see node details
                if (_node != null)
                    nodeNonLeftMouseDownDelegate?.Invoke(_node, e);
            }
        }

        private void NodeMouseUpHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                switch (clickAction)
                {
                    case TpClickAction.MOVE_LOCATION:
                        if (e.Button == MouseButtons.Left)
                            this.translateNode = false;
                        break;
                    case TpClickAction.SET_CONNECTIONS:
                        if (this.activeNode != null)
                            NodeLeftClickHandler(this.activeNode);
                        else if (this.activeSwitchNode != null)
                            NodeLeftClickHandler(this.activeSwitchNode);
                        break;
                    case TpClickAction.SET_NODE_TYPE:
                        if (this.activeNode != null)
                            NodeLeftClickHandler(this.activeNode);
                        else if (this.activeSwitchNode != null)
                            NodeLeftClickHandler(this.activeSwitchNode);
                        break;
                    case TpClickAction.SET_NODE_IP:
                        if (this.activeNode != null)
                            NodeLeftClickHandler(this.activeNode);
                        else if (this.activeSwitchNode != null)
                            NodeLeftClickHandler(this.activeSwitchNode);
                        break;
                    case TpClickAction.SET_NODE_MAC:
                        if (this.activeNode != null)
                            NodeLeftClickHandler(this.activeNode);
                        else if (this.activeSwitchNode != null)
                            NodeLeftClickHandler(this.activeSwitchNode);
                        break;
                    default:
                        break;
                }
                this.activeNode = null;
                this.activeSwitchNode = null;
            }
            else
                nodeNonLeftMouseUpDelegate?.Invoke(e);
            RedrawTopology();
        }

        private void NodeMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (translateNode && clickAction == TpClickAction.MOVE_LOCATION)
            {
                int toX = e.X - mouseLocDiffX, toY = e.Y - mouseLocDiffY;
                bool inside = toX >= 0 & toY >= 0;
                if (this.activeNode != null)
                {
                    inside = inside & (toX <= this.canvasSize.Width - this.activeNode.endLoc.x + this.activeNode.startLoc.x & toY <= this.canvasSize.Height - this.activeNode.endLoc.y + this.activeNode.startLoc.y);

                    if (inside)
                        this.activeNode.TranslateToLoc(
                            newX: (short)(e.X - mouseLocDiffX),
                            newY: (short)(e.Y - mouseLocDiffY),
                            redrawVis: this
                        );
                }
                else if (this.activeSwitchNode != null)
                {
                    inside = inside & toX <= this.canvasSize.Width - this.activeSwitchNode.endLoc.x + this.activeSwitchNode.startLoc.x & toY <= this.canvasSize.Height - this.activeSwitchNode.endLoc.y + this.activeSwitchNode.startLoc.y;
                    if (inside)
                        this.activeSwitchNode.TranslateToLoc(
                            newX: (short)(e.X - mouseLocDiffX),
                            newY: (short)(e.Y - mouseLocDiffY),
                            redrawVis: this
                        );
                }
            }
        }

        private void NodePreviewKeyDownHandler(object sender, PreviewKeyDownEventArgs e)
        {
            if (clickAction == TpClickAction.NONE)
                return;

            Point cursorLoc;
            if (e.KeyCode == Keys.R || e.KeyCode == Keys.V || e.KeyCode == Keys.H)
            {
                Control parent = (sender as Control).Parent;
                while (!(parent is Form))
                    parent = parent.Parent;
                Point pointInForm = parent.PointToClient(Cursor.Position);

                cursorLoc = new Point(
                    x: pointInForm.X + canvasPictureBox.Location.X,
                    y: pointInForm.Y + canvasPictureBox.Location.Y
                );
            }
            else return;

            TopologyNode _node = GetNodeAt(cursorLoc.X, cursorLoc.Y);
            if (_node != null)
            {
                switch (e.KeyCode)
                {
                    case Keys.R: // Rotate action
                        _node.Rotate();
                        break;
                    case Keys.V:
                        _node.FlipVertically();
                        break;
                    case Keys.H:
                        _node.FlipHorizontally();
                        break;
                    default:
                        break;
                }
                RedrawTopology();
            }
        }

        private void NodeLeftClickHandler(TopologyElement element)
        {
            if (element == null)
                return;

            string input;
            switch (clickAction)
            {
                case TpClickAction.MOVE_LOCATION:
                    break;
                case TpClickAction.SET_CONNECTIONS:
                    break;
                case TpClickAction.SET_NODE_TYPE:
                    if (element is TopologyNode)
                    {
                        input = Interaction.InputBox("Please enter:\n-1 to unset node type\n0 for DANP\n1 for DANH\n2 for REDBOXP\n3 for REDBOXH\n4 for VDANH\n5 for VDANP", "Node type", "-1");
                        int type;
                        if (int.TryParse(input, out type))
                            switch ((TopologyNode.TpNodeType)type)
                            {
                                case TopologyNode.TpNodeType.DANP:
                                    (element as TopologyNode).type = TopologyNode.TpNodeType.DANP;
                                    Log(element as TopologyNode, "type", "DANP");
                                    break;
                                case TopologyNode.TpNodeType.DANH:
                                    (element as TopologyNode).type = TopologyNode.TpNodeType.DANH;
                                    Log(element as TopologyNode, "type", "DANH");
                                    break;
                                case TopologyNode.TpNodeType.REDBOXP:
                                    (element as TopologyNode).type = TopologyNode.TpNodeType.REDBOXP;
                                    Log(element as TopologyNode, "type", "REDBOXP");
                                    break;
                                case TopologyNode.TpNodeType.REDBOXH:
                                    (element as TopologyNode).type = TopologyNode.TpNodeType.REDBOXH;
                                    Log(element as TopologyNode, "type", "REDBOXH");
                                    break;
                                case TopologyNode.TpNodeType.VDANH:
                                    (element as TopologyNode).type = TopologyNode.TpNodeType.VDANH;
                                    Log(element as TopologyNode, "type", "VDANH");
                                    break;
                                case TopologyNode.TpNodeType.VDANP:
                                    (element as TopologyNode).type = TopologyNode.TpNodeType.VDANP;
                                    Log(element as TopologyNode, "type", "VDANP");
                                    break;
                                default:
                                    (element as TopologyNode).type = TopologyNode.TpNodeType.DEFAULT;
                                    Log(element as TopologyNode, "type", "UNSET");
                                    break;
                            }
                        else
                            MessageBox.Show("Input is invalid, please read instructions more carefully!", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case TpClickAction.SET_NODE_IP:
                    if (element is TopologyNode)
                    {
                        input = Interaction.InputBox("Please enter IP Address of the node\n(e.g. 192.168.43.4)", "Node IP Address", "");
                        if (Regex.IsMatch(input, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$"))
                        {
                            (element as TopologyNode).ipAddress = IPAddress.Parse(input);
                            Log(element as TopologyNode, "IP address", (element as TopologyNode).ipAddress.ToString());
                        }
                        else
                            MessageBox.Show("IP Address is invalid, please trin!", "Invalid IP Address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case TpClickAction.SET_NODE_MAC:
                    if (element is TopologyNode)
                    {
                        input = Interaction.InputBox("Please enter Physical (MAC) Address of the node\n(e.g. 00-0a-95-9d-68-16)", "Node Physical (MAC) Address", "");
                        if (Regex.IsMatch(input, "^([0-9A-Fa-f]{2}-){5}([0-9A-Fa-f]{2})$"))
                        {
                            (element as TopologyNode).phyAddress = PhysicalAddress.Parse(input.ToUpper());
                            Log(element as TopologyNode, "type", (element as TopologyNode).phyAddress.ToString());
                        }
                        else
                            MessageBox.Show("Physical Address is invalid, please try again!", "Invalid Physical Address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                default:
                    if (element is TopologyNode)
                        MessageBox.Show(string.Format("Node {0} has been clicked!", (element as TopologyNode).id));
                    break;
            }
        }
        #endregion

        #region Topology elements
        internal abstract class TopologyElement
        {
            #region Variables
            internal TopologyLoc startLoc; // start-location
            internal TopologyLoc endLoc; // end-location
            #endregion

            internal TopologyElement(TopologyLoc startLoc, TopologyLoc endLoc)
            {
                this.startLoc = startLoc;
                this.endLoc = endLoc;
            }

            internal virtual void TranslateToDelta(short deltaX, short deltaY, TopologyVisualizer redrawVis = null)
            {
                if (redrawVis != null && (this is TopologyNode || this is TopologySwitchNode))
                    redrawVis.EraseNodeRect(this);

                this.startLoc.x += deltaX;
                this.startLoc.y += deltaY;
                this.endLoc.x += deltaX;
                this.endLoc.y += deltaY;

                if (redrawVis != null)
                    if (this is TopologyNode)
                        redrawVis.DrawNodeRect(this as TopologyNode);
                    else if (this is TopologySwitchNode)
                        redrawVis.DrawSwitchNodeRect(this as TopologySwitchNode);
            }

            internal virtual void TranslateToLoc(short newX, short newY, TopologyVisualizer redrawVis = null)
            {
                if (redrawVis != null && (this is TopologyNode || this is TopologySwitchNode))
                    redrawVis.EraseNodeRect(this);

                this.endLoc.x += (short)(newX - this.startLoc.x);
                this.endLoc.y += (short)(newY - this.startLoc.y);
                this.startLoc.x = newX;
                this.startLoc.y = newY;

                if (redrawVis != null)
                    if (this is TopologyNode)
                        redrawVis.DrawNodeRect(this as TopologyNode);
                    else if (this is TopologySwitchNode)
                        redrawVis.DrawSwitchNodeRect(this as TopologySwitchNode);
            }

            internal bool IsInLocation(int x, int y)
            {
                return (startLoc.x <= x & x <= endLoc.x) && (startLoc.y <= y & y <= endLoc.y);
            }
        }

        internal sealed class TopologyNode : TopologyElement
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
                : base(
                      startLoc: new TopologyLoc(posX, posY),
                      endLoc: new TopologyLoc((short)(posX + w), (short)(posY + h))
                )
            {
                this.id = id;

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
                : base(startLoc: startLoc, endLoc: endLoc)
            {
                SetBody(id, tPort, rPort, lPort, type, topMargin);
            }

            internal TopologyNode(TopologyLoc startLoc, TopologyLoc endLoc)
                : base(startLoc: startLoc, endLoc: endLoc)
            { }

            internal void SetBody(short id, TopologyPort tPort, TopologyPort rPort, TopologyPort lPort, TpNodeType type, short topMargin)
            {
                this.id = id;

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
            internal TopologyLoc Center
            {
                get
                {
                    return new TopologyLoc()
                    {
                        x = (short)(startLoc.x + (endLoc.x - startLoc.x) / 2),
                        y = (short)(startLoc.y + (endLoc.y - startLoc.y) / 2)
                    };
                }
            }
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

            internal bool GetPortByDbName(string dbName, out TopologyPort thePort)
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

            internal void ClearConnDrawCache()
            {
                this.portC.connLinkDrawn = false;
                this.portB.connLinkDrawn = false;
                this.portA.connLinkDrawn = false;
            }

            internal void SetRightPortConn(short rPortConnId)
            {
                this.portB.connPortId = rPortConnId;
            }

            internal void SetLeftPortConn(short lPortConnId)
            {
                this.portA.connPortId = lPortConnId;
            }

            internal void SetTopPortConn(short tPortConnId)
            {
                this.portC.connPortId = tPortConnId;
            }

            internal override void TranslateToDelta(short deltaX, short deltaY, TopologyVisualizer redrawVis = null)
            {
                this.portA.TranslateToDelta(deltaX, deltaY);
                this.portB.TranslateToDelta(deltaX, deltaY);
                this.portC.TranslateToDelta(deltaX, deltaY);
                base.TranslateToDelta(deltaX, deltaY, redrawVis);
            }

            internal override void TranslateToLoc(short newX, short newY, TopologyVisualizer redrawVis = null)
            {
                short deltaX = (short)(newX - this.startLoc.x);
                short deltaY = (short)(newY - this.startLoc.y);
                this.TranslateToDelta(deltaX, deltaY, redrawVis);
            }

            internal void Rotate()
            {
                // rotate PORT_A (left port)
                if (portA.startLoc.x == startLoc.x)
                {
                    if (portA.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |██    |      |    ██|
                        //  |      |  ->  |      |
                        //  |      |      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: (short)(endLoc.x - portA.Width),
                            y: short.MinValue
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |██    |
                        //  |      |  ->  |      |
                        //  |██    |      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: startLoc.y
                        ));
                    }
                }
                else
                {
                    if (portA.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |    ██|      |      |
                        //  |      |  ->  |      |
                        //  |      |      |    ██|
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: (short)(endLoc.y - portA.Height)
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |      |
                        //  |      |  ->  |      |
                        //  |    ██|      |██    |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: startLoc.x,
                            y: short.MinValue
                        ));
                    }
                }

                // rotate PORT_B (right port)
                if (portB.startLoc.x == startLoc.x)
                {
                    if (portB.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |██    |      |    ██|
                        //  |      |  ->  |      |
                        //  |      |      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: (short)(endLoc.x - portB.Width),
                            y: short.MinValue
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |██    |
                        //  |      |  ->  |      |
                        //  |██    |      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: startLoc.y
                        ));
                    }
                }
                else
                {
                    if (portB.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |    ██|      |      |
                        //  |      |  ->  |      |
                        //  |      |      |    ██|
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: (short)(endLoc.y - portB.Height)
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |      |
                        //  |      |  ->  |      |
                        //  |    ██|      |██    |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: startLoc.x,
                            y: short.MinValue
                        ));
                    }
                }

                // rotate PORT_C (top port)
                if (portC.startLoc.x == startLoc.x)
                {
                    //   ______        ______
                    //  |      |      |  ██  |
                    //  |██    |  ->  |      |
                    //  |      |      |      |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                    portC.TranslateToLoc(new TopologyLoc(
                        x: (short)(startLoc.x + portC.Width),
                        y: startLoc.y
                    ));
                }
                else if (portC.startLoc.y == startLoc.y)
                {
                    //   ______        ______
                    //  |  ██  |      |      |
                    //  |      |  ->  |    ██|
                    //  |      |      |      |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                    portC.TranslateToLoc(new TopologyLoc(
                        x: (short)(endLoc.x - portC.Width),
                        y: (short)(startLoc.y + portC.Height)
                    ));
                }
                else if (portC.endLoc.x == endLoc.x)
                {
                    //   ______        ______
                    //  |      |      |      |
                    //  |    ██|  ->  |      |
                    //  |      |      |  ██  |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                    portC.TranslateToLoc(new TopologyLoc(
                        x: (short)(startLoc.x + portC.Width),
                        y: (short)(endLoc.y - portC.Height)
                    ));
                }
                else if (portC.endLoc.y == endLoc.y)
                {
                    //   ______        ______
                    //  |      |      |      |
                    //  |      |  ->  |██    |
                    //  |  ██  |      |      |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                    portC.TranslateToLoc(new TopologyLoc(
                        x: startLoc.x,
                        y: (short)(startLoc.y + portC.Height)
                    ));
                }
            }

            internal void FlipVertically()
            {
                // rotate PORT_A (left port)
                if (portA.startLoc.x == startLoc.x)
                {
                    if (portA.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |██    |      |      |
                        //  |      |  ->  |      |
                        //  |      |      |██    |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: (short)(endLoc.y - portA.Height)
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |██    |
                        //  |      |  ->  |      |
                        //  |██    |      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: startLoc.y
                        ));
                    }
                }
                else
                {
                    if (portA.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |    ██|      |      |
                        //  |      |  ->  |      |
                        //  |      |      |    ██|
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: (short)(endLoc.y - portA.Height)
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |    ██|
                        //  |      |  ->  |      |
                        //  |    ██|      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: startLoc.y
                        ));
                    }
                }

                // rotate PORT_B (right port)
                if (portB.startLoc.x == startLoc.x)
                {
                    if (portB.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |██    |      |      |
                        //  |      |  ->  |      |
                        //  |      |      |██    |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: (short)(endLoc.y - portB.Height)
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |██    |
                        //  |      |  ->  |      |
                        //  |██    |      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: startLoc.y
                        ));
                    }
                }
                else
                {
                    if (portB.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |    ██|      |      |
                        //  |      |  ->  |      |
                        //  |      |      |    ██|
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: (short)(endLoc.y - portB.Height)
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |    ██|
                        //  |      |  ->  |      |
                        //  |    ██|      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: short.MinValue,
                            y: startLoc.y
                        ));
                    }
                }

                // rotate PORT_C (top port)
                if (portC.startLoc.x == startLoc.x)
                {
                    //   ______        ______
                    //  |      |      |      |
                    //  |██    |  ->  |██    |
                    //  |      |      |      |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                }
                else if (portC.startLoc.y == startLoc.y)
                {
                    //   ______        ______
                    //  |  ██  |      |      |
                    //  |      |  ->  |      |
                    //  |      |      |  ██  |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                    portC.TranslateToLoc(new TopologyLoc(
                        x: short.MinValue,
                        y: (short)(endLoc.y - portC.Height)
                    ));
                }
                else if (portC.endLoc.x == endLoc.x)
                {
                    //   ______        ______
                    //  |      |      |      |
                    //  |    ██|  ->  |    ██|
                    //  |      |      |      |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                }
                else if (portC.endLoc.y == endLoc.y)
                {
                    //   ______        ______
                    //  |      |      |  ██  |
                    //  |      |  ->  |      |
                    //  |  ██  |      |      |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                    portC.TranslateToLoc(new TopologyLoc(
                        x: short.MinValue,
                        y: startLoc.y
                    ));
                }
            }

            internal void FlipHorizontally()
            {
                // rotate PORT_A (left port)
                if (portA.startLoc.x == startLoc.x)
                {
                    if (portA.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |██    |      |    ██|
                        //  |      |  ->  |      |
                        //  |      |      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: (short)(endLoc.x - portA.Width),
                            y: short.MinValue
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |      |
                        //  |      |  ->  |      |
                        //  |██    |      |    ██|
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: (short)(endLoc.x - portA.Width),
                            y: short.MinValue
                        ));
                    }
                }
                else
                {
                    if (portA.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |    ██|      |██    |
                        //  |      |  ->  |      |
                        //  |      |      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: startLoc.x,
                            y: short.MinValue
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |      |
                        //  |      |  ->  |      |
                        //  |    ██|      |██    |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portA.TranslateToLoc(new TopologyLoc(
                            x: startLoc.x,
                            y: short.MinValue
                        ));
                    }
                }

                // rotate PORT_B (right port)
                if (portB.startLoc.x == startLoc.x)
                {
                    if (portB.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |██    |      |    ██|
                        //  |      |  ->  |      |
                        //  |      |      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: (short)(endLoc.x - portB.Width),
                            y: short.MinValue
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |      |
                        //  |      |  ->  |      |
                        //  |██    |      |    ██|
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: (short)(endLoc.x - portB.Width),
                            y: short.MinValue
                        ));
                    }
                }
                else
                {
                    if (portB.startLoc.y == startLoc.y)
                    {
                        //   ______        ______
                        //  |    ██|      |██    |
                        //  |      |  ->  |      |
                        //  |      |      |      |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: startLoc.x,
                            y: short.MinValue
                        ));
                    }
                    else
                    {
                        //   ______        ______
                        //  |      |      |      |
                        //  |      |  ->  |      |
                        //  |    ██|      |██    |
                        //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                        portB.TranslateToLoc(new TopologyLoc(
                            x: startLoc.x,
                            y: short.MinValue
                        ));
                    }
                }

                // rotate PORT_C (top port)
                if (portC.startLoc.x == startLoc.x)
                {
                    //   ______        ______
                    //  |      |      |      |
                    //  |██    |  ->  |    ██|
                    //  |      |      |      |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                    portC.TranslateToLoc(new TopologyLoc(
                        x: (short)(endLoc.x - portC.Width),
                        y: short.MinValue
                    ));
                }
                else if (portC.startLoc.y == startLoc.y)
                {
                    //   ______        ______
                    //  |  ██  |      |  ██  |
                    //  |      |  ->  |      |
                    //  |      |      |      |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                }
                else if (portC.endLoc.x == endLoc.x)
                {
                    //   ______        ______
                    //  |      |      |      |
                    //  |    ██|  ->  |██    |
                    //  |      |      |      |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                    portC.TranslateToLoc(new TopologyLoc(
                        x: startLoc.x,
                        y: short.MinValue
                    ));
                }
                else if (portC.endLoc.y == endLoc.y)
                {
                    //   ______        ______
                    //  |      |      |      |
                    //  |      |  ->  |      |
                    //  |  ██  |      |  ██  |
                    //   ‾‾‾‾‾‾        ‾‾‾‾‾‾
                }
            }

            public XElement ToXmlElement(string descr = "")
            {
                XElement res = new XElement("TpNode", new XAttribute("descr", descr));

                XElement id = new XElement("id");
                id.Value = this.id.ToString();
                res.Add(id);

                res.Add(this.startLoc.ToXmlElement("Starting Location"));
                res.Add(this.endLoc.ToXmlElement("Ending Location"));

                XElement topMargin = new XElement("topMargin");
                topMargin.Value = this.topMargin.ToString();
                res.Add(topMargin);

                res.Add(this.portC.ToXmlElement("Top Port"));
                res.Add(this.portB.ToXmlElement("Right Port"));
                res.Add(this.portA.ToXmlElement("Left Port"));

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

            public static TopologyNode ParseXml(XElement tpLocXml)
            {
                XNode node = tpLocXml.FirstNode;
                short id = short.Parse(((XElement)node).Value);
                node = node.NextNode;
                TopologyLoc startLoc = TopologyLoc.ParseXml((XElement)node);
                node = node.NextNode;
                TopologyLoc endLoc = TopologyLoc.ParseXml((XElement)node);

                TopologyNode res = new TopologyNode(startLoc, endLoc);

                node = node.NextNode;
                short topMargin = short.Parse(((XElement)node).Value);
                node = node.NextNode;
                TopologyPort tPort = TopologyPort.ParseXml(res, (XElement)node);
                node = node.NextNode;
                TopologyPort rPort = TopologyPort.ParseXml(res, (XElement)node);
                node = node.NextNode;
                TopologyPort lPort = TopologyPort.ParseXml(res, (XElement)node);
                node = node.NextNode;
                TpNodeType type = (TpNodeType)int.Parse(((XElement)node).Value);

                res.SetBody(id, tPort, rPort, lPort, type, topMargin);

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

        internal class TopologyPort : TopologyElement
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
            internal TopologyNode parent;
            internal Brush brush;
            internal short connPortId;
            internal TpPortType connPortType;
            internal bool connLinkDrawn;
            internal bool multipleConnSupported;
            public short Width { get { return (short)(endLoc.x - startLoc.x); } }
            public short Height { get { return (short)(endLoc.y - startLoc.y); } }
            #endregion

            internal TopologyPort(TpPortType type, TopologyLoc startLoc, TopologyLoc endLoc, TopologyNode parent, Brush brush = null, bool multipleConnSupported = false)
                : base(startLoc: startLoc, endLoc: endLoc)
            {
                this.type = type;
                this.startLoc = startLoc;
                this.endLoc = endLoc;
                this.parent = parent;
                this.connPortId = DISCONNECTED;
                this.connPortType = TpPortType.NULL;
                this.brush = brush ?? Brushes.Green;
                this.multipleConnSupported = multipleConnSupported;
            }

            internal TopologyLoc GetCenter()
            {
                return new TopologyLoc
                {
                    x = (short)(startLoc.x + (endLoc.x - startLoc.x) / 2),
                    y = (short)(startLoc.y + (endLoc.y - startLoc.y) / 2)
                };
            }

            internal void TranslateToLoc(TopologyLoc toLoc)
            {
                if (toLoc.x != short.MinValue)
                {
                    endLoc.x = (short)(toLoc.x + Width);
                    startLoc.x = toLoc.x;
                }

                if (toLoc.y != short.MinValue)
                {
                    endLoc.y = (short)(toLoc.y + Height);
                    startLoc.y = toLoc.y;
                }
            }

            internal virtual XElement ToXmlElement(string descr = "")
            {
                XElement res = new XElement("TpPort", new XAttribute("descr", descr));

                XElement type = new XElement("type");
                type.Value = this.type.ToString("d");
                res.Add(type);

                res.Add(this.startLoc.ToXmlElement("Starting Location"));
                res.Add(this.endLoc.ToXmlElement("Ending Location"));

                XElement connId = new XElement("connId");
                connId.Value = this.connPortId.ToString();
                res.Add(connId);

                XElement connPortType = new XElement("connPortType");
                connPortType.Value = ((int)this.connPortType).ToString();
                res.Add(connPortType);

                return res;
            }

            internal static TopologyPort ParseXml(TopologyNode parent, XElement tpLocXml)
            {
                XNode node = tpLocXml.FirstNode;
                TpPortType type = (TpPortType)int.Parse(((XElement)node).Value);
                node = node.NextNode;
                TopologyLoc startLoc = TopologyLoc.ParseXml((XElement)node);
                node = node.NextNode;
                TopologyLoc endLoc = TopologyLoc.ParseXml((XElement)node);
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

        internal sealed class TopologySwitchNode : TopologyPort
        {
            #region Variables
            internal readonly short id;
            #endregion

            internal TopologySwitchNode(short id, TopologyLoc startLoc, TopologyLoc endLoc)
                : base(
                      type: TpPortType.NULL,
                      startLoc: startLoc,
                      endLoc: endLoc,
                      parent: null,
                      brush: Brushes.SlateBlue,
                      multipleConnSupported: true
                )
            {
                this.id = id;
            }

            internal TopologySwitchNode(short id, short posX, short posY, short w, short h)
                : base(
                      type: TpPortType.NULL,
                      startLoc: new TopologyLoc(posX, posY),
                      endLoc: new TopologyLoc((short)(posX + w), (short)(posY + h)),
                      parent: null,
                      brush: Brushes.SlateBlue,
                      multipleConnSupported: true
                )
            {
                this.id = id;
            }

            internal override XElement ToXmlElement(string descr = "")
            {
                XElement res = base.ToXmlElement(descr);

                res.Name = "TpSwitchNode";
                XElement idElement = new XElement("id");
                idElement.Value = this.id.ToString();
                res.Add(idElement);

                return res;
            }

            internal static TopologySwitchNode ParseXml(XElement tpSwitchNodeXml)
            {
                TopologyPort portVersion = ParseXml(null, tpSwitchNodeXml);

                XNode idNode = tpSwitchNodeXml.LastNode;
                short id = short.Parse(((XElement)idNode).Value);

                return new TopologySwitchNode(id, portVersion.startLoc, portVersion.endLoc);
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

            internal static TopologyLoc CenterOf(TopologyLoc startLoc, TopologyLoc endLoc)
            {
                return new TopologyLoc((short)(startLoc.x + (endLoc.x - startLoc.x) / 2), (short)(startLoc.y + (endLoc.y - startLoc.y) / 2));
            }

            public static explicit operator Point(TopologyLoc loc)
            {
                return new Point(loc.x, loc.y);
            }

            public bool Is(short x, short y)
            {
                return this.x == x && this.y == y;
            }

            public XElement ToXmlElement(string descr = "")
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

            public static TopologyLoc ParseXml(XElement tpLocXml)
            {
                XNode node = tpLocXml.FirstNode;

                short x = short.Parse(((XElement)node).Value);
                node = node.NextNode;
                short y = short.Parse(((XElement)node).Value);

                return new TopologyLoc(x, y);
            }
        }
        #endregion

        internal void ExportNodeXmls(string xmlFilePath, string timestamp = "")
        {
            XElement switchesXml = new XElement("TpSwitches");
            foreach (TopologySwitchNode switchNode in switchNodes.Values)
                switchesXml.Add(switchNode.ToXmlElement(string.Format("Switch {0}", switchNode.id)));
            XElement nodesXml = new XElement("TpNodes");
            foreach (TopologyNode node in nodes.Values)
                nodesXml.Add(node.ToXmlElement(string.Format("Node {0}", node.id)));

            XElement root = new XElement("TPElements", new XAttribute("timestamp", timestamp));
            root.Add(switchesXml);
            root.Add(nodesXml);
            using (StreamWriter file = new StreamWriter(xmlFilePath, false))
                file.WriteLine(root.ToString());
        }

        internal void ImportNodeXmls(string xmlFilePath)
        {
            nodes.Clear();
            switchNodes.Clear();

            XElement root = XElement.Load(xmlFilePath);
            IEnumerator<XNode> enumerator = root.Nodes().GetEnumerator();

            enumerator.MoveNext();
            foreach (XNode switchesXml in (enumerator.Current as XElement).Nodes())
            {
                TopologySwitchNode switchNode = TopologySwitchNode.ParseXml((XElement)switchesXml);
                switchNodes.Add(switchNode.id, switchNode);
            }
            enumerator.MoveNext();
            foreach (XNode nodesXml in (enumerator.Current as XElement).Nodes())
            {
                TopologyNode tpNode = TopologyNode.ParseXml((XElement)nodesXml);
                nodes.Add(tpNode.id, tpNode);
            }
        }

        private void Log(TopologyNode node = null, string key = null, string value = null)
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

        internal delegate void NodeNonLeftMouseUpDelegate(MouseEventArgs e);

        internal delegate void NodeNonLeftMouseDownDelegate(TopologyNode node, MouseEventArgs e);
    }
}
