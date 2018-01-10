/**
 * Script that manages a listbox.
 *
 * Copyright (c) 2015-2016 Ivar Clemens for Champalimaud Centre for the Unknown, Lisbon
 */
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ICListBox : MonoBehaviour
{
    public Transform template;

    /** Dictionary containing items to present **/
    public ICDictionary<string, string> items = new ICDictionary<string, string>();

    /** Currently selected item **/
    public string selectedItem = "";

    /** Display properties **/
    public int verticalMargin = 30;
    public int interItemSpace = 10;
    public int itemHeight = 43;

    public Color textColor;
    public Color backgroundColor;

    public void Awake()
    {
        items.OnChanged.AddListener(() => {
            RebuildList();
        });

        if(!template)
            Debug.LogWarning("Template not set for ListBox: " + this.name);
    }


    public void Start()
    {
        // Move to top of listbox
        RectTransform content = GetContentTransform().GetComponent<RectTransform>();
        content.localPosition = new Vector2(0, 0);
    }


    /**
     * Returns the transform containing the listbox items.
     */
    private Transform GetContentTransform()
    {
        Transform viewport = this.transform.FindChild("Viewport");
        Transform content = viewport.FindChild("Content");

        return content;
    }


    /**
     * Refresh listbox contents
     */
    void RebuildList()
    {
        Transform content = GetContentTransform();
        var childCount = content.childCount;

        var index = 0;
        foreach(KeyValuePair<string, string> entry in items) {            
            if(index < childCount) { 
                // Child already exists, update existing
                Transform child = content.GetChild(index);
                UpdateObject(child, entry.Key, entry.Value);
            } else {
                // Create new child
                AddObject(index, entry.Key, entry.Value);
            }

            index++;
        }

        List<Transform> buffer = new List<Transform>();
        for(; index < childCount; index++) {
            buffer.Add(content.GetChild(index));
        }

        foreach(Transform child in buffer) {
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        UpdateSelection(selectedItem);
    }


    /**
     * Update the selection
     */
    private void UpdateSelection(string name)
    {
        Transform content = GetContentTransform();
        selectedItem = name;

        for(var i = 0; i < content.childCount; i++) {
            Transform child = content.GetChild(i);
            Image image = child.GetComponent<Image>();
            Text text = child.FindChild("ItemLabel").GetComponent<Text>();

            if(child.name == selectedItem) {
                image.color = Color.blue;
                text.color = Color.white;
            } else {
                image.color = Color.white;
                text.color = Color.black;                
            }
        }
    }


    /**
     * Changes the caption and name of an object.
     */
    private void UpdateObject(Transform child, string name, string caption)
    {
        child.name = name;

        Text label = child.FindChild("ItemLabel").GetComponent<Text>();
        label.text = caption;

        Button button = child.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            UpdateSelection(child.name);
        });
    }


    /**
     * Adds a new item to the listbox.
     */
    private void AddObject(int index, string name, string caption)
    {
        if(!template) return;

        Transform copy = Instantiate(template);       

        Vector3 position = template.localPosition;
        position.y = -verticalMargin - index * (interItemSpace + itemHeight);

        copy.name = name;

        copy.SetParent(GetContentTransform());
        copy.localPosition = position;

        UpdateObject(copy, name, caption);
    }

}
