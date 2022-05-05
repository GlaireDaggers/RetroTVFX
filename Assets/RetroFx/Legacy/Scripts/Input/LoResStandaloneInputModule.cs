﻿using System;

using UnityEngine.UI;

namespace UnityEngine.EventSystems
{
	[AddComponentMenu("Event/Standalone Input Module")]
	public class LoResStandaloneInputModule : PointerInputModule
	{
		public int DesiredResX = 320;
		public int DesiredResY = 240;

		public float FisheyeX = 0.05f;
		public float FisheyeY = 0.05f;

		public bool StretchToDisplay = false;
		public float DisplayAspect = 1.33f;

		private float m_NextAction;

		private Vector2 m_LastMousePosition;
		private Vector2 m_MousePosition;

		protected LoResStandaloneInputModule()
		{ }

		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public enum InputMode
		{
			Mouse,
			Buttons
		}

		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public InputMode inputMode
		{
			get { return InputMode.Mouse; }
		}

		[SerializeField]
		private string m_HorizontalAxis = "Horizontal";

		/// <summary>
		/// Name of the vertical axis for movement (if axis events are used).
		/// </summary>
		[SerializeField]
		private string m_VerticalAxis = "Vertical";

		/// <summary>
		/// Name of the submit button.
		/// </summary>
		[SerializeField]
		private string m_SubmitButton = "Submit";

		/// <summary>
		/// Name of the submit button.
		/// </summary>
		[SerializeField]
		private string m_CancelButton = "Cancel";

		[SerializeField]
		private float m_InputActionsPerSecond = 10;

		[SerializeField]
		private bool m_AllowActivationOnMobileDevice;

		public bool allowActivationOnMobileDevice
		{
			get { return m_AllowActivationOnMobileDevice; }
			set { m_AllowActivationOnMobileDevice = value; }
		}

		public float inputActionsPerSecond
		{
			get { return m_InputActionsPerSecond; }
			set { m_InputActionsPerSecond = value; }
		}

		/// <summary>
		/// Name of the horizontal axis for movement (if axis events are used).
		/// </summary>
		public string horizontalAxis
		{
			get { return m_HorizontalAxis; }
			set { m_HorizontalAxis = value; }
		}

		/// <summary>
		/// Name of the vertical axis for movement (if axis events are used).
		/// </summary>
		public string verticalAxis
		{
			get { return m_VerticalAxis; }
			set { m_VerticalAxis = value; }
		}

		public string submitButton
		{
			get { return m_SubmitButton; }
			set { m_SubmitButton = value; }
		}

		public string cancelButton
		{
			get { return m_CancelButton; }
			set { m_CancelButton = value; }
		}

		public override void UpdateModule()
		{
			m_LastMousePosition = m_MousePosition;
			m_MousePosition = Input.mousePosition;
		}

		public override bool IsModuleSupported()
		{
			// Check for mouse presence instead of whether touch is supported,
			// as you can connect mouse to a tablet and in that case we'd want
			// to use StandaloneInputModule for non-touch input events.
			return m_AllowActivationOnMobileDevice || Input.mousePresent;
		}

