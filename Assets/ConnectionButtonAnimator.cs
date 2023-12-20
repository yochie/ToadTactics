using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConnectionButtonAnimator : ButtonAnimator
{
    //Allows playing sound before button is disabled while attempting connection
    //relies on ordering of events in Button
    public void ForcePlayClickSound()
    {
        AudioManager.Singleton.PlaySoundEffect(this.buttonClickSound);
    }

    new public void OnPointerClick(PointerEventData eventData)
    {
        //do nothing, handled above
    }
}
