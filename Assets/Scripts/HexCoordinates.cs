using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HexCoordinates {

	public static Vector3[] directionVectors = {
		new Vector3(1,0,-1),
		new Vector3(1,-1,0),
		new Vector3(0,-1,1),
		new Vector3(0,1,-1),
		new Vector3(-1,0,1),
		new Vector3(-1,1,0)
	};

	private bool isFlatTop;

	[SerializeField]
	//cube coordinates
	private int q, r;
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
	public int X { get { return this.isFlatTop ? this.Q : this.Q + (this.R - Math.Abs(this.R % 2)); } }
	public int Y { get { return this.isFlatTop ? this.R  + (this.Q - Math.Abs(this.Q%2)) : this.R; } }


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

    internal string ToStringOnLines()
    {
		return this.Q.ToString() + "\n" + this.R.ToString() + "\n" + S.ToString();
	}

	public HexCoordinates Add(Vector3 dir)
	{
		return new HexCoordinates(this.Q + (int)dir.x, this.R + (int)dir.y, this.isFlatTop);
	}

	public HexCoordinates[] Neighbours()
	{
		HexCoordinates[] toReturn = new HexCoordinates[6];
		for (int i = 0; i < toReturn.Length; i++)
		{
			toReturn[i] = this.Add(HexCoordinates.directionVectors[i]);
		}
		return toReturn;
	}
}
