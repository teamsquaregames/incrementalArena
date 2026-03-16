
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Dalak.Screenshot
{
    public static class GameViewUtils
    {
        const BindingFlags Flags = BindingFlags.Public
                                          | BindingFlags.NonPublic
                                          | BindingFlags.Instance
                                          | BindingFlags.Static
                                          | BindingFlags.FlattenHierarchy;
        
        public enum GameViewSizeType
        {
            AspectRatio,
            FixedResolution
        }

        static object s_InitialSizeObj;
        const int miscSize = 1;

        static Type s_GameViewType = Type.GetType("UnityEditor.PlayModeView,UnityEditor");
        static string s_GetGameViewFuncName = "GetMainPlayModeView";

        public static EditorWindow GetMainGameView()
        {
            var getMainGameView =
                s_GameViewType.GetMethod(s_GetGameViewFuncName, Flags);
            if (getMainGameView == null)
            {
                Debug.LogError(string.Format(
                    "Can't find the main Game View : {0} function was not found in {1} type ! Did API change ?",
                    s_GetGameViewFuncName, s_GameViewType));
                return null;
            }

            var res = getMainGameView.Invoke(null, null);
            return (EditorWindow) res;
        }

        static object Group()
        {
            var T = Type.GetType("UnityEditor.GameViewSizes,UnityEditor");
            var sizes = T.BaseType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
            var instance = sizes.GetValue(null, new object[] { });

            var currentGroup = instance.GetType()
                .GetProperty("currentGroup", Flags);
            var group = currentGroup.GetValue(instance, new object[] { });
            return group;
        }

        static object FindRecorderSizeObj(string label)
        {
            var group = Group();

            var customs = group.GetType().GetField("m_Custom", Flags)
                .GetValue(group);

            var itr = (IEnumerator) customs.GetType().GetMethod("GetEnumerator").Invoke(customs, new object[] { });
            while (itr.MoveNext())
            {
                var txt = (string) itr.Current.GetType()
                    .GetField("m_BaseText", Flags).GetValue(itr.Current);
                if (txt == label)
                    return itr.Current;
            }

            return null;
        }

        static int IndexOf(object sizeObj)
        {
            var group = Group();
            var method = group.GetType().GetMethod("IndexOf", Flags);
            var index = (int) method.Invoke(group, new[] {sizeObj});

            var builtinList = group.GetType().GetField("m_Builtin", Flags)
                .GetValue(group);

            method = builtinList.GetType().GetMethod("Contains");
            if ((bool) method.Invoke(builtinList, new[] {sizeObj}))
                return index;

            method = group.GetType().GetMethod("GetBuiltinCount");
            index += (int) method.Invoke(group, new object[] { });

            return index;
        }

        static object NewSizeObj(int width, int height, string label)
        {
            var T = Type.GetType("UnityEditor.GameViewSize,UnityEditor");
            var tt = Type.GetType("UnityEditor.GameViewSizeType,UnityEditor");

            var c = T.GetConstructor(new[] {tt, typeof(int), typeof(int), typeof(string)});
            var sizeObj = c.Invoke(new object[] {1, width, height, label});
            return sizeObj;
        }

        static object AddSize(int width, int height, string label)
        {
            var sizeObj = NewSizeObj(width, height, label);

            var group = Group();
            var obj = group.GetType().GetMethod("AddCustomSize", Flags);
            obj.Invoke(group, new[] {sizeObj});
            return sizeObj;
        }


        // API //
        public static object AddCustomSize(int width, int height, string label)
        {
            var sizeObj = FindRecorderSizeObj(label);
            if (sizeObj != null)
            {
                sizeObj.GetType().GetField("m_Width", Flags)
                    .SetValue(sizeObj, width);
                sizeObj.GetType().GetField("m_Height", Flags)
                    .SetValue(sizeObj, height);
            }
            else
            {
                sizeObj = AddSize(width, height, label);
            }

            return sizeObj;
        }

        public static void SelectSize(object size)
        {
            if (size == null)
                return;
            var index = IndexOf(size);

            var gameView = GetMainGameView();
            if (gameView == null)
                return;
            var obj = gameView.GetType()
                .GetMethod("SizeSelectionCallback", Flags);
            obj.Invoke(gameView, new[] {index, size});
        }

        public static object GetCurrentSize()
        {
            {
                var gv = GetMainGameView();
                if (gv == null)
                    return new[] {miscSize, miscSize};
                var prop = gv.GetType()
                    .GetProperty("currentGameViewSize", Flags);
                return prop.GetValue(gv, new object[] { });
            }
        }
    }
}
