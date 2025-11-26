using System.Collections.Generic;

namespace GameConfig
{
    public partial class GuideConfigDB
    {
        private HashSet<string> _triggerModule = null;
        private List<int> _openWindowGuides;
        private List<int> _closeWindowGuides;
        public List<int> OpenWindowGuides => _openWindowGuides;
        public List<int> CloseWindowGuides => _closeWindowGuides;

        protected override void OnInitialized()
        {
            _openWindowGuides = new List<int>();
            _closeWindowGuides = new List<int>();
            _triggerModule = new HashSet<string>();
            for (int i = 0; i < All.Length; i++)
            {
                _triggerModule.Add(All[i].triggerModule);
                if (All[i].triggerModule == "OpenWindow")
                    _openWindowGuides.Add(All[i].id);
                if(All[i].triggerModule == "CloseWindow")
                    _closeWindowGuides.Add(All[i].id);
            }
        }

        public bool HasTriggerModule(string mvcName)
        {
            return _triggerModule.Contains(mvcName);
        }
        
        public void RefNextGuideList(int guideId, ref List<int> nextIds)
        {
            ref readonly GuideConfig config = ref this[guideId];
            if (config.nextId <= 0)
                return;
            if (nextIds.Contains(config.nextId))
                return;
            nextIds.Add(config.nextId);
            RefNextGuideList(config.nextId, ref nextIds);
        }

        public bool IsContain(int guideId)
        {
            return this._idToIdx.ContainsKey(guideId);
        }
        
        protected override void OnDispose()
        {
            _triggerModule?.Clear();
            _triggerModule = null;
            _openWindowGuides?.Clear();
            _openWindowGuides = null;
        }
    }
}