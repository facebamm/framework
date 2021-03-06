﻿#region MIT License

// Copyright (c) 2019 exomia - Daniel Bätz
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using Exomia.Framework.Game;

namespace Exomia.Framework.Tools
{
    /// <summary>
    ///     A timer 2. This class cannot be inherited.
    /// </summary>
    public sealed class Timer2 : IUpdateable
    {
        /// <summary>
        ///     Occurs when Enabled Changed.
        /// </summary>
        /// <inheritdoc />
        public event EventHandler EnabledChanged;

        /// <summary>
        ///     Occurs when Update Order Changed.
        /// </summary>
        /// <inheritdoc />
        public event EventHandler UpdateOrderChanged;

        /// <summary>
        ///     timer finished event.
        /// </summary>
        public event EventHandler<Timer2> TimerFinished;

        /// <summary>
        ///     timer ticked event.
        /// </summary>
        public event EventHandler<Timer2> TimerTicked;

        /// <summary>
        ///     The maximum iterations.
        /// </summary>
        private readonly uint _maxIterations;

        /// <summary>
        ///     The elapsed time.
        /// </summary>
        private float _elapsedTime;

        /// <summary>
        ///     True to enable, false to disable.
        /// </summary>
        private bool _enabled;

        /// <summary>
        ///     The update order.
        /// </summary>
        private int _updateOrder;

        /// <summary>
        ///     Gets the current iteration or 0 if maxIteration = 0.
        /// </summary>
        /// <value>
        ///     The current iteration.
        /// </value>
        public uint CurrentIteration { get; private set; }

        /// <inheritdoc />
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    EnabledChanged?.Invoke();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the timer tick(time in ms after a timer Tick occurs)
        /// </summary>
        /// <value>
        ///     The timer tick.
        /// </value>
        public float TimerTick { get; set; }

        /// <inheritdoc />
        public int UpdateOrder
        {
            get { return _updateOrder; }
            set
            {
                if (_updateOrder != value)
                {
                    _updateOrder = value;
                    UpdateOrderChanged?.Invoke();
                }
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Timer2" /> class.
        /// </summary>
        /// <param name="tick">          time in ms after a timer Tick occurs. </param>
        /// <param name="maxIterations">
        ///     (Optional) set the max iteration count for this timer or 0 for
        ///     unlimited.
        /// </param>
        public Timer2(float tick, uint maxIterations = 0)
        {
            TimerTick      = tick;
            _maxIterations = maxIterations;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Exomia.Framework.Tools.Timer2" /> class.
        /// </summary>
        /// <param name="tick">          time in ms after a timer Tick occurs. </param>
        /// <param name="tickCallback">  callback for each tick event. </param>
        /// <param name="maxIterations"> set the max iteration count for this timer or 0 for unlimited. </param>
        public Timer2(float tick, EventHandler<Timer2> tickCallback, uint maxIterations = 0)
            : this(tick, maxIterations)
        {
            TimerTicked += tickCallback;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Exomia.Framework.Tools.Timer2" /> class.
        /// </summary>
        /// <param name="tick">             time in ms after a timer Tick occurs. </param>
        /// <param name="tickCallback">     callback for each tick event. </param>
        /// <param name="finishedCallback"> callback for timer finished. </param>
        /// <param name="maxIterations">
        ///     set the max iteration count for this timer or 0 for
        ///     unlimited.
        /// </param>
        public Timer2(float tick, EventHandler<Timer2> tickCallback, EventHandler<Timer2> finishedCallback,
                      uint  maxIterations)
            : this(tick, tickCallback, maxIterations)
        {
            TimerFinished += finishedCallback;
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            if (!_enabled) { return; }

            _elapsedTime += gameTime.DeltaTimeMS;
            if (_elapsedTime >= TimerTick)
            {
                _elapsedTime -= TimerTick;
                TimerTicked?.Invoke(this);

                if (_maxIterations > 0 && ++CurrentIteration >= _maxIterations)
                {
                    _enabled = false;
                    TimerFinished?.Invoke(this);
                }
            }
        }

        /// <summary>
        ///     Resets this object.
        /// </summary>
        public void Reset()
        {
            _elapsedTime     = 0;
            CurrentIteration = 0;
            _enabled         = true;
        }
    }
}