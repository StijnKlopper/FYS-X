using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    private State currentState;

    public abstract void setCurrentState(State state);
}
