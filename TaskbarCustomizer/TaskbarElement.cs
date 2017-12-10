using System;
using TaskbarCustomizer.Helper;

namespace TaskbarCustomizer {

    public class TaskbarElement {
        private string ClassName = string.Empty;
        public IntPtr Handle { get; private set; }

        public int Top {
            get { return getRectangle().Top; }
        }

        public int Width {
            get { return getRectangle().Right - getRectangle().Left; }
        }

        public int Height {
            get { return getRectangle().Bottom - getRectangle().Top; }
        }

        public TaskbarElement(string ClassName) : this(ClassName, null) {
        }

        public TaskbarElement(string ClassName, string WindowTitle) {
            this.Handle = Utility.FindWindow(ClassName, WindowTitle);
        }

        public TaskbarElement(TaskbarElement Parent, string ClassName, int ElementIndex) {
            this.ClassName = ClassName;
            this.Handle = Utility.FindWindowByIndex(Parent.Handle, this.ClassName, ElementIndex);
        }

        public void ResizeElement(int width) {
            Utility.SetWindowPos(this.Handle, 0, 0, 0, width, this.Height, Utility.SWP_NOMOVE | Utility.SWP_NOACTIVATE);
        }

        public void MoveElement(int x) {
            MoveElement(x, 0);
        }

        public void MoveElement(int x, int y) {
            Utility.SetWindowPos(this.Handle, 0, x, y, 0, 0, Utility.SWP_NOSIZE | Utility.SWP_NOACTIVATE);
        }

        public bool IsElementVisible() {
            return Utility.IsWindowVisible(this.Handle);
        }

        public void ToggleElementVisibility() {
            if (Utility.IsWindowVisible(this.Handle))
                Utility.ShowWindow(this.Handle, Utility.SW_HIDE);
            else
                Utility.ShowWindow(this.Handle, Utility.SW_SHOW);
        }

        public void HideElement() {
            Utility.ShowWindow(this.Handle, Utility.SW_HIDE);
        }

        public void ShowElement() {
            Utility.ShowWindow(this.Handle, Utility.SW_SHOW);
        }

        private Utility.RECT getRectangle() {
            Utility.GetWindowRect(this.Handle, out Utility.RECT _rect);

            return _rect;
        }
    }
}