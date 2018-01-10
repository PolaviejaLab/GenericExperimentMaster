using System.Collections.Generic;
using System.IO;

/**
 * List of trials.
 */
public class ICTrialList
{
	private Queue<Dictionary<string, string>> trials;


    /**
     * Create a trial list from a CSV file.
     */
	public ICTrialList(string filename)
	{
		StreamReader reader = new StreamReader(filename);		
		string[] header = reader.ReadLine().Split(',');
		
		trials = new Queue<Dictionary<string, string>>();
		
		while(!reader.EndOfStream)
		{
			string line = reader.ReadLine();
			string[] columns = line.Split(',');
			
			Dictionary<string, string> trial = new Dictionary<string, string>();
			
			for(int i = 0; i < columns.Length; i++) {
				trial[header[i].Trim ()] = columns[i].Trim();
			}
			
			trials.Enqueue(trial);
		}		
	}
	

    /**
     * Returns true if there are more trials in the list.
     */
	public bool HasMore()
	{
		return trials.Count != 0;
	}
	

    /**
     * Returns the total number of trials in the list.
     */
	public int Count()
	{
		return trials.Count;
	}
	

    /**
     * Return the next trial from the list.
     */
	public Dictionary<string, string> Pop()
	{
		return trials.Dequeue();
	}
}
