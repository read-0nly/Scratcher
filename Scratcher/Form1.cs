using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics.Eventing.Reader;

namespace Scratcher
{
    public partial class Form1 : Form
    {
        List<string> x = new List<string>();
        XmlDocument xmlDoc = new XmlDocument();
        System.Windows.Forms.TabControl tabControl;
		Credentials c = null;
        sysinfo s = new sysinfo();
        int clockTick = 0;
        int clockLimit = 1024;
        int clockRate = 1000;

        public Form1(string[] args)
        {
            InitializeComponent();

            (xmlDoc).Load((args.Length > 0 && args[0] != null && ((string)(args[0])) != "." ? ((string)(args[0])) : ".\\Menu.xml"));

            if (xmlDoc.Attributes != null)
            {
                clockTick = (xmlDoc.Attributes["clockTick"] != null ? Convert.ToInt32(xmlDoc.Attributes["clockTick"].Value) : clockTick);
                clockLimit = (xmlDoc.Attributes["clockLimit"] != null ? Convert.ToInt32(xmlDoc.Attributes["clockLimit"].Value) : clockLimit);
                clockRate = (xmlDoc.Attributes["clockRate"] != null ? Convert.ToInt32(xmlDoc.Attributes["clockRate"].Value) : clockRate);
            }

            timer1.Interval = clockRate;

            if((args.Length>1 && args[1] != null && ((string)(args[1])) != ".")){
                switch(args[1]){
                    default:
                        break;
                    case "0":
                        break;
                    case "1":

                        c = new Credentials();
                        this.Text = this.Text + " - " + c.username;
                        break;
                }
            }
             // tabControl
            tabControl = new System.Windows.Forms.TabControl();
            tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            tabControl.Location = new System.Drawing.Point(12, 12);
            tabControl.Margin = new System.Windows.Forms.Padding(0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.AllowDrop = true;
            tabControl.Size = new System.Drawing.Size(this.Size.Width-40, this.Size.Height-110);
            tabControl.TabIndex = 0;
            Controls.Add(tabControl);
            processXML(xmlDoc.CloneNode(true), tabControl);
            this.AllowDrop = true;
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Tab_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Tab_DragEnter);
            
            label2.Text = TimeSpan.FromMilliseconds((UInt32)System.Environment.TickCount).Hours.ToString() + " hrs";

            sysinfo s = new sysinfo();
            progressBar1.Value = s.cpuUsage();
            progressBar2.Value = s.availableRam();

        }
        
        #region XML Handlers

        public void processXML(XmlNode xmlD, object parent)
        {
            try
            {
                foreach (XmlNode xmlE in xmlD.ChildNodes)
                {
                    object nextPart = new object();
                    switch (xmlE.Name)
                    {
                        case "Category":
                            nextPart = newTab(xmlE.Attributes["title"].Value, Color.FromName(xmlE.Attributes["color"].Value), xmlE.Attributes["text"].Value, true);

                            ((activePanel)nextPart).xml = xmlE;
                            try
                            {
                                ((Control)parent).Controls.Add(((FlowLayoutPanel)nextPart).Parent);
                            }
                            catch (Exception e) { }
                            break;
                        case "StaticDatum":
                            nextPart = newLabel(xmlE, false);
                            ((Control)parent).Controls.Add(((Label)nextPart));
                            break;
                        case "CmdDatum":
                            nextPart = newLabel(xmlE, false);
                            ((activePanel)parent).dynalbl.Add((activeLbl)nextPart);
                            ((Control)parent).Controls.Add(((Label)nextPart));
                            break;
                        case "PerfDatum":
                            nextPart = newLabel(xmlE,true);
                            ((Control)parent).Controls.Add(((Label)nextPart));
                            ((activePanel)parent).dynalbl.Add((activeLbl)nextPart);
                            System.Diagnostics.PerformanceCounter pc;
                            if (((activeLbl)nextPart).xml.Attributes["InstanceName"] == null)
                            {
                                pc = new System.Diagnostics.PerformanceCounter(((activeLbl)nextPart).xml.Attributes["CategoryName"].Value, ((activeLbl)nextPart).xml.Attributes["CounterName"].Value);
                            }
                            else{
                                pc = new System.Diagnostics.PerformanceCounter(((activeLbl)nextPart).xml.Attributes["CategoryName"].Value, ((activeLbl)nextPart).xml.Attributes["CounterName"].Value, ((activeLbl)nextPart).xml.Attributes["InstanceName"].Value);
                            }
                            ((activeLbl)nextPart).pc = pc;
                            break;
                        case "EvtDatum":
                            nextPart = newLabel(xmlE,true);
                            ((Control)parent).Controls.Add(((Label)nextPart));
                            ((activePanel)parent).dynalbl.Add((activeLbl)nextPart);
                            foreach (XmlNode qXml in ((((activeLbl)nextPart).xml.ChildNodes.Item(0)).ChildNodes.Item(0)).ChildNodes.Item(0))
                            {
                                ((activeLbl)nextPart).eventsQuery.Add((new EventLogQuery(qXml.Attributes["Path"].Value, PathType.LogName, qXml.InnerText)));

                                ((activeLbl)nextPart).eventsReader.Add(new EventLogReader((new EventLogQuery(qXml.Attributes["Path"].Value, PathType.LogName, qXml.InnerText))));
                            }
                            break;
                        case "Action":
                            nextPart = newButton(xmlE, new EventHandler(buttonClick));
                            ((FlowLayoutPanel)parent).Controls.Add((Button)nextPart);
                            break;
                        case "MenuRoot": processXML(xmlE, parent); break;

                    }
                    if (xmlE.HasChildNodes)
                    {
                        processXML(xmlE, nextPart);
                    }
                }
            }

            catch(Exception e){

            }
            
        }


