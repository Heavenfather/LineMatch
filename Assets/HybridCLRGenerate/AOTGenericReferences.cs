using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"DOTween.dll",
		"GameCore.dll",
		"Newtonsoft.Json.dll",
		"System.Core.dll",
		"System.dll",
		"UniTask.dll",
		"UnityEngine.CoreModule.dll",
		"YooAsset.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<ElementObjectPool.<ClearAllPool>d__14>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<Hotfix.Logic.GameLaunch.<OpenLoginModule>d__3>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<Hotfix.Logic.GameLaunch.<PreLocalization>d__2>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<Hotfix.Logic.GameLaunch.<Preload>d__1>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<Hotfix.Logic.GameLaunch.<Start>d__0>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.MVC.MVCManager.<ActiveModule>d__6>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.MVC.MVCManager.<ExecuteModuleActivation>d__12>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.MVC.MVCManager.<ProcessActivationPolicy>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.MVC.MVCModule.<Activate>d__29>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.MVC.MVCModule.<ActiveMainWindow>d__35>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.MVC.MVCModule.<OpenChildWindow>d__32<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.MVC.MVCModule.<Sleep>d__30>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.GameItemModule.<GetItemSprite>d__22,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.GameItemModule.<GetItemSprite>d__23,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.HttpModule.<GetWebRequest>d__19>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.HttpModule.<PostWebRequest>d__18>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.ResourceHandler.<TryWaitingLoading>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__34<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.ResourceModule.<LoadGameObjectAsync>d__35,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.ResourceModule.<TryWaitingLoading>d__40>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.ResourceModule.<UnloadAllUnusedAssets>d__24>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.UIBase.<CreateWidgetByPathAsync>d__57<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.UIBase.<CreateWidgetByTypeAsync>d__59<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.UIModule.<GetUIAsyncAwait>d__43<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__26<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__27,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixCore.Module.UIModule.<ShowUIAwaitImp>d__29,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.CollectElementItem.<<DoDestroy>b__3_0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.ElementBase.<PlayBrokenEff>d__16>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.GridItem.<CreateSelf>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.GridSystem.<>c__DisplayClass22_0.<<OnMatchRemainStep>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.GridSystem.<DoDelElement>d__55>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.GridSystem.<DoRocketDelElement>d__56>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.GridSystem.<OnDestroyTargetListElement>d__54>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.GridSystem.<StartDropElement>d__60>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.GridSystem.<StartMatch>d__26>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.LevelManager.<GetLevel>d__3,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.LevelManager.<GetLevel>d__4,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.MatchManager.<GetMatchElementSprite>d__22,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.RockElementItem.<>c__DisplayClass1_0.<<DoDestroy>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.ValidateLine.<CheckSpecialElement>d__3,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Match.ValidateSquare.<CheckSpecialElement>d__7>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.MatchBeginPanel.<SetLevel>d__16>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.MatchController.<<OnReduceMoveStep>b__19_0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.MatchData.<OnInitialized>d__12>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.MatchResultLose.<SetLevel>d__37>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.MatchResultWin.<UpdateGoals>d__28>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.AnimItem.<UpdateAnim>d__15>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.AnimItem.<UpdateSprite>d__16>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.PuzzleBG.<LoadBG>d__7>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.PuzzleGame.<GetAllCollectItems>d__37,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.PuzzleGame.<GetAllItemsSprite>d__38,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.PuzzleGame.<InitPSDPosData>d__31>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.PuzzleGame.<LoadAllCollectItems>d__36>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.PuzzleGame.<LoadBackGround>d__32>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.PuzzleGame.<LoadCollectItem>d__35>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.PuzzleGame.<LoadPSDData>d__33>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.PuzzleManager.<LoadMapData>d__58>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.Puzzle.PuzzleManager.<LoadPuzzleLevelData>d__57>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.PuzzleWindow.<LoadBoosterButton>d__19>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.PuzzleWindow.<LoadCollectTarget>d__18>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<HotfixLogic.PuzzleWindow.<LoadPuzzleReward>d__20>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<ElementObjectPool.<ClearAllPool>d__14>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<Hotfix.Logic.GameLaunch.<OpenLoginModule>d__3>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<Hotfix.Logic.GameLaunch.<PreLocalization>d__2>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<Hotfix.Logic.GameLaunch.<Preload>d__1>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<Hotfix.Logic.GameLaunch.<Start>d__0>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.MVC.MVCManager.<ActiveModule>d__6>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.MVC.MVCManager.<ExecuteModuleActivation>d__12>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.MVC.MVCManager.<ProcessActivationPolicy>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.MVC.MVCModule.<Activate>d__29>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.MVC.MVCModule.<ActiveMainWindow>d__35>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.MVC.MVCModule.<OpenChildWindow>d__32<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.MVC.MVCModule.<Sleep>d__30>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.GameItemModule.<GetItemSprite>d__22,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.GameItemModule.<GetItemSprite>d__23,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.HttpModule.<GetWebRequest>d__19>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.HttpModule.<PostWebRequest>d__18>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.ResourceHandler.<TryWaitingLoading>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__34<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.ResourceModule.<LoadGameObjectAsync>d__35,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.ResourceModule.<TryWaitingLoading>d__40>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.ResourceModule.<UnloadAllUnusedAssets>d__24>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.UIBase.<CreateWidgetByPathAsync>d__57<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.UIBase.<CreateWidgetByTypeAsync>d__59<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.UIModule.<GetUIAsyncAwait>d__43<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__26<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__27,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixCore.Module.UIModule.<ShowUIAwaitImp>d__29,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.CollectElementItem.<<DoDestroy>b__3_0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.ElementBase.<PlayBrokenEff>d__16>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.GridItem.<CreateSelf>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.GridSystem.<>c__DisplayClass22_0.<<OnMatchRemainStep>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.GridSystem.<DoDelElement>d__55>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.GridSystem.<DoRocketDelElement>d__56>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.GridSystem.<OnDestroyTargetListElement>d__54>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.GridSystem.<StartDropElement>d__60>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.GridSystem.<StartMatch>d__26>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.LevelManager.<GetLevel>d__3,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.LevelManager.<GetLevel>d__4,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.MatchManager.<GetMatchElementSprite>d__22,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.RockElementItem.<>c__DisplayClass1_0.<<DoDestroy>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.ValidateLine.<CheckSpecialElement>d__3,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Match.ValidateSquare.<CheckSpecialElement>d__7>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.MatchBeginPanel.<SetLevel>d__16>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.MatchController.<<OnReduceMoveStep>b__19_0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.MatchData.<OnInitialized>d__12>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.MatchResultLose.<SetLevel>d__37>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.MatchResultWin.<UpdateGoals>d__28>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.AnimItem.<UpdateAnim>d__15>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.AnimItem.<UpdateSprite>d__16>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.PuzzleBG.<LoadBG>d__7>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.PuzzleGame.<GetAllCollectItems>d__37,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.PuzzleGame.<GetAllItemsSprite>d__38,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.PuzzleGame.<InitPSDPosData>d__31>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.PuzzleGame.<LoadAllCollectItems>d__36>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.PuzzleGame.<LoadBackGround>d__32>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.PuzzleGame.<LoadCollectItem>d__35>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.PuzzleGame.<LoadPSDData>d__33>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.PuzzleManager.<LoadMapData>d__58>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.Puzzle.PuzzleManager.<LoadPuzzleLevelData>d__57>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.PuzzleWindow.<LoadBoosterButton>d__19>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.PuzzleWindow.<LoadCollectTarget>d__18>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<HotfixLogic.PuzzleWindow.<LoadPuzzleReward>d__20>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<HotfixCore.Module.ResourceHandler.<SetAssetByResources>d__16<object>>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<HotfixCore.Module.ResourceModule.<InvokeProgress>d__37>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__33<object>>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<HotfixCore.Module.SceneModule.<InvokeProgress>d__13>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<HotfixCore.Module.UIModule.<>c__DisplayClass44_0.<<GetUIAsync>g__GetUIAsyncImp|0>d<object>>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<HotfixCore.Module.UIWindow.<InternalLoad>d__65>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<HotfixCore.Utils.UnityUtil.<AddFixedUpdateListenerImp>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<HotfixCore.Utils.UnityUtil.<AddLateUpdateListenerImp>d__13>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<HotfixCore.Utils.UnityUtil.<AddUpdateListenerImp>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<HotfixCore.Module.ResourceHandler.<SetAssetByResources>d__16<object>>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<HotfixCore.Module.ResourceModule.<InvokeProgress>d__37>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__33<object>>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<HotfixCore.Module.SceneModule.<InvokeProgress>d__13>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<HotfixCore.Module.UIModule.<>c__DisplayClass44_0.<<GetUIAsync>g__GetUIAsyncImp|0>d<object>>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<HotfixCore.Module.UIWindow.<InternalLoad>d__65>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<HotfixCore.Utils.UnityUtil.<AddFixedUpdateListenerImp>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<HotfixCore.Utils.UnityUtil.<AddLateUpdateListenerImp>d__13>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<HotfixCore.Utils.UnityUtil.<AddUpdateListenerImp>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<byte>
	// Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<object>
	// Cysharp.Threading.Tasks.ITaskPoolNode<object>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.UIntPtr>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.UIntPtr>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.IUniTaskSource<byte>
	// Cysharp.Threading.Tasks.IUniTaskSource<object>
	// Cysharp.Threading.Tasks.Internal.ArrayPool<Cysharp.Threading.Tasks.UniTask<object>>
	// Cysharp.Threading.Tasks.Internal.ArrayPoolUtil.RentArray<Cysharp.Threading.Tasks.UniTask<object>>
	// Cysharp.Threading.Tasks.Internal.MinimumQueue<System.UIntPtr>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.UIntPtr>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.UIntPtr>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<byte>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<object>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.UIntPtr>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.UIntPtr>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<byte>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<object>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.UIntPtr>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.UIntPtr>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<byte>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<object>
	// Cysharp.Threading.Tasks.UniTask.WhenAllPromise.<>c<object>
	// Cysharp.Threading.Tasks.UniTask.WhenAllPromise<object>
	// Cysharp.Threading.Tasks.UniTask<System.UIntPtr>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.UIntPtr>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask<byte>
	// Cysharp.Threading.Tasks.UniTask<object>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<Cysharp.Threading.Tasks.AsyncUnit>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<System.UIntPtr>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<byte>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<object>
	// DG.Tweening.Core.DOSetter<float>
	// GameCore.LRU.AutoLru.CacheItem<object,object>
	// GameCore.LRU.AutoLru<object,object>
	// GameCore.Singleton.LazySingleton<object>
	// GameCore.Singleton.MonoSingleton<object>
	// System.Action<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Action<GameConfig.CoinShopItems>
	// System.Action<Hotfix.EventParameter.EventLocalizationChanged>
	// System.Action<Hotfix.EventParameter.EventOneParam<byte>>
	// System.Action<Hotfix.EventParameter.EventOneParam<int>>
	// System.Action<Hotfix.EventParameter.EventOneParam<object>>
	// System.Action<Hotfix.EventParameter.EventThreeParam<byte,int,int>>
	// System.Action<Hotfix.EventParameter.EventThreeParam<int,int,object>>
	// System.Action<Hotfix.EventParameter.EventThreeParam<object,byte,int>>
	// System.Action<Hotfix.EventParameter.EventTwoParam<UnityEngine.Vector2,UnityEngine.Vector2>>
	// System.Action<Hotfix.EventParameter.EventTwoParam<int,UnityEngine.Vector3>>
	// System.Action<Hotfix.EventParameter.EventTwoParam<int,object>>
	// System.Action<Hotfix.EventParameter.EventTwoParam<object,int>>
	// System.Action<HotfixCore.Module.AssetsRefInfo>
	// System.Action<HotfixCore.Module.ObjectInfo>
	// System.Action<HotfixLogic.Match.GridHoldInfo>
	// System.Action<HotfixLogic.Match.LevelDropElementInfo>
	// System.Action<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Action<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Action<UnityEngine.EventSystems.RaycastResult>
	// System.Action<UnityEngine.Rect>
	// System.Action<UnityEngine.SceneManagement.Scene>
	// System.Action<UnityEngine.Vector2>
	// System.Action<UnityEngine.Vector2Int>
	// System.Action<UnityEngine.Vector3>
	// System.Action<byte>
	// System.Action<float>
	// System.Action<int>
	// System.Action<object,int>
	// System.Action<object,object,object>
	// System.Action<object,object>
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.ArraySortHelper<GameConfig.CoinShopItems>
	// System.Collections.Generic.ArraySortHelper<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.Generic.ArraySortHelper<HotfixCore.Module.ObjectInfo>
	// System.Collections.Generic.ArraySortHelper<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.ArraySortHelper<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Rect>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector2>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector2Int>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector3>
	// System.Collections.Generic.ArraySortHelper<float>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.Comparer<GameConfig.CoinShopItems>
	// System.Collections.Generic.Comparer<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.Generic.Comparer<HotfixCore.Module.ObjectInfo>
	// System.Collections.Generic.Comparer<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.Comparer<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Comparer<System.UIntPtr>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.UIntPtr>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.Comparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.Comparer<UnityEngine.Rect>
	// System.Collections.Generic.Comparer<UnityEngine.Vector2>
	// System.Collections.Generic.Comparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.Comparer<UnityEngine.Vector3>
	// System.Collections.Generic.Comparer<byte>
	// System.Collections.Generic.Comparer<float>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.ComparisonComparer<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.ComparisonComparer<GameConfig.CoinShopItems>
	// System.Collections.Generic.ComparisonComparer<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.Generic.ComparisonComparer<HotfixCore.Module.ObjectInfo>
	// System.Collections.Generic.ComparisonComparer<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.ComparisonComparer<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.Generic.ComparisonComparer<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.ComparisonComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ComparisonComparer<System.UIntPtr>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.UIntPtr>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.Rect>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ComparisonComparer<byte>
	// System.Collections.Generic.ComparisonComparer<float>
	// System.Collections.Generic.ComparisonComparer<int>
	// System.Collections.Generic.ComparisonComparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<HotfixCore.DataStruck.TypeNamePair,object>
	// System.Collections.Generic.Dictionary.Enumerator<HotfixLogic.Match.GridHoldInfo,object>
	// System.Collections.Generic.Dictionary.Enumerator<UnityEngine.Vector2,object>
	// System.Collections.Generic.Dictionary.Enumerator<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,UnityEngine.Color>
	// System.Collections.Generic.Dictionary.Enumerator<int,UnityEngine.Vector2Int>
	// System.Collections.Generic.Dictionary.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>
	// System.Collections.Generic.Dictionary.Enumerator<object,System.ValueTuple<object,UnityEngine.Rect>>
	// System.Collections.Generic.Dictionary.Enumerator<object,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<HotfixCore.DataStruck.TypeNamePair,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<HotfixLogic.Match.GridHoldInfo,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<UnityEngine.Vector2,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,UnityEngine.Color>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,UnityEngine.Vector2Int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,System.ValueTuple<object,UnityEngine.Rect>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<HotfixCore.DataStruck.TypeNamePair,object>
	// System.Collections.Generic.Dictionary.KeyCollection<HotfixLogic.Match.GridHoldInfo,object>
	// System.Collections.Generic.Dictionary.KeyCollection<UnityEngine.Vector2,object>
	// System.Collections.Generic.Dictionary.KeyCollection<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,UnityEngine.Color>
	// System.Collections.Generic.Dictionary.KeyCollection<int,UnityEngine.Vector2Int>
	// System.Collections.Generic.Dictionary.KeyCollection<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>
	// System.Collections.Generic.Dictionary.KeyCollection<object,System.ValueTuple<object,UnityEngine.Rect>>
	// System.Collections.Generic.Dictionary.KeyCollection<object,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.KeyCollection<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<HotfixCore.DataStruck.TypeNamePair,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<HotfixLogic.Match.GridHoldInfo,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<UnityEngine.Vector2,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,UnityEngine.Color>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,UnityEngine.Vector2Int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,System.ValueTuple<object,UnityEngine.Rect>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<HotfixCore.DataStruck.TypeNamePair,object>
	// System.Collections.Generic.Dictionary.ValueCollection<HotfixLogic.Match.GridHoldInfo,object>
	// System.Collections.Generic.Dictionary.ValueCollection<UnityEngine.Vector2,object>
	// System.Collections.Generic.Dictionary.ValueCollection<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,UnityEngine.Color>
	// System.Collections.Generic.Dictionary.ValueCollection<int,UnityEngine.Vector2Int>
	// System.Collections.Generic.Dictionary.ValueCollection<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>
	// System.Collections.Generic.Dictionary.ValueCollection<object,System.ValueTuple<object,UnityEngine.Rect>>
	// System.Collections.Generic.Dictionary.ValueCollection<object,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.ValueCollection<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<HotfixCore.DataStruck.TypeNamePair,object>
	// System.Collections.Generic.Dictionary<HotfixLogic.Match.GridHoldInfo,object>
	// System.Collections.Generic.Dictionary<UnityEngine.Vector2,object>
	// System.Collections.Generic.Dictionary<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.Dictionary<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.Dictionary<int,UnityEngine.Color>
	// System.Collections.Generic.Dictionary<int,UnityEngine.Vector2Int>
	// System.Collections.Generic.Dictionary<int,int>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>
	// System.Collections.Generic.Dictionary<object,System.ValueTuple<object,UnityEngine.Rect>>
	// System.Collections.Generic.Dictionary<object,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>
	// System.Collections.Generic.EqualityComparer<HotfixCore.DataStruck.TypeNamePair>
	// System.Collections.Generic.EqualityComparer<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.EqualityComparer<System.UIntPtr>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.UIntPtr>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<object,UnityEngine.Rect>>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Color>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Rect>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector2>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.HashSet.Enumerator<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.HashSet.Enumerator<UnityEngine.Vector2Int>
	// System.Collections.Generic.HashSet.Enumerator<int>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.HashSet<UnityEngine.Vector2Int>
	// System.Collections.Generic.HashSet<int>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSetEqualityComparer<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.HashSetEqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.HashSetEqualityComparer<int>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.ICollection<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.ICollection<GameConfig.CoinShopItems>
	// System.Collections.Generic.ICollection<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.Generic.ICollection<HotfixCore.Module.ObjectInfo>
	// System.Collections.Generic.ICollection<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.ICollection<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<HotfixCore.DataStruck.TypeNamePair,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<HotfixLogic.Match.GridHoldInfo,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,UnityEngine.Color>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2Int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,System.ValueTuple<object,UnityEngine.Rect>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,UnityEngine.Vector2>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ICollection<UnityEngine.Rect>
	// System.Collections.Generic.ICollection<UnityEngine.Vector2>
	// System.Collections.Generic.ICollection<UnityEngine.Vector2Int>
	// System.Collections.Generic.ICollection<UnityEngine.Vector3>
	// System.Collections.Generic.ICollection<float>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.IComparer<GameConfig.CoinShopItems>
	// System.Collections.Generic.IComparer<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.Generic.IComparer<HotfixCore.Module.ObjectInfo>
	// System.Collections.Generic.IComparer<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.IComparer<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IComparer<UnityEngine.Rect>
	// System.Collections.Generic.IComparer<UnityEngine.Vector2>
	// System.Collections.Generic.IComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.IComparer<UnityEngine.Vector3>
	// System.Collections.Generic.IComparer<float>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IDictionary<object,GameCore.LitJson.ArrayMetadata>
	// System.Collections.Generic.IDictionary<object,GameCore.LitJson.ObjectMetadata>
	// System.Collections.Generic.IDictionary<object,GameCore.LitJson.PropertyMetadata>
	// System.Collections.Generic.IDictionary<object,object>
	// System.Collections.Generic.IEnumerable<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.IEnumerable<GameConfig.CoinShopItems>
	// System.Collections.Generic.IEnumerable<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.Generic.IEnumerable<HotfixCore.Module.ObjectInfo>
	// System.Collections.Generic.IEnumerable<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.IEnumerable<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<HotfixCore.DataStruck.TypeNamePair,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<HotfixLogic.Match.GridHoldInfo,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,UnityEngine.Color>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2Int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,System.ValueTuple<object,UnityEngine.Rect>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,UnityEngine.Vector2>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IEnumerable<UnityEngine.Rect>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector2>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector3>
	// System.Collections.Generic.IEnumerable<float>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.IEnumerator<GameConfig.CoinShopItems>
	// System.Collections.Generic.IEnumerator<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.Generic.IEnumerator<HotfixCore.Module.ObjectInfo>
	// System.Collections.Generic.IEnumerator<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.IEnumerator<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<HotfixCore.DataStruck.TypeNamePair,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<HotfixLogic.Match.GridHoldInfo,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,UnityEngine.Color>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2Int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,System.ValueTuple<object,UnityEngine.Rect>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,UnityEngine.Vector2>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IEnumerator<UnityEngine.Rect>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector2>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector3>
	// System.Collections.Generic.IEnumerator<float>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<HotfixCore.DataStruck.TypeNamePair>
	// System.Collections.Generic.IEqualityComparer<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.IEqualityComparer<UnityEngine.Vector2>
	// System.Collections.Generic.IEqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.IList<GameConfig.CoinShopItems>
	// System.Collections.Generic.IList<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.Generic.IList<HotfixCore.Module.ObjectInfo>
	// System.Collections.Generic.IList<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.IList<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IList<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IList<UnityEngine.Rect>
	// System.Collections.Generic.IList<UnityEngine.Vector2>
	// System.Collections.Generic.IList<UnityEngine.Vector2Int>
	// System.Collections.Generic.IList<UnityEngine.Vector3>
	// System.Collections.Generic.IList<float>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.IReadOnlyCollection<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.IReadOnlyDictionary<int,int>
	// System.Collections.Generic.KeyValuePair<HotfixCore.DataStruck.TypeNamePair,object>
	// System.Collections.Generic.KeyValuePair<HotfixLogic.Match.GridHoldInfo,object>
	// System.Collections.Generic.KeyValuePair<UnityEngine.Vector2,object>
	// System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,int>
	// System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>
	// System.Collections.Generic.KeyValuePair<int,UnityEngine.Color>
	// System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2Int>
	// System.Collections.Generic.KeyValuePair<int,int>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<object,HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>
	// System.Collections.Generic.KeyValuePair<object,System.ValueTuple<object,UnityEngine.Rect>>
	// System.Collections.Generic.KeyValuePair<object,UnityEngine.Vector2>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.LinkedList.Enumerator<object>
	// System.Collections.Generic.LinkedList<object>
	// System.Collections.Generic.LinkedListNode<object>
	// System.Collections.Generic.List.Enumerator<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.List.Enumerator<GameConfig.CoinShopItems>
	// System.Collections.Generic.List.Enumerator<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.Generic.List.Enumerator<HotfixCore.Module.ObjectInfo>
	// System.Collections.Generic.List.Enumerator<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.List.Enumerator<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List.Enumerator<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Rect>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector2>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector2Int>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector3>
	// System.Collections.Generic.List.Enumerator<float>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.List<GameConfig.CoinShopItems>
	// System.Collections.Generic.List<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.Generic.List<HotfixCore.Module.ObjectInfo>
	// System.Collections.Generic.List<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.List<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.List<UnityEngine.Rect>
	// System.Collections.Generic.List<UnityEngine.Vector2>
	// System.Collections.Generic.List<UnityEngine.Vector2Int>
	// System.Collections.Generic.List<UnityEngine.Vector3>
	// System.Collections.Generic.List<float>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.Generic.ObjectComparer<GameConfig.CoinShopItems>
	// System.Collections.Generic.ObjectComparer<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.Generic.ObjectComparer<HotfixCore.Module.ObjectInfo>
	// System.Collections.Generic.ObjectComparer<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.ObjectComparer<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ObjectComparer<System.UIntPtr>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.UIntPtr>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.ObjectComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Rect>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ObjectComparer<byte>
	// System.Collections.Generic.ObjectComparer<float>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<HotfixCore.DataStruck.GameFrameworkLinkedListRange<object>>
	// System.Collections.Generic.ObjectEqualityComparer<HotfixCore.DataStruck.TypeNamePair>
	// System.Collections.Generic.ObjectEqualityComparer<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.Generic.ObjectEqualityComparer<System.UIntPtr>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.UIntPtr>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<object,UnityEngine.Rect>>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Color>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Rect>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector2Int>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.Queue.Enumerator<UnityEngine.Vector2Int>
	// System.Collections.Generic.Queue.Enumerator<int>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue<UnityEngine.Vector2Int>
	// System.Collections.Generic.Queue<int>
	// System.Collections.Generic.Queue<object>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<GameConfig.CoinShopItems>
	// System.Collections.ObjectModel.ReadOnlyCollection<HotfixCore.Module.AssetsRefInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<HotfixCore.Module.ObjectInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<HotfixLogic.Match.GridHoldInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<HotfixLogic.Match.LevelDropElementInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Rect>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector2>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector2Int>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector3>
	// System.Collections.ObjectModel.ReadOnlyCollection<float>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Comparison<GameConfig.CoinShopItems>
	// System.Comparison<HotfixCore.Module.AssetsRefInfo>
	// System.Comparison<HotfixCore.Module.ObjectInfo>
	// System.Comparison<HotfixLogic.Match.GridHoldInfo>
	// System.Comparison<HotfixLogic.Match.LevelDropElementInfo>
	// System.Comparison<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Comparison<System.UIntPtr>
	// System.Comparison<System.ValueTuple<byte,System.UIntPtr>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Comparison<System.ValueTuple<byte,byte>>
	// System.Comparison<System.ValueTuple<byte,object>>
	// System.Comparison<UnityEngine.EventSystems.RaycastResult>
	// System.Comparison<UnityEngine.Rect>
	// System.Comparison<UnityEngine.Vector2>
	// System.Comparison<UnityEngine.Vector2Int>
	// System.Comparison<UnityEngine.Vector3>
	// System.Comparison<byte>
	// System.Comparison<float>
	// System.Comparison<int>
	// System.Comparison<object>
	// System.Func<Cysharp.Threading.Tasks.UniTask>
	// System.Func<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<object,object>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<object,object>,int>
	// System.Func<UnityEngine.EventSystems.RaycastResult,byte>
	// System.Func<UnityEngine.Vector2Int,byte>
	// System.Func<UnityEngine.Vector2Int,float>
	// System.Func<UnityEngine.Vector2Int,int>
	// System.Func<byte>
	// System.Func<int,UnityEngine.Vector2>
	// System.Func<int,byte>
	// System.Func<int,object>
	// System.Func<int>
	// System.Func<object,byte>
	// System.Func<object,int>
	// System.Func<object>
	// System.IEquatable<HotfixCore.DataStruck.TypeNamePair>
	// System.IEquatable<HotfixLogic.Match.GridHoldInfo>
	// System.Linq.Buffer<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Linq.Buffer<UnityEngine.Vector2Int>
	// System.Linq.Buffer<object>
	// System.Linq.Enumerable.<TakeIterator>d__25<UnityEngine.Vector2Int>
	// System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Linq.Enumerable.Iterator<UnityEngine.Vector2Int>
	// System.Linq.Enumerable.Iterator<int>
	// System.Linq.Enumerable.Iterator<object>
	// System.Linq.Enumerable.WhereArrayIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<int>
	// System.Linq.Enumerable.WhereEnumerableIterator<object>
	// System.Linq.Enumerable.WhereListIterator<object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<System.Collections.Generic.KeyValuePair<object,object>,int>
	// System.Linq.Enumerable.WhereSelectArrayIterator<UnityEngine.Vector2Int,int>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,int>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<System.Collections.Generic.KeyValuePair<object,object>,int>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<UnityEngine.Vector2Int,int>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,int>
	// System.Linq.Enumerable.WhereSelectListIterator<System.Collections.Generic.KeyValuePair<object,object>,int>
	// System.Linq.Enumerable.WhereSelectListIterator<UnityEngine.Vector2Int,int>
	// System.Linq.Enumerable.WhereSelectListIterator<object,int>
	// System.Linq.EnumerableSorter<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>,int>
	// System.Linq.EnumerableSorter<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Linq.EnumerableSorter<UnityEngine.Vector2Int,float>
	// System.Linq.EnumerableSorter<UnityEngine.Vector2Int,int>
	// System.Linq.EnumerableSorter<UnityEngine.Vector2Int>
	// System.Linq.IOrderedEnumerable<UnityEngine.Vector2Int>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<UnityEngine.Vector2Int>
	// System.Linq.OrderedEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>,int>
	// System.Linq.OrderedEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Linq.OrderedEnumerable<UnityEngine.Vector2Int,float>
	// System.Linq.OrderedEnumerable<UnityEngine.Vector2Int,int>
	// System.Linq.OrderedEnumerable<UnityEngine.Vector2Int>
	// System.Predicate<Cysharp.Threading.Tasks.UniTask<object>>
	// System.Predicate<GameConfig.CoinShopItems>
	// System.Predicate<HotfixCore.Module.AssetsRefInfo>
	// System.Predicate<HotfixCore.Module.ObjectInfo>
	// System.Predicate<HotfixLogic.Match.GridHoldInfo>
	// System.Predicate<HotfixLogic.Match.LevelDropElementInfo>
	// System.Predicate<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Predicate<UnityEngine.EventSystems.RaycastResult>
	// System.Predicate<UnityEngine.Rect>
	// System.Predicate<UnityEngine.Vector2>
	// System.Predicate<UnityEngine.Vector2Int>
	// System.Predicate<UnityEngine.Vector3>
	// System.Predicate<float>
	// System.Predicate<int>
	// System.Predicate<object>
	// System.Tuple<double,int,object>
	// System.ValueTuple<UnityEngine.Vector2,UnityEngine.Vector2>
	// System.ValueTuple<byte,System.UIntPtr>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.UIntPtr>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,byte>>
	// System.ValueTuple<byte,System.ValueTuple<byte,object>>
	// System.ValueTuple<byte,byte>
	// System.ValueTuple<byte,object>
	// System.ValueTuple<object,UnityEngine.Rect>
	// System.ValueTuple<object,object>
	// UnityEngine.Events.InvokableCall<UnityEngine.Vector2>
	// UnityEngine.Events.InvokableCall<int>
	// UnityEngine.Events.InvokableCall<object>
	// UnityEngine.Events.UnityAction<UnityEngine.Vector2>
	// UnityEngine.Events.UnityAction<byte>
	// UnityEngine.Events.UnityAction<int>
	// UnityEngine.Events.UnityAction<object>
	// UnityEngine.Events.UnityEvent<UnityEngine.Vector2>
	// UnityEngine.Events.UnityEvent<int>
	// UnityEngine.Events.UnityEvent<object>
	// UnityEngine.Pool.CollectionPool.<>c<object,Cysharp.Threading.Tasks.UniTask<object>>
	// UnityEngine.Pool.CollectionPool<object,Cysharp.Threading.Tasks.UniTask<object>>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Hotfix.Logic.GameLaunch.<OpenLoginModule>d__3>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Hotfix.Logic.GameLaunch.<OpenLoginModule>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Hotfix.Logic.GameLaunch.<PreLocalization>d__2>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Hotfix.Logic.GameLaunch.<PreLocalization>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Hotfix.Logic.GameLaunch.<Preload>d__1>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Hotfix.Logic.GameLaunch.<Preload>d__1&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Hotfix.Logic.GameLaunch.<Start>d__0>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Hotfix.Logic.GameLaunch.<Start>d__0&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.MVC.MVCManager.<ActiveModule>d__6>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.MVC.MVCManager.<ActiveModule>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.MVC.MVCManager.<ExecuteModuleActivation>d__12>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.MVC.MVCManager.<ExecuteModuleActivation>d__12&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.MVC.MVCManager.<ProcessActivationPolicy>d__11>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.MVC.MVCManager.<ProcessActivationPolicy>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.MVC.MVCModule.<Activate>d__29>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.MVC.MVCModule.<Activate>d__29&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.MVC.MVCModule.<ActiveMainWindow>d__35>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.MVC.MVCModule.<ActiveMainWindow>d__35&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.Module.ResourceHandler.<TryWaitingLoading>d__11>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.Module.ResourceHandler.<TryWaitingLoading>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.Module.ResourceModule.<TryWaitingLoading>d__40>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.Module.ResourceModule.<TryWaitingLoading>d__40&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.Module.ResourceModule.<UnloadAllUnusedAssets>d__24>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.Module.ResourceModule.<UnloadAllUnusedAssets>d__24&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.CollectElementItem.<<DoDestroy>b__3_0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.CollectElementItem.<<DoDestroy>b__3_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.ElementBase.<PlayBrokenEff>d__16>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.ElementBase.<PlayBrokenEff>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.GridSystem.<>c__DisplayClass22_0.<<OnMatchRemainStep>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.GridSystem.<>c__DisplayClass22_0.<<OnMatchRemainStep>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.GridSystem.<DoDelElement>d__55>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.GridSystem.<DoDelElement>d__55&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.GridSystem.<DoRocketDelElement>d__56>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.GridSystem.<DoRocketDelElement>d__56&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.GridSystem.<OnDestroyTargetListElement>d__54>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.GridSystem.<OnDestroyTargetListElement>d__54&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.GridSystem.<StartDropElement>d__60>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.GridSystem.<StartDropElement>d__60&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.GridSystem.<StartMatch>d__26>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.GridSystem.<StartMatch>d__26&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.RockElementItem.<>c__DisplayClass1_0.<<DoDestroy>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.RockElementItem.<>c__DisplayClass1_0.<<DoDestroy>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.ValidateSquare.<CheckSpecialElement>d__7>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.ValidateSquare.<CheckSpecialElement>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Puzzle.PuzzleGame.<InitPSDPosData>d__31>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Puzzle.PuzzleGame.<InitPSDPosData>d__31&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Puzzle.PuzzleManager.<LoadPuzzleLevelData>d__57>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Puzzle.PuzzleManager.<LoadPuzzleLevelData>d__57&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixCore.MVC.MVCModule.<ActiveMainWindow>d__35>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixCore.MVC.MVCModule.<ActiveMainWindow>d__35&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixCore.Module.HttpModule.<GetWebRequest>d__19>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixCore.Module.HttpModule.<GetWebRequest>d__19&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixCore.Module.HttpModule.<PostWebRequest>d__18>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixCore.Module.HttpModule.<PostWebRequest>d__18&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Match.GridItem.<CreateSelf>d__9>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Match.GridItem.<CreateSelf>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.MatchBeginPanel.<SetLevel>d__16>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.MatchBeginPanel.<SetLevel>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.MatchController.<<OnReduceMoveStep>b__19_0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.MatchController.<<OnReduceMoveStep>b__19_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.MatchData.<OnInitialized>d__12>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.MatchData.<OnInitialized>d__12&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.MatchResultLose.<SetLevel>d__37>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.MatchResultLose.<SetLevel>d__37&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.MatchResultWin.<UpdateGoals>d__28>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.MatchResultWin.<UpdateGoals>d__28&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.AnimItem.<UpdateAnim>d__15>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.AnimItem.<UpdateAnim>d__15&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.AnimItem.<UpdateSprite>d__16>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.AnimItem.<UpdateSprite>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.PuzzleBG.<LoadBG>d__7>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.PuzzleBG.<LoadBG>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.PuzzleGame.<InitPSDPosData>d__31>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.PuzzleGame.<InitPSDPosData>d__31&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.PuzzleGame.<LoadAllCollectItems>d__36>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.PuzzleGame.<LoadAllCollectItems>d__36&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.PuzzleGame.<LoadBackGround>d__32>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.PuzzleGame.<LoadBackGround>d__32&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.PuzzleGame.<LoadCollectItem>d__35>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.PuzzleGame.<LoadCollectItem>d__35&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.PuzzleGame.<LoadPSDData>d__33>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.PuzzleGame.<LoadPSDData>d__33&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.PuzzleManager.<LoadMapData>d__58>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.PuzzleManager.<LoadMapData>d__58&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.PuzzleWindow.<LoadBoosterButton>d__19>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.PuzzleWindow.<LoadBoosterButton>d__19&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.PuzzleWindow.<LoadCollectTarget>d__18>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.PuzzleWindow.<LoadCollectTarget>d__18&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.PuzzleWindow.<LoadPuzzleReward>d__20>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.PuzzleWindow.<LoadPuzzleReward>d__20&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,ElementObjectPool.<ClearAllPool>d__14>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,ElementObjectPool.<ClearAllPool>d__14&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.MVC.MVCModule.<Sleep>d__30>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.MVC.MVCModule.<Sleep>d__30&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.ValidateLine.<CheckSpecialElement>d__3>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.ValidateLine.<CheckSpecialElement>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__34<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__34<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.Module.ResourceModule.<LoadGameObjectAsync>d__35>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.Module.ResourceModule.<LoadGameObjectAsync>d__35&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.LevelManager.<GetLevel>d__3>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.LevelManager.<GetLevel>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.LevelManager.<GetLevel>d__4>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.LevelManager.<GetLevel>d__4&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__34<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__34<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,HotfixCore.Module.ResourceModule.<LoadGameObjectAsync>d__35>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,HotfixCore.Module.ResourceModule.<LoadGameObjectAsync>d__35&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixCore.MVC.MVCModule.<OpenChildWindow>d__32<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixCore.MVC.MVCModule.<OpenChildWindow>d__32<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixCore.Module.GameItemModule.<GetItemSprite>d__22>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixCore.Module.GameItemModule.<GetItemSprite>d__22&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixCore.Module.GameItemModule.<GetItemSprite>d__23>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixCore.Module.GameItemModule.<GetItemSprite>d__23&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixCore.Module.UIBase.<CreateWidgetByPathAsync>d__57<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixCore.Module.UIBase.<CreateWidgetByPathAsync>d__57<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixCore.Module.UIBase.<CreateWidgetByTypeAsync>d__59<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixCore.Module.UIBase.<CreateWidgetByTypeAsync>d__59<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__26<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__26<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__27>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__27&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Match.MatchManager.<GetMatchElementSprite>d__22>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Match.MatchManager.<GetMatchElementSprite>d__22&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.PuzzleGame.<GetAllCollectItems>d__37>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.PuzzleGame.<GetAllCollectItems>d__37&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.PuzzleGame.<GetAllItemsSprite>d__38>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.PuzzleGame.<GetAllItemsSprite>d__38&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__34<object>>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__34<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Module.ResourceModule.<LoadGameObjectAsync>d__35>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Module.ResourceModule.<LoadGameObjectAsync>d__35&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Module.UIModule.<GetUIAsyncAwait>d__43<object>>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Module.UIModule.<GetUIAsyncAwait>d__43<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Module.UIModule.<ShowUIAwaitImp>d__29>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Module.UIModule.<ShowUIAwaitImp>d__29&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixLogic.Puzzle.PuzzleGame.<GetAllCollectItems>d__37>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixLogic.Puzzle.PuzzleGame.<GetAllCollectItems>d__37&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixLogic.Puzzle.PuzzleGame.<GetAllItemsSprite>d__38>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixLogic.Puzzle.PuzzleGame.<GetAllItemsSprite>d__38&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<ElementObjectPool.<ClearAllPool>d__14>(ElementObjectPool.<ClearAllPool>d__14&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Hotfix.Logic.GameLaunch.<OpenLoginModule>d__3>(Hotfix.Logic.GameLaunch.<OpenLoginModule>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Hotfix.Logic.GameLaunch.<PreLocalization>d__2>(Hotfix.Logic.GameLaunch.<PreLocalization>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Hotfix.Logic.GameLaunch.<Preload>d__1>(Hotfix.Logic.GameLaunch.<Preload>d__1&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Hotfix.Logic.GameLaunch.<Start>d__0>(Hotfix.Logic.GameLaunch.<Start>d__0&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixCore.MVC.MVCManager.<ActiveModule>d__6>(HotfixCore.MVC.MVCManager.<ActiveModule>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixCore.MVC.MVCManager.<ExecuteModuleActivation>d__12>(HotfixCore.MVC.MVCManager.<ExecuteModuleActivation>d__12&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixCore.MVC.MVCManager.<ProcessActivationPolicy>d__11>(HotfixCore.MVC.MVCManager.<ProcessActivationPolicy>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixCore.MVC.MVCModule.<Activate>d__29>(HotfixCore.MVC.MVCModule.<Activate>d__29&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixCore.MVC.MVCModule.<ActiveMainWindow>d__35>(HotfixCore.MVC.MVCModule.<ActiveMainWindow>d__35&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixCore.MVC.MVCModule.<Sleep>d__30>(HotfixCore.MVC.MVCModule.<Sleep>d__30&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixCore.Module.HttpModule.<GetWebRequest>d__19>(HotfixCore.Module.HttpModule.<GetWebRequest>d__19&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixCore.Module.HttpModule.<PostWebRequest>d__18>(HotfixCore.Module.HttpModule.<PostWebRequest>d__18&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixCore.Module.ResourceHandler.<TryWaitingLoading>d__11>(HotfixCore.Module.ResourceHandler.<TryWaitingLoading>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixCore.Module.ResourceModule.<TryWaitingLoading>d__40>(HotfixCore.Module.ResourceModule.<TryWaitingLoading>d__40&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixCore.Module.ResourceModule.<UnloadAllUnusedAssets>d__24>(HotfixCore.Module.ResourceModule.<UnloadAllUnusedAssets>d__24&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.CollectElementItem.<<DoDestroy>b__3_0>d>(HotfixLogic.Match.CollectElementItem.<<DoDestroy>b__3_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.ElementBase.<PlayBrokenEff>d__16>(HotfixLogic.Match.ElementBase.<PlayBrokenEff>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.GridItem.<CreateSelf>d__9>(HotfixLogic.Match.GridItem.<CreateSelf>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.GridItem.<InitElement>d__19>(HotfixLogic.Match.GridItem.<InitElement>d__19&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.GridSystem.<>c__DisplayClass22_0.<<OnMatchRemainStep>b__0>d>(HotfixLogic.Match.GridSystem.<>c__DisplayClass22_0.<<OnMatchRemainStep>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.GridSystem.<DoDelElement>d__55>(HotfixLogic.Match.GridSystem.<DoDelElement>d__55&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.GridSystem.<DoRocketDelElement>d__56>(HotfixLogic.Match.GridSystem.<DoRocketDelElement>d__56&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.GridSystem.<FillInitialGrids>d__46>(HotfixLogic.Match.GridSystem.<FillInitialGrids>d__46&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.GridSystem.<OnDestroyTargetListElement>d__54>(HotfixLogic.Match.GridSystem.<OnDestroyTargetListElement>d__54&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.GridSystem.<StartDropElement>d__60>(HotfixLogic.Match.GridSystem.<StartDropElement>d__60&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.GridSystem.<StartMatch>d__26>(HotfixLogic.Match.GridSystem.<StartMatch>d__26&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.RockElementItem.<>c__DisplayClass1_0.<<DoDestroy>b__0>d>(HotfixLogic.Match.RockElementItem.<>c__DisplayClass1_0.<<DoDestroy>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Match.ValidateSquare.<CheckSpecialElement>d__7>(HotfixLogic.Match.ValidateSquare.<CheckSpecialElement>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.MatchBeginPanel.<SetLevel>d__16>(HotfixLogic.MatchBeginPanel.<SetLevel>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.MatchController.<<OnReduceMoveStep>b__19_0>d>(HotfixLogic.MatchController.<<OnReduceMoveStep>b__19_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.MatchData.<OnInitialized>d__12>(HotfixLogic.MatchData.<OnInitialized>d__12&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.MatchResultLose.<SetLevel>d__37>(HotfixLogic.MatchResultLose.<SetLevel>d__37&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.MatchResultWin.<UpdateGoals>d__28>(HotfixLogic.MatchResultWin.<UpdateGoals>d__28&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Puzzle.AnimItem.<UpdateAnim>d__15>(HotfixLogic.Puzzle.AnimItem.<UpdateAnim>d__15&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Puzzle.AnimItem.<UpdateSprite>d__16>(HotfixLogic.Puzzle.AnimItem.<UpdateSprite>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Puzzle.PuzzleBG.<LoadBG>d__7>(HotfixLogic.Puzzle.PuzzleBG.<LoadBG>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Puzzle.PuzzleGame.<InitPSDPosData>d__31>(HotfixLogic.Puzzle.PuzzleGame.<InitPSDPosData>d__31&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Puzzle.PuzzleGame.<LoadAllCollectItems>d__36>(HotfixLogic.Puzzle.PuzzleGame.<LoadAllCollectItems>d__36&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Puzzle.PuzzleGame.<LoadBackGround>d__32>(HotfixLogic.Puzzle.PuzzleGame.<LoadBackGround>d__32&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Puzzle.PuzzleGame.<LoadCollectItem>d__35>(HotfixLogic.Puzzle.PuzzleGame.<LoadCollectItem>d__35&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Puzzle.PuzzleGame.<LoadPSDData>d__33>(HotfixLogic.Puzzle.PuzzleGame.<LoadPSDData>d__33&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Puzzle.PuzzleManager.<LoadMapData>d__58>(HotfixLogic.Puzzle.PuzzleManager.<LoadMapData>d__58&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.Puzzle.PuzzleManager.<LoadPuzzleLevelData>d__57>(HotfixLogic.Puzzle.PuzzleManager.<LoadPuzzleLevelData>d__57&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.PuzzleWindow.<LoadBoosterButton>d__19>(HotfixLogic.PuzzleWindow.<LoadBoosterButton>d__19&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.PuzzleWindow.<LoadCollectTarget>d__18>(HotfixLogic.PuzzleWindow.<LoadCollectTarget>d__18&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<HotfixLogic.PuzzleWindow.<LoadPuzzleReward>d__20>(HotfixLogic.PuzzleWindow.<LoadPuzzleReward>d__20&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.Start<HotfixLogic.Match.ValidateLine.<CheckSpecialElement>d__3>(HotfixLogic.Match.ValidateLine.<CheckSpecialElement>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixCore.MVC.MVCModule.<OpenChildWindow>d__32<object>>(HotfixCore.MVC.MVCModule.<OpenChildWindow>d__32<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixCore.Module.GameItemModule.<GetItemSprite>d__22>(HotfixCore.Module.GameItemModule.<GetItemSprite>d__22&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixCore.Module.GameItemModule.<GetItemSprite>d__23>(HotfixCore.Module.GameItemModule.<GetItemSprite>d__23&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__34<object>>(HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__34<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixCore.Module.ResourceModule.<LoadGameObjectAsync>d__35>(HotfixCore.Module.ResourceModule.<LoadGameObjectAsync>d__35&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixCore.Module.UIBase.<CreateWidgetByPathAsync>d__57<object>>(HotfixCore.Module.UIBase.<CreateWidgetByPathAsync>d__57<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixCore.Module.UIBase.<CreateWidgetByTypeAsync>d__59<object>>(HotfixCore.Module.UIBase.<CreateWidgetByTypeAsync>d__59<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixCore.Module.UIModule.<GetUIAsyncAwait>d__43<object>>(HotfixCore.Module.UIModule.<GetUIAsyncAwait>d__43<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__26<object>>(HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__26<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__27>(HotfixCore.Module.UIModule.<ShowUIAsyncAwait>d__27&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixCore.Module.UIModule.<ShowUIAwaitImp>d__29>(HotfixCore.Module.UIModule.<ShowUIAwaitImp>d__29&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixLogic.Match.LevelManager.<GetLevel>d__3>(HotfixLogic.Match.LevelManager.<GetLevel>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixLogic.Match.LevelManager.<GetLevel>d__4>(HotfixLogic.Match.LevelManager.<GetLevel>d__4&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixLogic.Match.MatchManager.<GetMatchElementSprite>d__22>(HotfixLogic.Match.MatchManager.<GetMatchElementSprite>d__22&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixLogic.Puzzle.PuzzleGame.<GetAllCollectItems>d__37>(HotfixLogic.Puzzle.PuzzleGame.<GetAllCollectItems>d__37&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<HotfixLogic.Puzzle.PuzzleGame.<GetAllItemsSprite>d__38>(HotfixLogic.Puzzle.PuzzleGame.<GetAllItemsSprite>d__38&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.Module.ResourceHandler.<SetAssetByResources>d__16<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.Module.ResourceHandler.<SetAssetByResources>d__16<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__33<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__33<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixCore.Module.UIWindow.<InternalLoad>d__65>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixCore.Module.UIWindow.<InternalLoad>d__65&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Module.ResourceModule.<InvokeProgress>d__37>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Module.ResourceModule.<InvokeProgress>d__37&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__33<object>>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__33<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Module.SceneModule.<InvokeProgress>d__13>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Module.SceneModule.<InvokeProgress>d__13&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Module.UIModule.<>c__DisplayClass44_0.<<GetUIAsync>g__GetUIAsyncImp|0>d<object>>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Module.UIModule.<>c__DisplayClass44_0.<<GetUIAsync>g__GetUIAsyncImp|0>d<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Utils.UnityUtil.<AddFixedUpdateListenerImp>d__11>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Utils.UnityUtil.<AddFixedUpdateListenerImp>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Utils.UnityUtil.<AddLateUpdateListenerImp>d__13>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Utils.UnityUtil.<AddLateUpdateListenerImp>d__13&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Utils.UnityUtil.<AddUpdateListenerImp>d__9>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Utils.UnityUtil.<AddUpdateListenerImp>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotfixCore.Module.ResourceHandler.<SetAssetByResources>d__16<object>>(HotfixCore.Module.ResourceHandler.<SetAssetByResources>d__16<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotfixCore.Module.ResourceModule.<InvokeProgress>d__37>(HotfixCore.Module.ResourceModule.<InvokeProgress>d__37&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__33<object>>(HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__33<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotfixCore.Module.SceneModule.<InvokeProgress>d__13>(HotfixCore.Module.SceneModule.<InvokeProgress>d__13&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotfixCore.Module.UIModule.<>c__DisplayClass44_0.<<GetUIAsync>g__GetUIAsyncImp|0>d<object>>(HotfixCore.Module.UIModule.<>c__DisplayClass44_0.<<GetUIAsync>g__GetUIAsyncImp|0>d<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotfixCore.Module.UIWindow.<InternalLoad>d__65>(HotfixCore.Module.UIWindow.<InternalLoad>d__65&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotfixCore.Utils.UnityUtil.<AddFixedUpdateListenerImp>d__11>(HotfixCore.Utils.UnityUtil.<AddFixedUpdateListenerImp>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotfixCore.Utils.UnityUtil.<AddLateUpdateListenerImp>d__13>(HotfixCore.Utils.UnityUtil.<AddLateUpdateListenerImp>d__13&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotfixCore.Utils.UnityUtil.<AddUpdateListenerImp>d__9>(HotfixCore.Utils.UnityUtil.<AddUpdateListenerImp>d__9&)
		// System.Void Cysharp.Threading.Tasks.Internal.ArrayPoolUtil.EnsureCapacity<Cysharp.Threading.Tasks.UniTask<object>>(Cysharp.Threading.Tasks.UniTask<object>[]&,int,Cysharp.Threading.Tasks.Internal.ArrayPool<Cysharp.Threading.Tasks.UniTask<object>>)
		// System.Void Cysharp.Threading.Tasks.Internal.ArrayPoolUtil.EnsureCapacityCore<Cysharp.Threading.Tasks.UniTask<object>>(Cysharp.Threading.Tasks.UniTask<object>[]&,int,Cysharp.Threading.Tasks.Internal.ArrayPool<Cysharp.Threading.Tasks.UniTask<object>>)
		// Cysharp.Threading.Tasks.Internal.ArrayPoolUtil.RentArray<Cysharp.Threading.Tasks.UniTask<object>> Cysharp.Threading.Tasks.Internal.ArrayPoolUtil.Materialize<Cysharp.Threading.Tasks.UniTask<object>>(System.Collections.Generic.IEnumerable<Cysharp.Threading.Tasks.UniTask<object>>)
		// bool Cysharp.Threading.Tasks.Internal.RuntimeHelpersAbstraction.IsWellKnownNoReferenceContainsType<Cysharp.Threading.Tasks.UniTask<object>>()
		// Cysharp.Threading.Tasks.UniTask<object[]> Cysharp.Threading.Tasks.UniTask.WhenAll<object>(System.Collections.Generic.IEnumerable<Cysharp.Threading.Tasks.UniTask<object>>)
		// object DG.Tweening.TweenSettingsExtensions.OnComplete<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.SetAutoKill<object>(object)
		// object DG.Tweening.TweenSettingsExtensions.SetAutoKill<object>(object,bool)
		// object DG.Tweening.TweenSettingsExtensions.SetEase<object>(object,DG.Tweening.Ease)
		// object DG.Tweening.TweenSettingsExtensions.SetEase<object>(object,DG.Tweening.Ease,float,float)
		// object DG.Tweening.TweenSettingsExtensions.SetLoops<object>(object,int,DG.Tweening.LoopType)
		// object GameCore.LitJson.JsonMapper.ToObject<object>(string)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string,Newtonsoft.Json.JsonSerializerSettings)
		// object System.Activator.CreateInstance<object>()
		// Cysharp.Threading.Tasks.UniTask<object>[] System.Array.Empty<Cysharp.Threading.Tasks.UniTask<object>>()
		// object[] System.Array.Empty<object>()
		// System.Void System.Array.Fill<float>(float[],float)
		// int System.Collections.Generic.CollectionExtensions.GetValueOrDefault<int,int>(System.Collections.Generic.IReadOnlyDictionary<int,int>,int,int)
		// int System.HashCode.Combine<int,UnityEngine.Vector2Int>(int,UnityEngine.Vector2Int)
		// bool System.Linq.Enumerable.Any<UnityEngine.EventSystems.RaycastResult>(System.Collections.Generic.IEnumerable<UnityEngine.EventSystems.RaycastResult>,System.Func<UnityEngine.EventSystems.RaycastResult,bool>)
		// int System.Linq.Enumerable.Count<int>(System.Collections.Generic.IEnumerable<int>)
		// int System.Linq.Enumerable.Count<object>(System.Collections.Generic.IEnumerable<object>)
		// object System.Linq.Enumerable.First<object>(System.Collections.Generic.IEnumerable<object>)
		// object System.Linq.Enumerable.First<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// UnityEngine.Rect System.Linq.Enumerable.Last<UnityEngine.Rect>(System.Collections.Generic.IEnumerable<UnityEngine.Rect>)
		// object System.Linq.Enumerable.Last<object>(System.Collections.Generic.IEnumerable<object>)
		// int System.Linq.Enumerable.Max<UnityEngine.Vector2Int>(System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>,System.Func<UnityEngine.Vector2Int,int>)
		// int System.Linq.Enumerable.Min<UnityEngine.Vector2Int>(System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>,System.Func<UnityEngine.Vector2Int,int>)
		// System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>> System.Linq.Enumerable.OrderBy<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>,int>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>,System.Func<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>,int>)
		// System.Linq.IOrderedEnumerable<UnityEngine.Vector2Int> System.Linq.Enumerable.OrderBy<UnityEngine.Vector2Int,float>(System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>,System.Func<UnityEngine.Vector2Int,float>)
		// System.Linq.IOrderedEnumerable<UnityEngine.Vector2Int> System.Linq.Enumerable.OrderBy<UnityEngine.Vector2Int,int>(System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>,System.Func<UnityEngine.Vector2Int,int>)
		// System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>> System.Linq.Enumerable.OrderByDescending<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>,int>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>,System.Func<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>,int>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<System.Collections.Generic.KeyValuePair<object,object>,int>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>,System.Func<System.Collections.Generic.KeyValuePair<object,object>,int>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<UnityEngine.Vector2Int,int>(System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>,System.Func<UnityEngine.Vector2Int,int>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// int System.Linq.Enumerable.Sum<System.Collections.Generic.KeyValuePair<object,object>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>,System.Func<System.Collections.Generic.KeyValuePair<object,object>,int>)
		// int System.Linq.Enumerable.Sum<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int> System.Linq.Enumerable.Take<UnityEngine.Vector2Int>(System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>,int)
		// System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int> System.Linq.Enumerable.TakeIterator<UnityEngine.Vector2Int>(System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>,int)
		// System.Linq.IOrderedEnumerable<UnityEngine.Vector2Int> System.Linq.Enumerable.ThenBy<UnityEngine.Vector2Int,int>(System.Linq.IOrderedEnumerable<UnityEngine.Vector2Int>,System.Func<UnityEngine.Vector2Int,int>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>> System.Linq.Enumerable.ToList<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int,object>>)
		// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,object>> System.Linq.Enumerable.ToList<System.Collections.Generic.KeyValuePair<object,object>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>)
		// System.Collections.Generic.List<UnityEngine.Vector2Int> System.Linq.Enumerable.ToList<UnityEngine.Vector2Int>(System.Collections.Generic.IEnumerable<UnityEngine.Vector2Int>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<object,object>>.Select<int>(System.Func<System.Collections.Generic.KeyValuePair<object,object>,int>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Iterator<UnityEngine.Vector2Int>.Select<int>(System.Func<UnityEngine.Vector2Int,int>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Iterator<object>.Select<int>(System.Func<object,int>)
		// System.Linq.IOrderedEnumerable<UnityEngine.Vector2Int> System.Linq.IOrderedEnumerable<UnityEngine.Vector2Int>.CreateOrderedEnumerable<int>(System.Func<UnityEngine.Vector2Int,int>,System.Collections.Generic.IComparer<int>,bool)
		// object System.Reflection.CustomAttributeExtensions.GetCustomAttribute<object>(System.Reflection.MemberInfo,bool)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__36>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__36&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Match.ValidateSquare.<Validate>d__6>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Match.ValidateSquare.<Validate>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Puzzle.PuzzleGame.<LoadLevel>d__30>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Puzzle.PuzzleGame.<LoadLevel>d__30&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.Puzzle.PuzzleManager.<LoadData>d__55>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.Puzzle.PuzzleManager.<LoadData>d__55&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotfixLogic.PuzzleWindow.<LoadLevel>d__17>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotfixLogic.PuzzleWindow.<LoadLevel>d__17&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,HotfixLogic.Match.ValidateLine.<Validate>d__2>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,HotfixLogic.Match.ValidateLine.<Validate>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.CollectTarget.<LoadSprite>d__11>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.CollectTarget.<LoadSprite>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,HotfixLogic.Puzzle.PuzzleTips.<ShowTips>d__17>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,HotfixLogic.Puzzle.PuzzleTips.<ShowTips>d__17&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__36>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__36&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__36>(HotfixCore.Module.ResourceModule.<LoadAssetAsync>d__36&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotfixLogic.CollectTarget.<LoadSprite>d__11>(HotfixLogic.CollectTarget.<LoadSprite>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotfixLogic.Match.ValidateLine.<Validate>d__2>(HotfixLogic.Match.ValidateLine.<Validate>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotfixLogic.Match.ValidateSquare.<Validate>d__6>(HotfixLogic.Match.ValidateSquare.<Validate>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotfixLogic.Puzzle.PuzzleGame.<LoadLevel>d__30>(HotfixLogic.Puzzle.PuzzleGame.<LoadLevel>d__30&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotfixLogic.Puzzle.PuzzleManager.<LoadData>d__55>(HotfixLogic.Puzzle.PuzzleManager.<LoadData>d__55&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotfixLogic.Puzzle.PuzzleTips.<ShowTips>d__17>(HotfixLogic.Puzzle.PuzzleTips.<ShowTips>d__17&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotfixLogic.PuzzleWindow.<LoadLevel>d__17>(HotfixLogic.PuzzleWindow.<LoadLevel>d__17&)
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// System.Tuple<double,int,object> System.Tuple.Create<double,int,object>(double,int,object)
		// object UnityEngine.Component.GetComponent<object>()
		// System.Void UnityEngine.Component.GetComponents<object>(System.Collections.Generic.List<object>)
		// object[] UnityEngine.Component.GetComponentsInChildren<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.Component.TryGetComponent<object>(object&)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
		// object UnityEngine.GameObject.GetComponentInParent<object>()
		// object UnityEngine.GameObject.GetComponentInParent<object>(bool)
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.GameObject.TryGetComponent<object>(object&)
		// object UnityEngine.Object.FindObjectOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// UnityEngine.ResourceRequest UnityEngine.Resources.LoadAsync<object>(string)
		// YooAsset.AssetHandle YooAsset.ResourcePackage.LoadAssetAsync<object>(string,uint)
		// YooAsset.AssetHandle YooAsset.YooAssets.LoadAssetAsync<object>(string,uint)
	}
}