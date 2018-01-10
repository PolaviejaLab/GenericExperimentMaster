using UnityEngine;

using System.Threading;
using NUnit.Framework;

[TestFixture]
public class ICDictionaryTest
{
    /**
     * Make sure event handler is called on insert.
     */
    [Test]
    public void InsertTest()
    {
        AutoResetEvent ev = new AutoResetEvent(false);

        ICDictionary<string, string> dictionary = new ICDictionary<string, string>();
        dictionary.OnChanged.AddListener(() => { ev.Set(); });

        dictionary.Add("Key", "Value");
        Assert.IsTrue(ev.WaitOne(100));
        Assert.IsTrue(dictionary.ContainsKey("Key") && dictionary["Key"] == "Value");
    }


    /**
     * Test deleting of an item with an event handler present.
     */
    [Test]
    public void DeleteTest()
    {
        AutoResetEvent ev = new AutoResetEvent(false);

        ICDictionary<string, string> dictionary = new ICDictionary<string, string>();
        dictionary.Add("Key", "Value");

        dictionary.OnChanged.AddListener(() => { ev.Set(); });
        dictionary.Remove("Key");

        Assert.IsTrue(ev.WaitOne(100));
        Assert.IsFalse(dictionary.ContainsKey("Key"));
    }


    /**
     * Test adding and removing items without a handler being present.
     */
    [Test]
    public void NoHandlerTest()
    {
        ICDictionary<string, string> dictionary = new ICDictionary<string, string>();
        Assert.IsFalse(dictionary.ContainsKey("Key"));
        dictionary.Add("Key", "Value");
        Assert.IsTrue(dictionary.ContainsKey("Key"));
        dictionary.Remove("Key");
        Assert.IsFalse(dictionary.ContainsKey("Key"));
    }
}
