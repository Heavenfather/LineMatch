using System;
using UnityEngine;

namespace HotfixLogic.Match
{
    [Serializable]
    public class ColorLightReferenceParticleColor
    {
        public int ColorId;

        public ParticleSystem.MinMaxGradient _fang;
        
        public ParticleSystem.MinMaxGradient glow_a;
        
        public ParticleSystem.MinMaxGradient glow_g;
        
        public ParticleSystem.MinMaxGradient shikuai_b;
        
        public ParticleSystem.MinMaxGradient shikuai_s;
        
        public ParticleSystem.MinMaxGradient shikuai;
    }
    
    public class GridColorLightBlock : MonoBehaviour
    {
        [SerializeField]
        private ColorLightReferenceParticleColor[] _colorLightReferenceParticleColors;

        public ColorLightReferenceParticleColor GetConfigColor(int colorId)
        {
            for (int i = 0; i < _colorLightReferenceParticleColors.Length; i++)
            {
                if(_colorLightReferenceParticleColors[i].ColorId == colorId)
                    return _colorLightReferenceParticleColors[i];
            }
            return null;
        }
    }
}