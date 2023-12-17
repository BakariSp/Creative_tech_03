using System;
using System.Collections.Generic;

public enum CubeState {
    Red = 1,
    Green = 2,
    Blue = 3,
    Black = 4,
    White = 5,
    Empty = 6
    // Add other states as needed
}
public class Rule
{
    public CubeState[] pattern;
    public CubeState[] replacement;

    //public Rule(int[] p, int[] r)
    //{
    //    pattern = p;
    //    replacement = r;
    //}

    public Rule(CubeState[] p, CubeState[] r)
    {
        pattern = p;
        replacement = r;
    }
    
}

public class GridState {
    public List<int> possibileSpace;
    public bool isAssigned;
    public CubeState cubeState;

    public GridState(List<int> l, bool a, CubeState c)
    {
        possibileSpace = l;
        isAssigned = a;
        cubeState = c;
    }
}
