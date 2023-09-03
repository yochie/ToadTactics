using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct HexCoordinates : IEquatable<HexCoordinates>
{

	public static Vector3[] directionVectors = {
		new Vector3(1,0,-1),
		new Vector3(1,-1,0),
		new Vector3(0,-1,1),
		new Vector3(0,1,-1),
		new Vector3(-1,0,1),
		new Vector3(-1,1,0)
	};

	public readonly bool isFlatTop;

	//cube coordinates
	public readonly int q, r;
	public int Q { get { return this.q; } }

	public int R { get { return this.r; } }

	public int S
	{
		get
		{
			return -Q - R;
		}
	}

	//offset coordinates
	public int X { get { return this.isFlatTop ? this.Q : this.Q + (this.R - Math.Abs(this.R % 2)) / 2; } }
	public int Y { get { return this.isFlatTop ? this.R  + (this.Q - Math.Abs(this.Q%2)) / 2 : this.R; } }

	public Vector2Int OffsetCoordinatesAsVector()
    {
		return new Vector2Int(this.X, this.Y);
    }

	public HexCoordinates (int q, int r, bool isFlatTop) {
		this.q = q;
		this.r = r;
		this.isFlatTop = isFlatTop;
	}

	public static HexCoordinates FromOffsetCoordinates(int x, int y, bool isFlatTop)
	{
        if (isFlatTop)
        {
			return new HexCoordinates(x, y - ((x - Math.Abs(x%2) ) / 2), isFlatTop);
		}
		else
        {
			return new HexCoordinates(x - ((y - Math.Abs(y % 2)) / 2), y, isFlatTop);
		}
	}

	public override string ToString()
	{
		return "(" + this.Q.ToString() + ", " + this.R.ToString() + ", " + S.ToString() + ")";
	}

    internal static HexCoordinates RotateVector(HexCoordinates toRotate, bool clockwise)
    {
		if(clockwise)
			return new HexCoordinates(-toRotate.R, -toRotate.S, toRotate.isFlatTop);        
		else
			return new HexCoordinates(-toRotate.S, -toRotate.Q, toRotate.isFlatTop);
	}

    internal string ToStringOnLines()
    {
		return this.Q.ToString() + "\n" + this.R.ToString() + "\n" + S.ToString();
	}

	public HexCoordinates Add(Vector3 dir)
	{
		return new HexCoordinates(this.Q + (int)dir.x, this.R + (int)dir.y, this.isFlatTop);
	}

	public HexCoordinates Add(HexCoordinates dir)
	{
		if (this.isFlatTop != dir.isFlatTop)
			throw new Exception("Trying to add coordinates with different flatTop config");
		return new HexCoordinates(this.Q + dir.Q, this.R + dir.R, this.isFlatTop);
	}


	public HexCoordinates[] NeighbhouringCoordinates()
	{
		HexCoordinates[] toReturn = new HexCoordinates[6];
		for (int i = 0; i < toReturn.Length; i++)
		{
			toReturn[i] = this.Add(HexCoordinates.directionVectors[i]);
		}
		return toReturn;
	}

    public bool Equals(HexCoordinates other)
    {
        if (this.isFlatTop == other.isFlatTop && this.Q == other.Q && this.R == other.R)
        {
			return true;
        } else
        {
			return false;
        }

    }
	
	public static HexCoordinates Add(HexCoordinates h1, HexCoordinates h2)
    {
		if (h1.isFlatTop != h2.isFlatTop)
			throw new Exception("Attempting to add HexCoordinates with different flat top mode");

		return new HexCoordinates(h1.Q + h2.Q, h1.R + h2.R, h1.isFlatTop);    
	}

	public static HexCoordinates Substract(HexCoordinates h1, HexCoordinates h2)
	{
		if (h1.isFlatTop != h2.isFlatTop)
			throw new Exception("Attempting to add HexCoordinates with different flat top mode");

		return new HexCoordinates(h1.Q - h2.Q, h1.R - h2.R, h1.isFlatTop);
	}

    internal HexCoordinates Elongate(int toAdd)
    {
		if (this.OnSingleAxis())
			throw new Exception("elongate only supported along single axis");

		int newQ = this.Q > 0 ? this.Q + toAdd : this.Q - toAdd;
		int newR = this.R > 0 ? this.R + toAdd : this.R - toAdd;

		return new HexCoordinates(newQ, newR, this.isFlatTop);
	}

	public bool OnSingleAxis()
    {
		if (this.Q != 0 && this.R != 0 && this.S != 0)
			return false;
		else
			return true;
	}
}
