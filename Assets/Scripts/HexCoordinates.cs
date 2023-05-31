using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HexCoordinates {

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

	public HexCoordinates (int q, int r) {
		this.q = q;
		this.r = r;
	}

	public static HexCoordinates FromOffsetCoordinates(int x, int y, bool isFlatTop)
	{
        if (isFlatTop)
        {
			return new HexCoordinates(x, y - ((x - Math.Abs(x%2) ) / 2) );
		}
		else
        {
			return new HexCoordinates(x - ((y - Math.Abs(y % 2)) / 2), y);
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
}
