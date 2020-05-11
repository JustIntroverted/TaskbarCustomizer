using System;
using TaskbarCustomizer.Helpers;

namespace TaskbarCustomizer.Taskbars.Elements {

    public class TaskbarElement {
        private string _elementClassName = string.Empty;
        
        private IntPtr _handle { get; set; }

        private void SetHandle(IntPtr value) {
            _handle = value;
        }

        public IntPtr GetHandle() {
            return _handle;
        }

        public Utility.AccentPolicy AccentPolicy = new Utility.AccentPolicy();


        public int GetTop() {
            return GetRectangle().Top;
        }

        public int GetWidth() {
            return GetRectangle().Right - GetRectangle().Left;
        }

        public int GetHeight() {
            return GetRectangle().Bottom - GetRectangle().Top;
        }

        public TaskbarElement(string ClassName) : this(ClassName, null) {
        }

        public TaskbarElement(string ClassName, string WindowTitle) {
            SetHandle(Utility.FindWindow(ClassName, WindowTitle));
        }

        public TaskbarElement(TaskbarElement Parent, string ClassName, int ElementIndex) {
            _elementClassName = ClassName;
            SetHandle(Utility.FindWindowByIndex(Parent.GetHandle(), _elementClassName, ElementIndex));
        }

        public void ApplyAccentPolicy() {
            IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(AccentPolicy));
            System.Runtime.InteropServices.Marshal.StructureToPtr(AccentPolicy, ptr, false);

            Utility.WindowCompositionAttributeData data = new Utility.WindowCompositionAttributeData() {
                Attribute = Utility.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                Data = ptr,
                SizeOfData = System.Runtime.InteropServices.Marshal.SizeOf(AccentPolicy)
            };

            Utility.SetWindowCompositionAttribute(GetHandle(), ref data);

            System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
        }

        public void ResizeElement(int width) {
            Utility.SetWindowPos(GetHandle(), 0, 0, 0, width, GetHeight(), Utility.SWP_NOMOVE | Utility.SWP_NOACTIVATE);
        }

        public void MoveElement(int x) {
            MoveElement(x, 0);
        }

        public void MoveElement(int x, int y) {
            Utility.SetWindowPos(GetHandle(), 0, x, y, 0, 0, Utility.SWP_NOSIZE | Utility.SWP_NOACTIVATE);
        }

        public bool IsElementVisible() {
            return Utility.IsWindowVisible(GetHandle());
        }

        public void ToggleElementVisibility() {
            if (Utility.IsWindowVisible(GetHandle()))
                Utility.ShowWindow(GetHandle(), Utility.SW_HIDE);
            else
                Utility.ShowWindow(GetHandle(), Utility.SW_SHOW);
        }

        public void HideElement() {
            Utility.ShowWindow(GetHandle(), Utility.SW_HIDE);
        }

        public void ShowElement() {
            Utility.ShowWindow(GetHandle(), Utility.SW_SHOW);
        }

        private Utility.RECT GetRectangle() {
            Utility.GetWindowRect(GetHandle(), out Utility.RECT _rect);

            return _rect;
        }
    }
}