		public override bool ShouldActivateModule()
		{
			if (!base.ShouldActivateModule())
				return false;

			var shouldActivate = Input.GetButtonDown(m_SubmitButton);
			shouldActivate |= Input.GetButtonDown(m_CancelButton);
			shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(m_HorizontalAxis), 0.0f);
			shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(m_VerticalAxis), 0.0f);
			shouldActivate |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f;
			shouldActivate |= Input.GetMouseButtonDown(0);
			return shouldActivate;
		}

		public override void ActivateModule()
		{
			base.ActivateModule();
			m_MousePosition = Input.mousePosition;
			m_LastMousePosition = Input.mousePosition;

			var toSelect = eventSystem.currentSelectedGameObject;
			//if (toSelect == null)
			//toSelect = eventSystem.lastSelectedGameObject;
			if (toSelect == null)
				toSelect = eventSystem.firstSelectedGameObject;

			eventSystem.SetSelectedGameObject(null, GetBaseEventData());
			eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
		}

		public override void DeactivateModule()
		{
			base.DeactivateModule();
			ClearSelection();
		}

		public override void Process()
		{
			bool usedEvent = SendUpdateEventToSelectedObject();

			if (eventSystem.sendNavigationEvents)
			{
				if (!usedEvent)
					usedEvent |= SendMoveEventToSelectedObject();

				if (!usedEvent)
					SendSubmitEventToSelectedObject();
			}

			ProcessMouseEvent();
		}

		/// <summary>
		/// Process submit keys.
		/// </summary>
		private bool SendSubmitEventToSelectedObject()
		{
			if (eventSystem.currentSelectedGameObject == null)
				return false;

			var data = GetBaseEventData();
			if (Input.GetButtonDown(m_SubmitButton))
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

			if (Input.GetButtonDown(m_CancelButton))
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
			return data.used;
		}

		private bool AllowMoveEventProcessing(float time)
		{
			bool allow = Input.GetButtonDown(m_HorizontalAxis);
			allow |= Input.GetButtonDown(m_VerticalAxis);
			allow |= (time > m_NextAction);
			return allow;
		}

		private Vector2 GetRawMoveVector()
		{
			Vector2 move = Vector2.zero;
			move.x = Input.GetAxisRaw(m_HorizontalAxis);
			move.y = Input.GetAxisRaw(m_VerticalAxis);

			if (Input.GetButtonDown(m_HorizontalAxis))
			{
				if (move.x < 0)
					move.x = -1f;
				if (move.x > 0)
					move.x = 1f;
			}
			if (Input.GetButtonDown(m_VerticalAxis))
			{
				if (move.y < 0)
					move.y = -1f;
				if (move.y > 0)
					move.y = 1f;
			}
			return move;
		}

		/// <summary>
		/// Process keyboard events.
		/// </summary>
		private bool SendMoveEventToSelectedObject()
		{
			float time = Time.unscaledTime;

			if (!AllowMoveEventProcessing(time))
				return false;

			Vector2 movement = GetRawMoveVector();
			// Debug.Log(m_ProcessingEvent.rawType + " axis:" + m_AllowAxisEvents + " value:" + "(" + x + "," + y + ")");
			var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);
			if (!Mathf.Approximately(axisEventData.moveVector.x, 0f)
				|| !Mathf.Approximately(axisEventData.moveVector.y, 0f))
			{
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
			}
			m_NextAction = time + 1f / m_InputActionsPerSecond;
			return axisEventData.used;
		}

		// This is the real function we want, the two commented out lines (Input.mousePosition) are replaced with m_cursorPos (our fake mouse position, set with the public function, UpdateCursorPosition)
		private readonly MouseState m_MouseState = new MouseState();
		protected override MouseState GetMousePointerEventData()
		{
			//MouseState m = new MouseState();

			// Populate the left button...
			PointerEventData leftData;
			var created = GetPointerData(kMouseLeftId, out leftData, true);

			leftData.Reset();

			// map cursor pos to normalized 0-1 range
			Vector2 mPos = Input.mousePosition;

			float actualScreenWidth = Screen.width;
			if (!StretchToDisplay)
			{
				actualScreenWidth = DisplayAspect * Screen.height;
				mPos.x -= (Screen.width - actualScreenWidth) * 0.5f;
			}

			mPos.x /= actualScreenWidth;
			mPos.y /= (float)Screen.height;
			mPos.y = 1f - mPos.y;

			mPos.x = Mathf.Clamp01(mPos.x);
			mPos.y = Mathf.Clamp01(mPos.y);

			// transform to -1..1 range
			Vector2 coords = mPos;
			coords -= new Vector2(0.5f, 0.5f);
			coords *= 2f;

			// apply fisheye distortion
			float aspect = ((float)Screen.width / (float)Screen.height);
			if (!StretchToDisplay)
			{
				aspect = 1f / DisplayAspect;
			}

			Vector2 realCoordOffs = Vector2.zero;
			realCoordOffs.x = (coords.y * coords.y) * -FisheyeX * coords.x * 0.1f;
			realCoordOffs.y = (coords.x * coords.x) * -FisheyeY * coords.y * 0.1f * aspect;
			mPos -= realCoordOffs;

			mPos.y = 1f - mPos.y;

			// map to screen resolution
			mPos.x *= DesiredResX;
			mPos.y *= DesiredResY;

			mPos.y -= 1;

			if (created)
				//leftData.position = m_cursorPos;
				leftData.position = mPos;

			Vector2 pos = mPos;
			//Vector2 pos = m_cursorPos;
			leftData.delta = pos - leftData.position;
			leftData.position = pos;
			leftData.scrollDelta = Input.mouseScrollDelta;
			leftData.button = PointerEventData.InputButton.Left;
			eventSystem.RaycastAll(leftData, m_RaycastResultCache);
			var raycast = FindFirstRaycast(m_RaycastResultCache);
			leftData.pointerCurrentRaycast = raycast;
			m_RaycastResultCache.Clear();

			// copy the apropriate data into right and middle slots
			PointerEventData rightData;
			GetPointerData(kMouseRightId, out rightData, true);
			CopyFromTo(leftData, rightData);
			rightData.button = PointerEventData.InputButton.Right;

			PointerEventData middleData;
			GetPointerData(kMouseMiddleId, out middleData, true);
			CopyFromTo(leftData, middleData);
			middleData.button = PointerEventData.InputButton.Middle;

			m_MouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), leftData);
			m_MouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
			m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);

			return m_MouseState;
		}

		/// <summary>
		/// Process all mouse events.
		/// </summary>
		private void ProcessMouseEvent()
		{
			var mouseData = GetMousePointerEventData();

			var pressed = mouseData.AnyPressesThisFrame();
			var released = mouseData.AnyReleasesThisFrame();

			var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

			if (!UseMouse(pressed, released, leftButtonData.buttonData))
				return;

			// Process the first mouse button fully
			ProcessMousePress(leftButtonData);
			ProcessMove(leftButtonData.buttonData);
			ProcessDrag(leftButtonData.buttonData);

			// Now process right / middle clicks
			ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
			ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
			ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
			ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

			if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
			{
				var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
				ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
			}
		}

		private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
		{
			if (pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling())
				return true;

			return false;
		}

		private bool SendUpdateEventToSelectedObject()
		{
			if (eventSystem.currentSelectedGameObject == null)
				return false;

			var data = GetBaseEventData();
			ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
			return data.used;
		}

		/// <summary>
		/// Process the current mouse press.
		/// </summary>
		private void ProcessMousePress(MouseButtonEventData data)
		{
			var pointerEvent = data.buttonData;
			var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

			// PointerDown notification
			if (data.PressedThisFrame())
			{
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.useDragThreshold = true;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

				DeselectIfSelectionChanged(currentOverGo, pointerEvent);

				// search for the control that will receive the press
				// if we can't find a press handler set the press
				// handler to be what would receive a click.
				var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

				// didnt find a press handler... search for a click handler
				if (newPressed == null)
					newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

				// Debug.Log("Pressed: " + newPressed);

				float time = Time.unscaledTime;

				if (newPressed == pointerEvent.lastPress)
				{
					var diffTime = time - pointerEvent.clickTime;
					if (diffTime < 0.3f)
						++pointerEvent.clickCount;
					else
						pointerEvent.clickCount = 1;

					pointerEvent.clickTime = time;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}

				pointerEvent.pointerPress = newPressed;
				pointerEvent.rawPointerPress = currentOverGo;

				pointerEvent.clickTime = time;

				// Save the drag handler as well
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

				if (pointerEvent.pointerDrag != null)
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
			}

			// PointerUp notification
			if (data.ReleasedThisFrame())
			{
				// Debug.Log("Executing pressup on: " + pointer.pointerPress);
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

				// Debug.Log("KeyCode: " + pointer.eventData.keyCode);

				// see if we mouse up on the same element that we clicked on...
				var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

				// PointerClick and Drop events
				if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
				{
					ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
				}
				else if (pointerEvent.pointerDrag != null)
				{
					ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
				}

				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;

				if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

				pointerEvent.dragging = false;
				pointerEvent.pointerDrag = null;

				// redo pointer enter / exit to refresh state
				// so that if we moused over somethign that ignored it before
				// due to having pressed on something else
				// it now gets it.
				if (currentOverGo != pointerEvent.pointerEnter)
				{
					HandlePointerExitAndEnter(pointerEvent, null);
					HandlePointerExitAndEnter(pointerEvent, currentOverGo);
				}
			}
		}
	}
}