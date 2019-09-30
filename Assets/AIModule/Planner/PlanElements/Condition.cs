﻿using System.Collections.Generic;

public class Sense
{
    private string name = "";
    internal string Name
    {
        get
        {
            return name;
        }
    }

    private double value;
    internal double Value
    {
        get
        {
            return value;
        }
    }

    private string comperator;
    internal string Comperator
    {
        get
        {
            return comperator;
        }
    }

    public Sense(string name, double value, string comperator)
    {
        this.name = name;
        this.value = value;
        this.comperator = comperator;
    }
}