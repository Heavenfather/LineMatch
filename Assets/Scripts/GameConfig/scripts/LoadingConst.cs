/*
 * Generate by EnhanceExcel2Anything,don't modify it!
*/

namespace GameConfig
{
    
    public struct LoadingConst
    {
        public float startX { get; }
        public float startY { get; }
        public int vCount { get; }
        public int hCount { get; }
        public float size { get; }
        public string htmlColor { get; }
        public string imgName { get; }
        
        internal LoadingConst(float startX, float startY, int vCount, int hCount, float size, string htmlColor, string imgName)
        {
            this.startX = startX;
            this.startY = startY;
            this.vCount = vCount;
            this.hCount = hCount;
            this.size = size;
            this.htmlColor = htmlColor;
            this.imgName = imgName;
        }
    }
}