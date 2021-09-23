using UnityEngine;

namespace Interfaces
{
    public abstract class Algorithm : ScriptableObject
    {
        public abstract Maze Generate(int width, int height);
    }
}