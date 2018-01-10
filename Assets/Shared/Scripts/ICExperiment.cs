/**
 * Base-object containing meta-data about an experiment
 *
 * Copyright (c) 2015-2016 Ivar Clemens for Champalimaud Centre for the Unknown, Lisbon
 */
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/**
 * Responsible for bootstrapping an experiment
 */
public class ICExperiment: MonoBehaviour 
{
    [Tooltip("Name shown in the UI")]
    public string friendlyName = "";

    [Tooltip("Minimum number of participants")]
    public int minimumParticipants = 1;

    [Tooltip("Maximum number of participants")]
    public int maximumParticipants = 1;

    [Tooltip("Name of the scene to transition to")]
    public string sceneName;


    /**
     * Returns display name
     */
    public string getDisplayName() {
        if(friendlyName != "")
            return friendlyName;
        if(name != "")
            return name;
        return "Unnamed experiment";
    }
}
