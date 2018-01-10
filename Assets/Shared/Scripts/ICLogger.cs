using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Collections;
using UnityOSC;

/**
 * Logging component that writes 
 */
public class ICLogger : MonoBehaviour 
{
    /**
     * Send log messages to OSC node
     */
    public bool useOSC = false;

    /**
     * OSC address and port
     */
    public string oscAddress = "127.0.0.1";
    public int oscPort = 4567;

    private StreamWriter writer;
    private OSCClient oscClient = null;


    /**
     * Open OSC connection when constructing class
     */
    public ICLogger(): base()
    {
        if(useOSC)
            oscClient = new OSCClient(IPAddress.Parse(oscAddress), oscPort);
    }


    /**
     * Open a new log-file for output
     */
    public void OpenLog(string filename)
    {
        writer = new StreamWriter(filename, true);
        Write("Logger\tStarted logging");
    }


    /**
     * Close the log-file
     */
    public void CloseLog()
    {
        Write("Logger\tStopped logging");
        writer.Close();
    }


    /**
     * Write message to log file and (if open) OSC
     */
    public void Write(string message)
    {
        if(writer != null) {
            writer.WriteLine(DateTime.UtcNow.ToString("o") + "\t" + message);
            writer.Flush();
        }
        
        if(oscClient != null) {
            OSCMessage packet = new OSCMessage("/", message);
            oscClient.Send(packet);
        }
    }
}
