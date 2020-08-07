using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeNetwork.Views.Controls
{
    public class DragCanvas : Canvas
    {
        #region Dragging

        public static readonly DependencyProperty PositionOffsetProperty =
            DependencyProperty.Register("PositionOffset", typeof(Point), typeof(DragCanvas), new PropertyMetadata(new Point(), PositionOffsetChanged));

        public Point PositionOffset
        {
            get { return (Point)GetValue(PositionOffsetProperty); }
            set { SetValue(PositionOffsetProperty, value); }
        }

        private static void PositionOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var canvas = (DragCanvas)d;
            if (e.NewValue is Point position)
            {
                canvas.MoveChildren(position.X - canvas._previousPositionOffset.X, position.Y - canvas._previousPositionOffset.Y);
            }
        }

        /// <summary>
        /// Triggered when the user clicks and moves the canvas, starting a drag
        /// </summary>
        /// <param name="sender">The dragcanvas that triggered this event</param>
        /// <param name="args">The mouseevent that triggered this event</param>
        public delegate void DragStartEventHandler(object sender, MouseEventArgs args);
        public event DragStartEventHandler DragStart;

        /// <summary>
        /// Triggered when the user drags the canvas
        /// </summary>
        /// <param name="sender">The dragcanvas that triggered this event</param>
        /// <param name="args">Contains the distance traveled since the last drag move or drag start event</param>
        public delegate void DragMoveEventHandler(object sender, DragMoveEventArgs args);
        public event DragMoveEventHandler DragMove;

        /// <summary>
        /// Triggered when the user releases the mouse and the drag stops.
        /// </summary>
        /// <param name="sender">The dragcanvas that triggered this event</param>
        /// <param name="args">Contains the total distance traveled</param>
        public delegate void DragEndEventHandler(object sender, DragMoveEventArgs args);
        public event DragEndEventHandler DragStop;

        public bool IsDraggingEnabled { get; set; } = true;

        /// <summary>
        /// Used when the mousebutton is down to check if the initial click was in this element.
        /// This is useful because we dont want to assume a drag operation when the user moves the mouse but originally clicked a different element
        /// </summary>
        private bool _userClickedThisElement;

        /// <summary>
        /// Is a drag operation currently in progress?
        /// </summary>
        private bool _dragActive;

        /// <summary>
        /// The position of the mouse (screen co-ordinate) where the mouse was clicked down.
        /// </summary>
        private Point _originScreenCoordPosition;

        /// <summary> 
        /// The position of the mouse (screen co-ordinate) when the previous DragDelta event was fired 
        /// </summary>
        private Point _previousMouseScreenPos;
        private Point _previousPositionOffset;

        /// <summary> 
        /// This event puts the control into a state where it is ready for a drag operation.
        /// </summary>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsDraggingEnabled)
            {
                _userClickedThisElement = true;

                _previousMouseScreenPos = _originScreenCoordPosition = e.GetPosition(this);
                Focus();
                CaptureMouse(); //All mouse events will now be handled by the dragcanvas
            }

            base.OnMouseLeftButtonDown(e);
        }

        /// <summary> 
        /// Trigger a dragging event when the user moves the mouse while the left mouse button is pressed
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_userClickedThisElement && !_dragActive)
            {
                _dragActive = true;
                DragStart?.Invoke(this, e);
            }

            if (_dragActive)
            {
                Point curMouseScreenPos = e.GetPosition(this);

                if (!curMouseScreenPos.Equals(_previousMouseScreenPos))
                {
                    double xDelta = curMouseScreenPos.X - _previousMouseScreenPos.X;
                    double yDelta = curMouseScreenPos.Y - _previousMouseScreenPos.Y;

                    var dragEvent = new DragMoveEventArgs(e, xDelta, yDelta);
                    DragMove?.Invoke(this, dragEvent);

                    MoveChildren(xDelta, yDelta);
                    this.PositionOffset = _previousPositionOffset;

                    _previousMouseScreenPos = curMouseScreenPos;
                }
            }

            base.OnMouseMove(e);
        }


        /// <summary>
        /// Stop dragging when the user releases the left mouse button
        /// </summary>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _userClickedThisElement = false;
            ReleaseMouseCapture(); //Stop absorbing all mouse events

            if (_dragActive)
            {
                _dragActive = false;

                Point curMouseScreenPos = e.GetPosition(this);
                double xDelta = curMouseScreenPos.X - _originScreenCoordPosition.X;
                double yDelta = curMouseScreenPos.Y - _originScreenCoordPosition.Y;

                DragStop?.Invoke(this, new DragMoveEventArgs(e, xDelta, yDelta));
            }

            base.OnMouseLeftButtonUp(e);
        }

        private void MoveChildren(double deltaX, double deltaY)
        {
            foreach (UIElement cur in Children)
            {
                double prevLeft = Canvas.GetLeft(cur);
                if (Double.IsNaN(prevLeft))
                {
                    prevLeft = 0;
                }

                double prevTop = Canvas.GetTop(cur);
                if (Double.IsNaN(prevTop))
                {
                    prevTop = 0;
                }

                Canvas.SetLeft(cur, prevLeft + (deltaX));
                Canvas.SetTop(cur, prevTop + (deltaY));
            }

            _previousPositionOffset = new Point(_previousPositionOffset.X + deltaX, _previousPositionOffset.Y + deltaY);
        }
        #endregion

        #region Zoom

        public static readonly DependencyProperty MaxWheelOffsetProperty =
            DependencyProperty.Register("MaxWheelOffset", typeof(int), typeof(DragCanvas), new FrameworkPropertyMetadata(10, MaxWheelOffsetChanged));

        public int MaxWheelOffset
        {
            get { return (int)GetValue(MaxWheelOffsetProperty); }
            set { SetValue(MaxWheelOffsetProperty, value); }
        }

        private static void MaxWheelOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var canvas = (DragCanvas)d;
            var binding = BindingOperations.GetBindingExpression(canvas, WheelOffsetProperty);
            binding?.UpdateTarget();
        }

        public static readonly DependencyProperty MinWheelOffsetProperty =
            DependencyProperty.Register("MinWheelOffset", typeof(int), typeof(DragCanvas), new FrameworkPropertyMetadata(-5, MinWheelOffsetChanged));

        public int MinWheelOffset
        {
            get { return (int)GetValue(MinWheelOffsetProperty); }
            set { SetValue(MinWheelOffsetProperty, value); }
        }

        private static void MinWheelOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var canvas = (DragCanvas)d;
            var binding = BindingOperations.GetBindingExpression(canvas, WheelOffsetProperty);
            binding?.UpdateTarget();
        }

        public static readonly DependencyProperty WheelOffsetProperty =
            DependencyProperty.Register("WheelOffset", typeof(int), typeof(DragCanvas), new FrameworkPropertyMetadata(0, WheelOffsetChanged, WheelOffsetValueCoerce));

        public int WheelOffset
        {
            get { return (int)GetValue(WheelOffsetProperty); }
            set { SetValue(WheelOffsetProperty, value); }
        }

        private static void WheelOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var canvas = (DragCanvas)d;
            if (e.NewValue is int wheelOffset && wheelOffset != canvas._wheelOffset)
            {
                canvas.SetOffset(wheelOffset, new Point(canvas.ActualWidth / 2, canvas.ActualHeight / 2));
            }
        }

        private static object WheelOffsetValueCoerce(DependencyObject d, object baseValue)
        {
            if (baseValue is int intValue)
            {
                var canvas = (DragCanvas)d;
                if (intValue < canvas.MinWheelOffset)
                {
                    return canvas.MinWheelOffset;
                }
                else if (intValue > canvas.MaxWheelOffset)
                {
                    return canvas.MaxWheelOffset;
                }
            }

            return baseValue;
        }

        public delegate void ZoomEvent(object source, ZoomEventArgs args);
        public event ZoomEvent Zoom;

        private int _wheelOffset = 0;

        private ScaleTransform _curScaleTransform = new ScaleTransform(1.0, 1.0);

        private Rect ZoomView(Rect curView, double curZoom, double newZoom, Point relZoomPoint) //curView in content space, relZoomPoint is relative to view space
        {
            double zoomModifier = curZoom / newZoom;
            Size newSize = new Size(curView.Width * zoomModifier, curView.Height * zoomModifier);

            Point zoomCenter = new Point(curView.X + (curView.Width * relZoomPoint.X), curView.Y + (curView.Height * relZoomPoint.Y));
            double newX = zoomCenter.X - (relZoomPoint.X * newSize.Width);
            double newY = zoomCenter.Y - (relZoomPoint.Y * newSize.Height);
            Point newPos = new Point(newX, newY);

            return new Rect(newPos, newSize);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;

            //Calculate new scaling factor
            if ((_wheelOffset == MinWheelOffset && e.Delta < 0) || (_wheelOffset == MaxWheelOffset && e.Delta > 0))
            {
                return;
            }

            _wheelOffset += e.Delta / 120;
            if (_wheelOffset < MinWheelOffset)
            {
                _wheelOffset = MinWheelOffset;
            }
            else if (_wheelOffset > MaxWheelOffset)
            {
                _wheelOffset = MaxWheelOffset;
            }

            Point viewSpaceMousePos = e.GetPosition(this);
            SetOffset(_wheelOffset, viewSpaceMousePos, e);
            this.WheelOffset = _wheelOffset;
        }

        private void SetOffset(int offset, Point viewSpaceMousePos, MouseWheelEventArgs e = null)
        {
            double oldScale = _curScaleTransform.ScaleX;
            double newScale = Math.Log(1 + ((offset + 6) / 10d)) * 2d;

            //Calculate current viewing window onto the content
            Point topLeftContentSpace = TranslatePoint(new Point(0, 0), Children[0]);
            Point bottomRightContentSpace = TranslatePoint(new Point(ActualWidth, ActualHeight), Children[0]);
            Rect curView = new Rect
            {
                Location = topLeftContentSpace,
                Size = new Size(bottomRightContentSpace.X - topLeftContentSpace.X, bottomRightContentSpace.Y - topLeftContentSpace.Y)
            };

            //Mouse position as a fraction of the view size
            Point relZoomPoint = new Point
            {
                X = viewSpaceMousePos.X / this.ActualWidth,
                Y = viewSpaceMousePos.Y / this.ActualHeight
            };

            //Calculate new viewing window
            Rect newView = ZoomView(curView, oldScale, newScale, relZoomPoint);

            //Calculate new content offset based on the new view
            Point newOffset = new Point(-newView.X * newScale, -newView.Y * newScale);

            //Calculate new viewing window scale
            ScaleTransform newScaleTransform = new ScaleTransform
            {
                ScaleX = newScale,
                ScaleY = newScale
            };

            var zoomEvent = new ZoomEventArgs(e, _curScaleTransform, newScaleTransform, newOffset);
            Zoom?.Invoke(this, zoomEvent);

            ApplyZoomToChildren(zoomEvent);

            _curScaleTransform = newScaleTransform;
            _wheelOffset = offset;
        }

        private void ApplyZoomToChildren(ZoomEventArgs e)
        {
            foreach (UIElement cur in this.Children)
            {
                cur.RenderTransform = e.NewScale;
                Canvas.SetLeft(cur, e.ContentOffset.X);
                Canvas.SetTop(cur, e.ContentOffset.Y);
            }

            _previousPositionOffset = new Point(e.ContentOffset.X, e.ContentOffset.Y);
        }

        internal void SetZoomLevelFor(Rect bounding)
        {
            //this.PositionOffset = bounding.Location;

            //bounding.Scale(1d/_curScaleTransform.ScaleX, 1d / _curScaleTransform.ScaleY);

            Point topLeftContentSpace = TranslatePoint(new Point(0, 0), Children[0]);
            Point bottomRightContentSpace = TranslatePoint(new Point(ActualWidth, ActualHeight), Children[0]);
            Rect curView = new Rect
            {
                Location = topLeftContentSpace,
                Size = new Size(bottomRightContentSpace.X - topLeftContentSpace.X, bottomRightContentSpace.Y - topLeftContentSpace.Y)
            };
            var zoom = curView.Width * _curScaleTransform.ScaleX / bounding.Width;
            zoom = Math.Pow(Math.E, (zoom) / 2d);
            zoom = (-1 + zoom - 60d) / 10d;
            WheelOffset = (int)zoom;
        }
        #endregion
    }

    public class DragMoveEventArgs : EventArgs
    {
        public MouseEventArgs MouseEvent { get; }
        public double DeltaX { get; }
        public double DeltaY { get; }

        public DragMoveEventArgs(MouseEventArgs mouseEvent, double deltaX, double deltaY)
        {
            this.MouseEvent = mouseEvent;
            this.DeltaX = deltaX;
            this.DeltaY = deltaY;
        }
    }

    public class ZoomEventArgs : EventArgs
    {
        public MouseEventArgs MouseEvent { get; }
        public ScaleTransform OldScaleScale { get; }
        public ScaleTransform NewScale { get; }
        public Point ContentOffset { get; }

        public ZoomEventArgs(MouseEventArgs e, ScaleTransform oldScale, ScaleTransform newScale, Point contentOffset)
        {
            this.MouseEvent = e;
            this.OldScaleScale = oldScale;
            this.NewScale = newScale;
            this.ContentOffset = contentOffset;
        }
    }
}