        string getDatumValue(activeLbl label)
        {
            int refreshRate = 100;
            switch (label.xml.Name)
            {


                default: return "";
                case "StaticDatum":
                    if (label.xml.Attributes["Value"] != null)
                    {
                        return label.xml.Attributes["Value"].Value;

                    }
                    else
                    {
                        return "";
                    }
                case "CmdDatum":
                    refreshRate = (((activeLbl)label).xml.Attributes["refresh"] != null ? Convert.ToInt32(((activeLbl)label).xml.Attributes["refresh"].Value) : 12);
                    if (clockTick % refreshRate == 0 || ((activeLbl)label).firstRun )
                    {
                        ((activeLbl)label).firstRun = false;
                        string x = "";
                        if (c != null && c.domain != null && ((activeLbl)label).xml.Attributes["cmd"]!=null )
                        {
                            x = (c.RunAs(((activeLbl)label).xml.Attributes["cmd"].Value, (((activeLbl)label).xml.Attributes["args"]!=null?((activeLbl)label).xml.Attributes["args"].Value:""), true)).Last();
                        }
                        else if (((activeLbl)label).xml.Attributes["cmd"] != null)
                        {
                            System.Diagnostics.ProcessStartInfo pS = new System.Diagnostics.ProcessStartInfo(((activeLbl)label).xml.Attributes["cmd"].Value, (((activeLbl)label).xml.Attributes["args"] != null ? ((activeLbl)label).xml.Attributes["args"].Value : ""));
                            pS.UseShellExecute = false;
                            pS.CreateNoWindow = true;
                            System.Diagnostics.Process.Start(pS);

                            pS.RedirectStandardOutput = true;
                            System.Diagnostics.Process p = System.Diagnostics.Process.Start(pS);
                            List<string> output = new List<String>();
                            while (!p.StandardOutput.EndOfStream)
                            {
                                output.Add(p.StandardOutput.ReadLine());
                            }
                            return output.Last(); ;

                        }
                        else
                        {
                            return "Bad XML entry";
                        }

                        return x;
                    }
                    else
                    {
                        return label.Text.Replace(label.xml.Attributes["title"].Value + "\r\n-------\r\n", "");
                    }
                case "PerfDatum":
                    refreshRate = (((activeLbl)label).xml.Attributes["refresh"] != null ? Convert.ToInt32(((activeLbl)label).xml.Attributes["refresh"].Value) : 12);
                    if (clockTick % refreshRate == 0 || ((activeLbl)label).firstRun)
                    {
                        ((activeLbl)label).firstRun = false;
                        float i = -1;
                        try
                        {
                            if (label.pc != null)
                            {
                                i = label.pc.NextValue();
                            }
                        }
                        catch (EventLogNotFoundException e)
                        {
                            Console.WriteLine("Error while reading the perf logs");
                            return "Error";
                        }
                        return i.ToString();
                    }
                    else
                    {
                        return label.Text.Replace(label.xml.Attributes["title"].Value + "\r\n-------\r\n", "");
                    }

                case "EvtDatum":
                    refreshRate = (((activeLbl)label).xml.Attributes["refresh"] != null ? Convert.ToInt32(((activeLbl)label).xml.Attributes["refresh"].Value) : 12);
                    if (clockTick % refreshRate == 0 || ((activeLbl)label).firstRun)
                    {
                        ((activeLbl)label).firstRun = false;
                        int count = 0;
                        foreach (EventLogQuery q in label.eventsQuery)
                        {

                            try
                            {
                                EventLogReader r = new EventLogReader(q);

                                for (EventRecord eventdetail = r.ReadEvent(); eventdetail != null; eventdetail = r.ReadEvent())
                                {
                                    // Read Event details
                                    count++;
                                }

                            }
                            catch (EventLogNotFoundException e)
                            {
                                Console.WriteLine("Error while reading the event logs");
                                return "Error";
                            }

                        }
                        return count.ToString();
                    }
                    else { return label.Text.Replace(label.xml.Attributes["title"].Value + "\r\n-------\r\n", ""); }
                   


            }

        }

