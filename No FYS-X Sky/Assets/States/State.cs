using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    private State currentState;

    public abstract void setCurrentState(State state);
}
