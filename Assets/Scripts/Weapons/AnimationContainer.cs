using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Starlight/Weapon Animation Container"), System.Serializable]
public class AnimationContainer : ScriptableObject
{
    [System.Serializable]
    public struct AnimationClipPair
    {
        public string name;
        public AnimationClip clip;
    }
    public List<AnimationClipPair> clipList;
}



public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }
    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if(index != -1)
            {
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
            }
        }
    }
}