        #endregion

        #region Generators
        Button newButton(XmlNode xelem, EventHandler evHdlr)
        {
            actionBtn actBtn;
            actBtn = new actionBtn();
            actBtn.AutoSize = true;
            actBtn.BackColor = (xelem.Attributes["bColor"] != null? Color.FromName(xelem.Attributes["bColor"].Value) : Color.LightGray);
            actBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            actBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            actBtn.ForeColor = (xelem.Attributes["fColor"] != null ? Color.FromName(xelem.Attributes["fColor"].Value) : Color.Black);
            actBtn.Location = new System.Drawing.Point(6, 6);
            actBtn.Name = xelem.Attributes["title"].Value;
            actBtn.MaximumSize = new System.Drawing.Size(150, 25);
            actBtn.MinimumSize = new System.Drawing.Size(150, 25);
            actBtn.TabIndex = 0;
            actBtn.Text = xelem.Attributes["text"].Value; 
            actBtn.UseVisualStyleBackColor = false;
            actBtn.Click += new System.EventHandler(evHdlr);
            actBtn.Command = xelem.Attributes["cmd"].Value;
            actBtn.args = (xelem.Attributes["args"] != null ? xelem.Attributes["args"].Value : "");
            return actBtn;
        }

        activeLbl newLabel(XmlNode xelem, bool active)
        {

            // 
            // dataLbl
            // 
            activeLbl dataLbl;
            dataLbl = new activeLbl();
            dataLbl.xml = xelem; 
            dataLbl.AutoSize = true;
            dataLbl.BackColor = (xelem.Attributes["bColor"] != null ? Color.FromName(xelem.Attributes["bColor"].Value) : Color.LightGray);
            dataLbl.BorderStyle = ((xelem.Attributes["noborder"] != null) && (xelem.Attributes["noborder"].Value == "1") ? System.Windows.Forms.BorderStyle.None : System.Windows.Forms.BorderStyle.FixedSingle);
            dataLbl.ForeColor = (xelem.Attributes["fColor"] != null ? Color.FromName(xelem.Attributes["fColor"].Value) : Color.Black);
            dataLbl.Location = new System.Drawing.Point(8, 8);
            dataLbl.Margin = new System.Windows.Forms.Padding(3);
            dataLbl.Name = xelem.Attributes["title"].Value;
            dataLbl.Padding = new System.Windows.Forms.Padding(3);
            dataLbl.MaximumSize = new System.Drawing.Size(150, 100);
            dataLbl.MinimumSize = new System.Drawing.Size(150, 25);
            dataLbl.TabIndex = 0;
            dataLbl.Text = "Loading";
            dataLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            return dataLbl;
        }

        
        activePanel newTab(string name,Color c, string title, bool ltr)
        {

            // 
            // utilPage
            // 
            System.Windows.Forms.TabPage utilPage;
            utilPage = new System.Windows.Forms.TabPage();
            utilPage.AllowDrop = true;
            utilPage.BackColor = c;
            utilPage.Location = new System.Drawing.Point(0, 0);
            utilPage.Name = name;
            utilPage.Margin = new System.Windows.Forms.Padding(0);
            utilPage.Padding = new System.Windows.Forms.Padding(0);
            utilPage.TabIndex = 1;
            utilPage.Text = title;
            utilPage.DragDrop += new System.Windows.Forms.DragEventHandler(this.Tab_DragDrop);
            utilPage.DragEnter += new System.Windows.Forms.DragEventHandler(this.Tab_DragEnter);
            tabControl.SelectedIndexChanged += new EventHandler(this.timer1_Tick);
            // 
            // flowLayoutPanel2
            // 
            activePanel flowLayoutPanel2;
            flowLayoutPanel2 = new activePanel();
            flowLayoutPanel2.AllowDrop = true;
            flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            flowLayoutPanel2.AutoScroll = true;
            flowLayoutPanel2.FlowDirection = (ltr?System.Windows.Forms.FlowDirection.LeftToRight:System.Windows.Forms.FlowDirection.TopDown);
            flowLayoutPanel2.Location = new System.Drawing.Point(5, 5);
            flowLayoutPanel2.Name = name + "Panel";
            flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            flowLayoutPanel2.Padding = new System.Windows.Forms.Padding(0);
            flowLayoutPanel2.Size = utilPage.Size - (new System.Drawing.Size(5, 5));
            flowLayoutPanel2.TabIndex = 0;
            utilPage.Controls.Add(flowLayoutPanel2);
            return flowLayoutPanel2;
        }
        #endregion
        
