using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ColorPalette")]
public class ColorPaletteSO : ScriptableObject
{
    public Color HEX_DEFAULT_COLOR;
    public Color HEX_START_COLOR;
    public Color HEX_OPPONENT_START_COLOR;
    public Color HEX_HOVER_COLOR;
    public Color HEX_SELECT_COLOR;
    public Color HEX_IN_MOVE_RANGE_COLOR;
    public Color HEX_ATTACK_TARGETABLE_COLOR;
    public Color HEX_ABILITY_TARGETABLE_COLOR;
    public Color HEX_ATTACK_HOVER_COLOR;
    public Color HEX_IN_ATTACK_RANGE_COLOR;
    public Color HEX_IN_ABILITY_RANGE_COLOR;
    public Color HEX_OUT_OF_ACTION_RANGE_COLOR;
    public Color HEX_ABILITY_HOVER_COLOR;

    public Color OUTLINE_VALID_TARGET_COLOR;
    public Color OUTLINE_INVALID_TARGET_COLOR;
}
