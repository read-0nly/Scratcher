using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Scratcher
{
    class sysinfo
    {private PerformanceCounter cpuCounter;
private PerformanceCounter ramCounter;
public sysinfo()
{
    InitialiseCPUCounter();
    InitializeRAMCounter();
}

private void updateTimer_Tick(object sender, EventArgs e)
{
    string Text = "CPU Usage: " +
    Convert.ToInt32(cpuCounter.NextValue()).ToString() +
    "%";

    string text = "Avaliable RAM:"+(Convert.ToInt32(ramCounter.NextValue())).ToString() + "Mb";
}

public int availableRam()
{
    return Convert.ToInt32(ramCounter.NextValue());
}

public int cpuUsage()
{
    return Convert.ToInt32((cpuCounter.NextValue()));
}
private void InitialiseCPUCounter()
{
    cpuCounter = new PerformanceCounter(
    "Processor",
    "% Processor Time",
    "_Total",
    true
    );
    cpuCounter.NextValue();
}

private void InitializeRAMCounter()
{
    ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use", true);
    ramCounter.NextValue();

}
    }
}