        #region Basic GUI stuff 

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            MethodInvoker inv = delegate
            {
                refreshLbl.ForeColor = Color.LimeGreen;
            };
            this.Invoke(inv);
            crossThreadUpd(); 
            MethodInvoker inv2 = delegate
            {
                refreshLbl.ForeColor = Color.Black;
            };
            this.Invoke(inv2);
        }

        private void crossThreadUpd()
        {
            MethodInvoker inv = delegate
            {

                label2.Text = TimeSpan.FromMilliseconds((UInt32)System.Environment.TickCount).Hours.ToString() + " hrs";
       
                activePanel aP = ((activePanel)(((TabPage)(tabControl.SelectedTab)).Controls[0]));
                foreach (activeLbl a in aP.dynalbl)
                {
                    String x =  getDatumValue(a);
                    a.Text = (x != null ? a.xml.Attributes["title"].Value + "\r\n-------\r\n" + x : a.xml.Attributes["title"].Value);
                }
                progressBar1.Value = s.cpuUsage();
                progressBar2.Value = s.availableRam();
            };

             this.Invoke(inv);
             clockTick = (clockTick >= clockLimit ? 0 : clockTick);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
        
        #endregion
        
        #region Event Handlers

        private void buttonClick(object sender, EventArgs e)
        {
            actionBtn x = ((actionBtn)sender);
            if (c == null)
            {
                if (x.process == null) {
                    System.Diagnostics.ProcessStartInfo pS = new System.Diagnostics.ProcessStartInfo(x.Command, x.args);
                    pS.UseShellExecute = true;
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo = pS;
                    x.process = proc;
                }
                try
                {
                    if (x.process.StartTime == null)
                    {
                        x.process.Start();
                        x.Text = "> " + x.Text.Replace(">", "").Replace("<", "") + " <";
                    }
                    else
                    {
                        x.process.Kill();
                        x.Text = x.Text.Replace(">", "").Replace("<", "");
                    }
                }
                catch(Exception ex)
                {
                    x.process.Start();
                    x.Text = "> " + x.Text.Replace(">", "").Replace("<", "") + " <";
                }
            }
            else
            {
                c.RunAs(x.Command, x.args);
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = checkBox1.Checked;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        
        private void Tab_DragEnter(object sender, DragEventArgs e)
        {
        }
        private void Tab_DragDrop(object sender, DragEventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {
            (new sandbox()).Show();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {

            e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string s in files)
                {
                    FileInfo fI = new FileInfo(s);
                    XmlDocument temp = new XmlDocument();
                    temp.LoadXml("<new><elements/></new>");
                    activePanel aP = ((activePanel)sender);
                    XmlNode new_node = temp.CloneNode(false);
                    ComponentCollection cc = aP.Container.Components;
                    aP.xml.InsertAfter(new_node, aP.xml.LastChild);

                }

            }
        }

        private void label1_DoubleClick(object sender, EventArgs e)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            clockTick++;
            if (!backgroundWorker1.IsBusy) { backgroundWorker1.RunWorkerAsync(); }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();

        }
        #endregion

            }
    }

