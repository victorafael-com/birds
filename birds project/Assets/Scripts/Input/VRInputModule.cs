// Decompiled with JetBrains decompiler
// Type: UnityEngine.EventSystems.VRInputModule
// Assembly: UnityEngine.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5AD10460-EE01-4AB1-8025-0F9170A9DD60
// Assembly location: /Applications/Unity/Unity.app/Contents/UnityExtensions/Unity/GUISystem/UnityEngine.UI.dll

using System;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems
{
	
    /// <summary>
    ///   <para>A BaseInputModule designed for mouse  keyboard  controller input.</para>
    /// </summary>
    [AddComponentMenu("Event/Standalone Input Module")]
    public class VRInputModule : VRPointerInputModule
    {
		public static Vector2 CenterPos{
			get{
				#if UNITY_EDITOR
				return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
				#else
				return new Vector2(500,510);
				#endif
			}
		}

        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";
        [SerializeField]
        private string m_VerticalAxis = "Vertical";
        [SerializeField]
        private string m_SubmitButton = "Submit";
        [SerializeField]
        private string m_CancelButton = "Cancel";
        [SerializeField]
        private float m_InputActionsPerSecond = 10f;
        [SerializeField]
        private float m_RepeatDelay = 0.5f;
        private float m_PrevActionTime;
        private Vector2 m_LastMoveVector;
        private int m_ConsecutiveMoveCount;
        private Vector2 m_LastMousePosition;
        private Vector2 m_MousePosition;
        [FormerlySerializedAs("m_AllowActivationOnMobileDevice")] [SerializeField] private bool m_ForceModuleActive;

        protected VRInputModule()
        {
        }

        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        public override void UpdateModule()
        {
            this.m_LastMousePosition = this.m_MousePosition;
			this.m_MousePosition = CenterPos;
        }

        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        /// <returns>
        ///   <para>Supported.</para>
        /// </returns>
        public override bool IsModuleSupported()
        {
//            if (!this.m_ForceModuleActive && !Input.mousePresent)
//                return Input.touchSupported;
            return true;
        }

        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        /// <returns>
        ///   <para>Should activate.</para>
        /// </returns>
        public override bool ShouldActivateModule()
        {
            return true;
        }

        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        public override void ActivateModule()
        {
            base.ActivateModule();
			Vector2 pos = CenterPos;
			m_MousePosition = pos;
			m_LastMousePosition = pos;
            GameObject selectedGameObject = this.eventSystem.currentSelectedGameObject;
            if (selectedGameObject == null)
                selectedGameObject = this.eventSystem.firstSelectedGameObject;
            eventSystem.SetSelectedGameObject(selectedGameObject, this.GetBaseEventData());
        }

        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        public override void DeactivateModule()
        {
            base.DeactivateModule();
            ClearSelection();
        }

        /// <summary>
        ///   <para>See BaseInputModule.</para>
        /// </summary>
        public override void Process()
        {
            bool selectedObject = this.SendUpdateEventToSelectedObject();
            if (eventSystem.sendNavigationEvents)
            {
                if (!selectedObject)
                    selectedObject |= this.SendMoveEventToSelectedObject();
                if (!selectedObject)
                    SendSubmitEventToSelectedObject();
            }
            ProcessMouseEvent();
        }

        /// <summary>
        ///   <para>Calculate and send a submit event to the current selected object.</para>
        /// </summary>
        /// <returns>
        ///   <para>If the submit event was used by the selected object.</para>
        /// </returns>
        protected bool SendSubmitEventToSelectedObject()
        {
            if ((UnityEngine.Object) this.eventSystem.currentSelectedGameObject == (UnityEngine.Object) null)
                return false;
            BaseEventData baseEventData = this.GetBaseEventData();
            if (Input.GetButtonDown(this.m_SubmitButton))
                ExecuteEvents.Execute<ISubmitHandler>(this.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
            if (Input.GetButtonDown(this.m_CancelButton))
                ExecuteEvents.Execute<ICancelHandler>(this.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
            return baseEventData.used;
        }

        private Vector2 GetRawMoveVector()
        {
            Vector2 zero = Vector2.zero;
            zero.x = Input.GetAxisRaw(this.m_HorizontalAxis);
            zero.y = Input.GetAxisRaw(this.m_VerticalAxis);
            if (Input.GetButtonDown(this.m_HorizontalAxis))
            {
                if ((double) zero.x < 0.0)
                    zero.x = -1f;
                if ((double) zero.x > 0.0)
                    zero.x = 1f;
            }
            if (Input.GetButtonDown(this.m_VerticalAxis))
            {
                if ((double) zero.y < 0.0)
                    zero.y = -1f;
                if ((double) zero.y > 0.0)
                    zero.y = 1f;
            }
            return zero;
        }

        /// <summary>
        ///   <para>Calculate and send a move event to the current selected object.</para>
        /// </summary>
        /// <returns>
        ///   <para>If the move event was used by the selected object.</para>
        /// </returns>
        protected bool SendMoveEventToSelectedObject()
        {
            float unscaledTime = Time.unscaledTime;
            Vector2 rawMoveVector = this.GetRawMoveVector();
            if (Mathf.Approximately(rawMoveVector.x, 0.0f) && Mathf.Approximately(rawMoveVector.y, 0.0f))
            {
                this.m_ConsecutiveMoveCount = 0;
                return false;
            }
            bool flag1 = Input.GetButtonDown(this.m_HorizontalAxis) || Input.GetButtonDown(this.m_VerticalAxis);
            bool flag2 = (double) Vector2.Dot(rawMoveVector, this.m_LastMoveVector) > 0.0;
            if (!flag1)
                flag1 = !flag2 || this.m_ConsecutiveMoveCount != 1 ? (double) unscaledTime > (double) this.m_PrevActionTime + 1.0 / (double) this.m_InputActionsPerSecond : (double) unscaledTime > (double) this.m_PrevActionTime + (double) this.m_RepeatDelay;
            if (!flag1)
                return false;
            AxisEventData axisEventData = this.GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0.6f);
            if (axisEventData.moveDir != MoveDirection.None)
            {
                ExecuteEvents.Execute<IMoveHandler>(this.eventSystem.currentSelectedGameObject, (BaseEventData) axisEventData, ExecuteEvents.moveHandler);
                if (!flag2)
                    this.m_ConsecutiveMoveCount = 0;
                ++this.m_ConsecutiveMoveCount;
                this.m_PrevActionTime = unscaledTime;
                this.m_LastMoveVector = rawMoveVector;
            }
            else
                this.m_ConsecutiveMoveCount = 0;
            return axisEventData.used;
        }

        /// <summary>
        ///   <para>Iterate through all the different mouse events.</para>
        /// </summary>
        /// <param name="id">The mouse pointer Event data id to get.</param>
        protected void ProcessMouseEvent()
        {
            this.ProcessMouseEvent(0);
        }

        /// <summary>
        ///   <para>Iterate through all the different mouse events.</para>
        /// </summary>
        /// <param name="id">The mouse pointer Event data id to get.</param>
        protected void ProcessMouseEvent(int id)
        {
            VRPointerInputModule.MouseState pointerEventData = this.GetMousePointerEventData(id);
            VRPointerInputModule.MouseButtonEventData eventData = pointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
            this.ProcessMousePress(eventData);
            this.ProcessMove(eventData.buttonData);
            this.ProcessDrag(eventData.buttonData);
            this.ProcessMousePress(pointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData);
            this.ProcessDrag(pointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
            this.ProcessMousePress(pointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
            this.ProcessDrag(pointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
            if (Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
                return;
            ExecuteEvents.ExecuteHierarchy<IScrollHandler>(ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject), (BaseEventData) eventData.buttonData, ExecuteEvents.scrollHandler);
        }

        /// <summary>
        ///   <para>Send a update event to the currently selected object.</para>
        /// </summary>
        /// <returns>
        ///   <para>If the update event was used by the selected object.</para>
        /// </returns>
        protected bool SendUpdateEventToSelectedObject()
        {
            if ((UnityEngine.Object) this.eventSystem.currentSelectedGameObject == (UnityEngine.Object) null)
                return false;
            BaseEventData baseEventData = this.GetBaseEventData();
            ExecuteEvents.Execute<IUpdateSelectedHandler>(this.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
            return baseEventData.used;
        }

        protected void ProcessMousePress(MouseButtonEventData data)
        {
            PointerEventData buttonData = data.buttonData;
            GameObject gameObject1 = buttonData.pointerCurrentRaycast.gameObject;
            if (data.PressedThisFrame())
            {
                buttonData.eligibleForClick = true;
                buttonData.delta = Vector2.zero;
                buttonData.dragging = false;
                buttonData.useDragThreshold = true;
                buttonData.pressPosition = buttonData.position;
                buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
                this.DeselectIfSelectionChanged(gameObject1, (BaseEventData) buttonData);
                GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject1, (BaseEventData) buttonData, ExecuteEvents.pointerDownHandler);
                if (gameObject2 == null)
                    gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject1);
                float unscaledTime = Time.unscaledTime;
                if (gameObject2 == buttonData.lastPress)
                {
                    if ((unscaledTime - buttonData.clickTime) < 0.300000011920929)
                        ++buttonData.clickCount;
                    else
                        buttonData.clickCount = 1;
                    buttonData.clickTime = unscaledTime;
                }
                else
                    buttonData.clickCount = 1;
                buttonData.pointerPress = gameObject2;
                buttonData.rawPointerPress = gameObject1;
                buttonData.clickTime = unscaledTime;
                buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject1);
                if ((UnityEngine.Object) buttonData.pointerDrag != (UnityEngine.Object) null)
                    ExecuteEvents.Execute<IInitializePotentialDragHandler>(buttonData.pointerDrag, (BaseEventData) buttonData, ExecuteEvents.initializePotentialDrag);
            }
            if (!data.ReleasedThisFrame())
                return;
            ExecuteEvents.Execute<IPointerUpHandler>(buttonData.pointerPress, (BaseEventData) buttonData, ExecuteEvents.pointerUpHandler);
            GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject1);
            if ((UnityEngine.Object) buttonData.pointerPress == (UnityEngine.Object) eventHandler && buttonData.eligibleForClick)
                ExecuteEvents.Execute<IPointerClickHandler>(buttonData.pointerPress, (BaseEventData) buttonData, ExecuteEvents.pointerClickHandler);
            else if ((UnityEngine.Object) buttonData.pointerDrag != (UnityEngine.Object) null && buttonData.dragging)
                ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject1, (BaseEventData) buttonData, ExecuteEvents.dropHandler);
            buttonData.eligibleForClick = false;
            buttonData.pointerPress = (GameObject) null;
            buttonData.rawPointerPress = (GameObject) null;
            if ((UnityEngine.Object) buttonData.pointerDrag != (UnityEngine.Object) null && buttonData.dragging)
                ExecuteEvents.Execute<IEndDragHandler>(buttonData.pointerDrag, (BaseEventData) buttonData, ExecuteEvents.endDragHandler);
            buttonData.dragging = false;
            buttonData.pointerDrag = (GameObject) null;
            if (!((UnityEngine.Object) gameObject1 != (UnityEngine.Object) buttonData.pointerEnter))
                return;
            this.HandlePointerExitAndEnter(buttonData, (GameObject) null);
            this.HandlePointerExitAndEnter(buttonData, gameObject1);
        }
    }
}
