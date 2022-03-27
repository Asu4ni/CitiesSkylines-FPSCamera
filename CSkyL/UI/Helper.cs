namespace CSkyL.UI
{
    using ColossalFramework.UI;
    using ICities;
    using UnityEngine;

    public static class Helper
    {
        public static float ScreenWidth => Lang.ReadFields(_UIView)
                                                .Get<Vector2>("m_CachedScreenResolution").x;
        public static float ScreenHeight => Lang.ReadFields(_UIView)
                                                .Get<Vector2>("m_CachedScreenResolution").y;

        public static void MakeDraggable(this Element element,
                System.Action actionDragStart = null, System.Action actionDragEnd = null)
        {
            var dragComp = element._UIComp.AddUIComponent<UIDragHandleFPS>();
            dragComp.name = element._UIComp.name + "_drag";
            dragComp.SetDraggedComponent(element._UIComp, actionDragStart, actionDragEnd);
        }

        public static GameElement GetElement(string name)
            => GameElement._FromUIComponent(_UIView.FindUIComponent(name));

        public static GameElement GetElement(UIHelperBase helper)
            => GameElement._FromUIComponent((helper as UIHelper)?.self as UIScrollablePanel);

        internal static UIView _UIView => UIView.GetAView();
    }

    internal class UIDragHandleFPS : UIDragHandle
    {
        public void SetDraggedComponent(UIComponent compToDrag,
                            System.Action actionDragStart, System.Action actionDragEnd)
        {
            target = compToDrag;
            this._actionDragStart = actionDragStart; this._actionDragEnd = actionDragEnd;

            width = compToDrag.width; height = compToDrag.height;
            relativePosition = Vector3.zero;
        }

        protected override void OnMouseDown(UIMouseEventParameter eventParam)
        {
            _state = State.CouldDrag;
            base.OnMouseDown(eventParam);
        }

        protected override void OnMouseMove(UIMouseEventParameter eventParam)
        {
            if (eventParam.buttons.IsFlagSet(UIMouseButton.Left) && _state == State.CouldDrag) {
                _actionDragStart();
                _state = State.Dragging;
            }
            base.OnMouseMove(eventParam);
        }

        protected override void OnMouseUp(UIMouseEventParameter eventParam)
        {
            if (_state == State.Dragging) {
                _actionDragEnd();
                _state = State.Idle;
            }
            base.OnMouseMove(eventParam);
        }

        protected override void OnClick(UIMouseEventParameter eventParam)
        {
            if (_state == State.Dragging) eventParam.Use();
            base.OnClick(eventParam);
        }

        private System.Action _actionDragStart, _actionDragEnd;

        private enum State { Idle, CouldDrag, Dragging };
        private State _state = State.Idle;
    }
}
