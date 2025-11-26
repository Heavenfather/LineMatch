using System.Collections.Generic;

namespace GameConfig
{
    public partial class SpecialEleRuleDB
    {
        private List<SpecialEleRule> allRules = null;
        protected override void OnInitialized()
        {
            allRules = new List<SpecialEleRule>(All);
            allRules.Sort((a, b) =>
            {
                //priority越小越靠前
                if (a.priority < b.priority)
                    return -1;
                if (a.priority > b.priority)
                    return 1;
                return 0;
            });
        }

        public SpecialEleRule? Match(int width, int height)
        {
            foreach (var rule in allRules)
            {
                //检查宽度条件
                if(width < rule.minWidth) continue;
                if(rule.maxWidth > 0 && width > rule.maxWidth) continue;
                
                //检查高度条件
                if(height < rule.minHeight) continue;
                if(rule.maxHeight > 0 && height > rule.maxHeight) continue;

                return rule;
            }
            return null;
        }
        
        // 获取最终的火箭类型（处理默认或强制方向）
        public ElementType GetFinalRocketType(int width, int height, SpecialEleRule rule)
        {
            if (rule.resultElement == ElementType.Rocket)
            {
                // 根据几何形状自动判断
                if (width > height) return ElementType.RocketHorizontal;
                
                return ElementType.Rocket; 
            }
            return rule.resultElement;
        }

        protected override void OnDispose()
        {
            allRules.Clear();
            allRules = null;
        }
    }
}